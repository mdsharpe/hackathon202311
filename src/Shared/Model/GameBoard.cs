using System.Collections.Immutable;

namespace Shared.Model;

public class GameBoard
{
    private static ImmutableArray<Tile> TileTypes => Enum.GetValues(typeof(Tile)).OfType<Tile>()
        .Where(tile => tile != Tile.EmptyCell)
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
                Tile randomTile = GenerateRandomTile();
                Tiles[rowIndex][columnIndex] = randomTile;
            }
        }
    }

    public Tile GenerateRandomTile()
    {
        Random rnd = new Random();
        int randomIndex = rnd.Next(0, TileTypes.Length);
        Tile randomTile = TileTypes[randomIndex];
        return randomTile;
    }
}
