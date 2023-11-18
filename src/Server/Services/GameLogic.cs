using System.Collections.Immutable;
using Microsoft.Extensions.Internal;
using Shared.Model;

namespace Server.Services;

public class GameLogic
{
    private const int MinimumNumberMatches = 3;
    private static readonly TimeSpan DestroyDelay = TimeSpan.FromMilliseconds(1500);

    private readonly ILogger<GameLogic> _logger;
    private readonly ISystemClock _clock;
    private readonly GameBoard _board;
    private readonly Random _rng = new();

    private static ImmutableArray<TileColour> TileColourTypes => Enum.GetValues(typeof(TileColour)).OfType<TileColour>()
        .Where(tile => tile != TileColour.EmptyCell)
        .ToImmutableArray();

    public GameLogic(
        ISystemClock clock,
        GameBoard board,
        ILogger<GameLogic> logger)
    {
        _clock = clock;
        _board = board;
        _logger = logger;
    }

    public void Init()
    {
        _board.Tiles = new Tile[GlobalConstants.xSize][];

        for (int x = 0; x < GlobalConstants.xSize; x++)
        {
            _board.Tiles[x] = new Tile[GlobalConstants.ySize];

            for (int y = 0; y < GlobalConstants.ySize; y++)
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

    public void MarkDestroyedTiles()
    {
        var tilesToDestroy = CheckDimension(checkRows: true)
            .Concat(CheckDimension(checkRows: false));

        foreach (var tile in tilesToDestroy)
        {
            _board.Tiles[tile.X][tile.Y].DestroyedAt = _clock.UtcNow;
        }
    }

    public Tile[] GetDestroyedTilesToCleanUp(bool skipDelay = false)
        => _board.EnumerateAll()
            .Where(o => o.GetIsDestroyed())
            .Where(o => skipDelay || _clock.UtcNow.Subtract(o.DestroyedAt!.Value) > DestroyDelay)
            .ToArray();

    public void CleanUpDestroyedTiles(IEnumerable<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            tile.TileColour = TileColour.EmptyCell;
            tile.DestroyedAt = null;
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
                        potentialTilesToDestroy = new List<Coordinates>(); // Clear the list so it's not added multiple times
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
                    potentialTilesToDestroy = new List<Coordinates>()
                    {
                        new Coordinates(
                            GetRow(checkRows, a, b),
                            GetColumn(checkRows, a, b))
                    };
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
