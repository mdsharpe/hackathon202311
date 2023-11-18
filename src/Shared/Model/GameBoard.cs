using System;

namespace Shared.Model;

public class GameBoard
{
    public Tile[][] Tiles { get; set; } = Array.Empty<Tile[]>();
    private Array TileTypes { get; set; }

    public int Height => Tiles.Select(o => o.Length).DefaultIfEmpty().Max();
    public int Width => Tiles.Length;
    public void InitializeTiles(int xSize, int ySize)
    {
        TileTypes = Enum.GetValues(typeof(Tile)).OfType<Tile>().Where(tile => tile != Tile.EmptyCell).ToArray();
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

    private Tile GenerateRandomTile()
    {
        Random rnd = new Random();
        int randomIndex = rnd.Next(0, TileTypes.Length);
        Tile randomTile = (Tile)TileTypes.GetValue(randomIndex);
        return randomTile;
    }
}
