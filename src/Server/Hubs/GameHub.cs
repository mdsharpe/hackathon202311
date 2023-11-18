using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public class GameHub : Hub<IGameHub>
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

        await Clients.All.OnBoardChanged(_gameBoard);
    }
}
