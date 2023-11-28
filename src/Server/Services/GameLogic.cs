using System.Collections.Immutable;
using Shared.Model;

namespace Server.Services;

public class GameLogic
{
    private const int MinimumNumberMatches = 3;
    private static readonly TimeSpan DestroyDelay = TimeSpan.FromMilliseconds(1000);

    private readonly TimeProvider _timeProvider;
    private readonly GameBoard _board;
    private readonly Random _rng = new();

    private static ImmutableArray<TileColour> TileColourTypes => Enum.GetValues(typeof(TileColour)).OfType<TileColour>()
        .Where(tile => tile != TileColour.EmptyCell)
        .ToImmutableArray();

    public GameLogic(
        TimeProvider timeProvider,
        GameBoard board)
    {
        _timeProvider = timeProvider;
        _board = board;
    }

    public void Init()
    {
        _board.Tiles = new Tile[GlobalConstants.SizeX][];

        for (int x = 0; x < GlobalConstants.SizeX; x++)
        {
            _board.Tiles[x] = new Tile[GlobalConstants.SizeY];

            for (int y = 0; y < GlobalConstants.SizeY; y++)
            {
                _board.Tiles[x][y] = new Tile()
                {
                    TileColour = GenerateRandomTileColour()
                };
            }
        }

        do
        {
            for (var x = 0; x < _board.Width; x++)
            {
                for (var y = 0; y < _board.Height; y++)
                {
                    if (_board.Tiles[x][y].TileColour == TileColour.EmptyCell)
                    {
                        _board.Tiles[x][y].TileColour = GenerateRandomTileColour();
                    }
                }
            }

            MarkDestroyedTiles();
            var destroyedTiles = GetDestroyedTilesToCleanUp(skipDelay: true);
            CleanUpDestroyedTiles(destroyedTiles);
        }
        while (_board.Tiles.SelectMany(t => t).Any(t => t.TileColour == TileColour.EmptyCell));
    }

    public bool GetIsMoveValid(Move move)
    {
        if (move.SourceCoordinates.X == 0 && move.Direction == Direction.Left)
        {
            return false;
        }

        if (move.SourceCoordinates.X == _board.Width - 1 && move.Direction == Direction.Right)
        {
            return false;
        }

        if (move.SourceCoordinates.Y == 0 && move.Direction == Direction.Down)
        {
            return false;
        }

        if (move.SourceCoordinates.Y == _board.Height - 1 && move.Direction == Direction.Up)
        {
            return false;
        }

        return true;
    }

    public void ApplyMoves(IEnumerable<Move> moves)
    {
        foreach (var move in moves)
        {
            ApplyMove(move);
        }
    }

    public void ApplyMove(Move move)
    {
        var sourceCoordinates = move.SourceCoordinates;
        var direction = move.Direction;

        var sourceTile = _board.Tiles[sourceCoordinates.X][sourceCoordinates.Y];
        var targetCoordinates = GetTargetCoordinates(sourceCoordinates, direction);
        var targetTile = _board.Tiles[targetCoordinates.X][targetCoordinates.Y];

        if (sourceTile.GetIsDestroyed() || targetTile.GetIsDestroyed())
        {
            return;
        }

        _board.Tiles[sourceCoordinates.X][sourceCoordinates.Y] = targetTile;
        _board.Tiles[targetCoordinates.X][targetCoordinates.Y] = sourceTile;
    }

    public void MarkDestroyedTiles()
    {
        var now = _timeProvider.GetUtcNow();

        var coordsToDestroy = CheckDimension(checkRows: true)
            .Concat(CheckDimension(checkRows: false))
            .Distinct()
            .ToArray();

        foreach (var coord in coordsToDestroy)
        {
            var tile = _board.Tiles[coord.X][coord.Y];

            if (!tile.GetIsDestroyed())
            {
                tile.DestroyedAt = now;
            }
        }
    }

    public Tile[] GetDestroyedTilesToCleanUp(bool skipDelay = false)
    {
        var now = _timeProvider.GetUtcNow();

        return _board.EnumerateAll()
            .Where(o => o.GetIsDestroyed())
            .Where(o => skipDelay || now.Subtract(o.DestroyedAt!.Value) > DestroyDelay)
            .ToArray();
    }

