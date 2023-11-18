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

    public async Task MoveTile(Coordinates sourceCoordinates, Direction direction)
    {
        try
        {
            DiscardInvalidMoves(sourceCoordinates, direction);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation(ex.Message);
            return;
        }

        // Get relevant tile data
        var sourceTile = _gameBoard.Tiles[sourceCoordinates.X, sourceCoordinates.Y];
        var targetCoordinates = GetTargetCoordinates(sourceCoordinates, direction);
        var targetTile = _gameBoard.Tiles[targetCoordinates.X, targetCoordinates.Y];

        // Swap tiles
        _gameBoard.Tiles[sourceCoordinates.X,sourceCoordinates.Y] = targetTile;
        _gameBoard.Tiles[targetCoordinates.X, targetCoordinates.Y] = sourceTile;

        await Clients.All.SendAsync(
            nameof(IGameHubClient.OnBoardChanged),
            _gameBoard);
    }

    private void DiscardInvalidMoves(Coordinates coordinates, Direction direction)
    {
        if (coordinates.X == 0 && direction == Direction.Left)
        {
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }

        if (coordinates.X == _gameBoard.Tiles.GetLength(0) && direction == Direction.Right)
        {
            // Assume all rows are the same length
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }

        if (coordinates.Y == 0 && direction == Direction.Down)
        {
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }

        if (coordinates.Y == _gameBoard.Tiles.GetLength(1) && direction == Direction.Up)
        {
            // Assume all columns are the same length
            throw new InvalidOperationException($"Can't move {nameof(direction)}");
        }
    }

    private Coordinates GetTargetCoordinates(Coordinates sourceCoordinates, Direction direction)
    {
        if (direction == Direction.Up)
        {
            return new Coordinates(sourceCoordinates.X, sourceCoordinates.Y + 1);
        }
        else if(direction == Direction.Down)
        {
            return new Coordinates(sourceCoordinates.X, sourceCoordinates.Y - 1);
        }
        else if (direction == Direction.Left)
        {
            return new Coordinates(sourceCoordinates.X - 1, sourceCoordinates.Y);
        }
        else if (direction == Direction.Right)
        {
            return new Coordinates(sourceCoordinates.X + 1, sourceCoordinates.Y);
        }
        else
        {
            throw new InvalidOperationException("How?");
        }
    }
}
