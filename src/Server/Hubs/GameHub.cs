using Microsoft.AspNetCore.SignalR;
using Server.Services;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public class GameHub : Hub, IGameHub
{
    private readonly GameBoard _gameBoard;
    private readonly TileSmasher _tileSmasher;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        GameBoard gameBoard,
        TileSmasher tileSmasher,
        ILogger<GameHub> logger)
    {
        _gameBoard = gameBoard ?? throw new ArgumentNullException(nameof(gameBoard));
        _tileSmasher = tileSmasher ?? throw new ArgumentNullException(nameof(tileSmasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<GameBoard> GetBoard()
    {
        return Task.FromResult(_gameBoard);
    }

    public async Task Move(Coordinates sourceCoordinates, Direction direction)
    {
        if (!GetIsMoveValid(sourceCoordinates, direction))
        {
            return;
        }

        // Get relevant tile data
        var sourceTile = _gameBoard.Tiles[sourceCoordinates.X][sourceCoordinates.Y];
        var targetCoordinates = GetTargetCoordinates(sourceCoordinates, direction);
        var targetTile = _gameBoard.Tiles[targetCoordinates.X][targetCoordinates.Y];

        // Swap tiles
        _gameBoard.Tiles[sourceCoordinates.X][sourceCoordinates.Y] = targetTile;
        _gameBoard.Tiles[targetCoordinates.X][targetCoordinates.Y] = sourceTile;

        _tileSmasher.DestoryTilesIfMatched();

        await Clients.All.SendAsync(
            nameof(IGameHubClient.OnBoardChanged),
            _gameBoard);
    }

    private bool GetIsMoveValid(Coordinates coordinates, Direction direction)
    {
        if (coordinates.X == 0 && direction == Direction.Left)
        {
            return false;
        }

        if (coordinates.X == _gameBoard.Width - 1 && direction == Direction.Right)
        {
            return false;
        }

        if (coordinates.Y == 0 && direction == Direction.Down)
        {
            return false;
        }

        if (coordinates.Y == _gameBoard.Height - 1 && direction == Direction.Up)
        {
            return false;
        }

        return true;
    }

    private Coordinates GetTargetCoordinates(Coordinates sourceCoordinates, Direction direction)
    {
        if (direction == Direction.Up)
        {
            return new Coordinates(sourceCoordinates.X, sourceCoordinates.Y + 1);
        }
        else if (direction == Direction.Down)
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
