using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000);

    private readonly GameBoard _board;
    private readonly IHubContext<GameHub> _hub;
    private readonly GameLogic _logic;
    private readonly ReaderWriterLockSlim _locker;

    public GameEngine(
        GameBoard board,
        IHubContext<GameHub> hub,
        GameLogic logic,
        ReaderWriterLockSlim locker)
    {
        _board = board;
        _hub = hub;
        _logic = logic;
        _locker = locker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logic.Init();

        while (!stoppingToken.IsCancellationRequested)
        {
            var taskDelay = Task.Delay(Interval, stoppingToken);

            _locker.EnterUpgradeableReadLock();

            try
            {
                if (_board.EnumerateAll().Any(o => o.IsDestroyed))
                {
                    _locker.EnterWriteLock();

                    try
                    {
                        _logic.CleanUpDestroyedTiles();
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }

                    await _hub.Clients.All.PushBoard(_board);
                }
            }
            finally
            {
                _locker.ExitUpgradeableReadLock();
            }

            await taskDelay;
        }
    }
}
