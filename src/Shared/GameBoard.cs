namespace Shared
{
    public class GameBoard
    {
        public GameBoard(List<List<Tile>> tiles)
        {
            Tiles = tiles;
        }

        List<List<Tile>> Tiles { get; set; }
    }
}
