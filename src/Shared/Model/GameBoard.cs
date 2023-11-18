namespace Shared.Model;

public class GameBoard
{
    GameBoard() 
    {
    }

    public Tile[,] Tiles { get; set; } = new Tile[,] { { } };
}
