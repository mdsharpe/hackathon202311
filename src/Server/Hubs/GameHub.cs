using Microsoft.AspNetCore.SignalR;
using Server.Services;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public class GameHub : Hub, IGameHub
{
    private readonly GameBoard _board;
    private readonly GameLogic _logic;
    private readonly ReaderWriterLockSlim _locker;

    public GameHub(
        GameBoard board,
        GameLogic logic,
        ReaderWriterLockSlim locker)
    {
        _board = board;
        _logic = logic;
        _locker = locker;
    }

    public Task<GameBoard> GetBoard()
    {
        _locker.EnterReadLock();

        try
        {
            return Task.FromResult(_board);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public async Task StartNewGame()
    {
        _locker.EnterWriteLock();

        try
        {
            _logic.Init();
            await PushBoardToClients();
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    public async Task Move(Coordinates sourceCoordinates, Direction direction)
    {
        _locker.EnterUpgradeableReadLock();

        try
        {
            if (!GetIsMoveValid(sourceCoordinates, direction))
            {
                return;
            }

            var sourceTile = _board.Tiles[sourceCoordinates.X][sourceCoordinates.Y];
            var targetCoordinates = GetTargetCoordinates(sourceCoordinates, direction);
            var targetTile = _board.Tiles[targetCoordinates.X][targetCoordinates.Y];

            if (sourceTile.IsDestroyed || targetTile.IsDestroyed)
            {
                return;
            }

            _locker.EnterWriteLock();

            try
            {
                _board.Tiles[sourceCoordinates.X][sourceCoordinates.Y] = targetTile;
                _board.Tiles[targetCoordinates.X][targetCoordinates.Y] = sourceTile;

                _logic.MarkDestroyedTiles();

                await PushBoardToClients();
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
    }

    private async Task PushBoardToClients()
    {
        await Clients.All.PushBoard(_board);
    }

    private bool GetIsMoveValid(Coordinates coordinates, Direction direction)
    {
        if (coordinates.X == 0 && direction == Direction.Left)
        {
            return false;
        }

        if (coordinates.X == _board.Width - 1 && direction == Direction.Right)
        {
            return false;
        }

        if (coordinates.Y == 0 && direction == Direction.Down)
        {
            return false;
        }

        if (coordinates.Y == _board.Height - 1 && direction == Direction.Up)
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
            throw new InvalidOperationException();
        }
    }
}
