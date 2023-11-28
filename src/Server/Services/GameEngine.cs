using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(333);

    private readonly IHubContext<GameHub> _hub;
    private readonly GameBoard _board;
    private readonly BoardLock _boardLock;
    private readonly MoveQueue _moveQueue;
    private readonly GameLogic _logic;

    public GameEngine(
        IHubContext<GameHub> hub,
        GameBoard board,
        BoardLock boardLock,
        MoveQueue moveQueue,
        GameLogic logic)
    {
        _hub = hub;
        _board = board;
        _boardLock = boardLock;
        _moveQueue = moveQueue;
        _logic = logic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logic.Init();

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = Task.Delay(Interval, stoppingToken);
            var processing = HandleBoard(stoppingToken);

            try
            {
                await Task.WhenAll(processing, delay);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private async Task HandleBoard(CancellationToken cancellationToken)
    {
        _boardLock.EnterUpgradeableReadLock();

        try
        {
            var moves = _moveQueue.DequeueAll();
            var tilesToCleanUp = _logic.GetDestroyedTilesToCleanUp();

            if (tilesToCleanUp.Any() || moves.Any())
            {
                _boardLock.EnterWriteLock();

                try
                {
                    if (tilesToCleanUp.Any())
                    {
                        _logic.CleanUpDestroyedTiles(tilesToCleanUp);
                    }

                    if (moves.Any())
                    {
                        _logic.ApplyMoves(moves);
                        _logic.MarkDestroyedTiles();
                    }

                    await _hub.Clients.All.PushBoard(_board);
                }
                finally
                {
                    _boardLock.ExitWriteLock();
                }
            }
        }
        finally
        {
            _boardLock.ExitUpgradeableReadLock();
        }
    }
}
