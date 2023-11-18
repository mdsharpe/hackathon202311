using System.Collections.Immutable;

namespace Shared.Model;

public class GameBoard
{
    private static ImmutableArray<TileColour> TileColourTypes => Enum.GetValues(typeof(TileColour)).OfType<TileColour>()
        .Where(tile => tile != TileColour.EmptyCell)
        .ToImmutableArray();

    public Tile[][] Tiles { get; set; } = Array.Empty<Tile[]>();

    public int Height => Tiles.Select(o => o.Length).DefaultIfEmpty().Max();
    public int Width => Tiles.Length;
    public void InitializeTiles(int xSize, int ySize)
    {
        Tiles = new Tile[xSize][];
        for (int rowIndex = 0; rowIndex < xSize; rowIndex++)
        {
            Tiles[rowIndex] = new Tile[ySize];
            for (int columnIndex = 0; columnIndex < ySize; columnIndex++)
            {
                Tiles[rowIndex][columnIndex] = new Tile() 
                {
                    TileColour = GenerateRandomTileColour(),
                    IsDestroyed = false,
                };
            }
        }
    }

    public TileColour GenerateRandomTileColour()
    {
        Random rnd = new Random();
        int randomIndex = rnd.Next(0, TileColourTypes.Length);
        TileColour randomTile = TileColourTypes[randomIndex];
        return randomTile;
    }

    public bool GetIsGameOver()
        => (from row in Tiles
            from tile in row
            group tile by tile into tileGroup
            let count = tileGroup.Count()
            where count > 2
            select tileGroup.Key)
        .Any();
}
