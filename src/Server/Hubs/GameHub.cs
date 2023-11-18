using Shared.Model;

namespace Server.Hubs;

public class GameHub
{
    private readonly GameBoard _gameBoard;

    public GameHub(
        ILogger<GameHub> logger,
        GameBoard gameBoard)
    {
        _gameBoard = gameBoard;
    }

    public async Task MoveTile(int x, int y, Direction direction)
    {
        // TODO move tile in game board
        
        // Initialize the tiles
        int xSize = 10;
        int ySize = 15;
        _gameBoard.InitializeTiles(10, 15);
        
    }
}
