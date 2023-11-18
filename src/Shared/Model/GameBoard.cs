using System;
namespace Shared.Model
{
    public class GameBoard
    {
        public GameBoard(Tile[,] tiles)
        {
            Tiles = tiles;
        }

        public Tile[][] Tiles { get; set; } = Array.Empty<Tile[]>();
        private Array TileTypes { get; set; }

        public void InitializeTiles(int xSize, int ySize)
        {
            TileTypes = Enum.GetValues(typeof(Tile));
            Tiles = new Tile[ySize][];
            for (int rowIndex = 0; rowIndex < ySize; rowIndex++)
            {
                Tiles[rowIndex] = new Tile[xSize];
                for (int columnIndex = 0; columnIndex < xSize; columnIndex++)
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
}
