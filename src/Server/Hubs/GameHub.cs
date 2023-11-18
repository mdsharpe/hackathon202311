using Microsoft.AspNetCore.SignalR;
using Server.Services;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public class GameHub : Hub, IGameHub
{
    private readonly GameBoard _board;
    private readonly GameLogic _logic;
    private readonly DirtyTracker _dirtyTracker;

    public GameHub(
        GameBoard board,
        GameLogic logic,
        DirtyTracker dirtyTracker)
    {
        _board = board;
        _logic = logic;
        _dirtyTracker = dirtyTracker;
    }

    public Task<GameBoard> GetBoard()
    {
        _dirtyTracker.EnterReadLock();

        try
        {
            return Task.FromResult(_board);
        }
        finally
        {
            _dirtyTracker.ExitReadLock();
        }
    }

    public Task StartNewGame()
    {
        _dirtyTracker.EnterWriteLock();

        try
        {
            _logic.Init();
            _dirtyTracker.MarkAsDirty();
        }
        finally
        {
            _dirtyTracker.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task Move(Coordinates sourceCoordinates, Direction direction)
    {
        if (!GetIsMoveValid(sourceCoordinates, direction))
        {
            return Task.CompletedTask;
        }

        _dirtyTracker.EnterUpgradeableReadLock();

        try
        {
            var sourceTile = _board.Tiles[sourceCoordinates.X][sourceCoordinates.Y];
            var targetCoordinates = GetTargetCoordinates(sourceCoordinates, direction);
            var targetTile = _board.Tiles[targetCoordinates.X][targetCoordinates.Y];

            if (sourceTile.GetIsDestroyed() || targetTile.GetIsDestroyed())
            {
                return Task.CompletedTask;
            }

            _dirtyTracker.EnterWriteLock();

            try
            {
                _board.Tiles[sourceCoordinates.X][sourceCoordinates.Y] = targetTile;
                _board.Tiles[targetCoordinates.X][targetCoordinates.Y] = sourceTile;

                _logic.MarkDestroyedTiles();
                _dirtyTracker.MarkAsDirty();
            }
            finally
            {
                _dirtyTracker.ExitWriteLock();
            }
        }
        finally
        {
            _dirtyTracker.ExitUpgradeableReadLock();
        }

        return Task.CompletedTask;
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
