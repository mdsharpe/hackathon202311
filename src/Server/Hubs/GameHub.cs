using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public class GameHub : Hub
{
    private readonly GameBoard _gameBoard;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        ILogger<GameHub> logger,
        GameBoard gameBoard)
    {
        _gameBoard = gameBoard;
        _logger = logger;
    }

    public async Task MoveTile(int x, int y, Direction direction)
    {
        try
        {
            DiscardInvalidMoves(x, y, direction);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation(ex.Message);
        }

        // TODO move tile in game board

        await Clients.All.SendAsync(
            nameof(IGameHubClient.OnBoardChanged),
            _gameBoard);
    }

    private void DiscardInvalidMoves(int x, int y, Direction direction)
    {
        if (x == 0 && direction == Direction.Left)
        {
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }

        if (x == _gameBoard.Tiles.GetLength(0) && direction == Direction.Right)
        {
            // Assume all rows are the same length
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }

        if (y == 0 && direction == Direction.Down)
        {
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }

        if (y == _gameBoard.Tiles.GetLength(1) && direction == Direction.Up)
        {
            // Assume all columns are the same length
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }
    }
}
