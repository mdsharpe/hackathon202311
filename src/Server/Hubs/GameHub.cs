using Microsoft.AspNetCore.SignalR;
using Server.Services;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public class GameHub : Hub, IGameHub
{
    private readonly GameBoard _board;
    private readonly BoardLock _boardLock;
    private readonly GameLogic _logic;
    private readonly MoveQueue _moveQueue;

    public GameHub(
        GameBoard board,
        BoardLock boardLock,
        GameLogic logic,
        MoveQueue moveQueue)
    {
        _board = board;
        _logic = logic;
        _boardLock = boardLock;
        _moveQueue = moveQueue;
    }

    public Task<GameBoard> GetBoard()
    {
        _boardLock.EnterReadLock();

        try
        {
            return Task.FromResult(_board);
        }
        finally
        {
            _boardLock.ExitReadLock();
        }
    }

    public Task StartNewGame()
    {
        _boardLock.EnterWriteLock();

        try
        {
            _logic.Init();
        }
        finally
        {
            _boardLock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task Move(Move move)
    {
        if (_logic.GetIsMoveValid(move))
        {
            _moveQueue.Enqueue(move);
        }

        return Task.CompletedTask;
    }
}
