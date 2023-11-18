namespace Shared.Model;

public class GameBoard
{
    public GameBoard()
    {
    }

    public List<List<Tile>> Tiles { get; set; } = new List<List<Tile>>();
}