    public void CleanUpDestroyedTiles(IEnumerable<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            tile.TileColour = TileColour.EmptyCell;
            tile.DestroyedAt = null;
        }
    }

    private Coordinates GetTargetCoordinates(Coordinates sourceCoordinates, Direction direction)
    {
        if (direction == Direction.Up)
        {
            return new Coordinates(sourceCoordinates.X, sourceCoordinates.Y + 1);
        }
        else if (direction == Direction.Down)
        {
            return new Coordinates(sourceCoordinates.X, sourceCoordinates.Y - 1);
        }
        else if (direction == Direction.Left)
        {
            return new Coordinates(sourceCoordinates.X - 1, sourceCoordinates.Y);
        }
        else if (direction == Direction.Right)
        {
            return new Coordinates(sourceCoordinates.X + 1, sourceCoordinates.Y);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Either check each row or check each column for a string of at least three matching tiles.
    /// </summary>
    /// <param name="checkRows">If we're not checking each row, we're checking each column.</param>
    /// <returns>A non-unique list of coordinates of tiles to smash.</returns>
    private List<Coordinates> CheckDimension(bool checkRows)
    {
        var confirmedTilesToDestroyForDimension = new List<Coordinates>();

        for (var a = 0; a < GetDimensionLength(getRowLength: checkRows); a++)
        {
            // If we're checking rows, we are iterating along each column here, & vice versa
            // We need to reset once we start iterating over a new row/column
            var matchedTileColour = TileColour.EmptyCell; // Default
            var matchedTilesCounter = 0;
            var potentialTilesToDestroy = new List<Coordinates>();

            for (var b = 0; b < GetDimensionLength(getRowLength: !checkRows); b++)
            {
                var newColour = _board.Tiles[GetRow(checkRows, a, b)][GetColumn(checkRows, a, b)].TileColour;
                if (newColour == TileColour.EmptyCell)
                {
                    matchedTileColour = newColour;
                    // We don't need to delete any empty cells, they're not new matches
                    continue;
                }

                // If we're checking columns, we are iterating along each row here, & vice versa
                if (newColour == matchedTileColour)
                {
                    matchedTilesCounter++;
                    if (matchedTilesCounter < MinimumNumberMatches)
                    {
                        // Not enough tiles matched to mark them for destroyed - but there still could be on the next pass
                        potentialTilesToDestroy.Add(
                            new Coordinates(
                                GetRow(checkRows, a, b),
                                GetColumn(checkRows, a, b)));
                    }
                    else
                    {
                        confirmedTilesToDestroyForDimension.AddRange(potentialTilesToDestroy);
                        potentialTilesToDestroy = []; // Clear the list so it's not added multiple times
                        confirmedTilesToDestroyForDimension.Add(
                            new Coordinates(
                                GetRow(checkRows, a, b),
                                GetColumn(checkRows, a, b))); // Don't forget the tile we're on
                    }
                }
                else
                {
                    matchedTilesCounter = 1; // It matches with itself, so we have 1 match
                    matchedTileColour = _board.Tiles[
                        GetRow(checkRows, a, b)][
                        GetColumn(checkRows, a, b)].TileColour;
                    potentialTilesToDestroy =
                    [
                        new Coordinates(
                            GetRow(checkRows, a, b),
                            GetColumn(checkRows, a, b))
                    ];
                }
            }
        }

        return confirmedTilesToDestroyForDimension;
    }

    private TileColour GenerateRandomTileColour()
        => TileColourTypes[_rng.Next(0, TileColourTypes.Length)];

    private int GetDimensionLength(bool getRowLength)
    {
        // Assume all rows and columns are the same respective lengths
        return getRowLength ? _board.Width : _board.Height;
    }

    private static int GetRow(bool getRow, int indexDimensionA, int indexDimensionB)
        => getRow ? indexDimensionA : indexDimensionB;

    private static int GetColumn(bool getRow, int indexDimensionA, int indexDimensionB)
        => getRow ? indexDimensionB : indexDimensionA;
}
