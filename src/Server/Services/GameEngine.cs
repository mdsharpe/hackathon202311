using Microsoft.AspNetCore.SignalR;
using Server.Hubs;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(500);

    private readonly GameBoard _board;
    private readonly IHubContext<GameHub> _hub;
    private readonly GameLogic _logic;
    private readonly DirtyTracker _dirtyTracker;

    public GameEngine(
        GameBoard board,
        IHubContext<GameHub> hub,
        GameLogic logic,
        DirtyTracker dirtyTracker)
    {
        _board = board;
        _hub = hub;
        _logic = logic;
        _dirtyTracker = dirtyTracker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logic.Init();

        while (!stoppingToken.IsCancellationRequested)
        {
            await HandleBoard();

            var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                _dirtyTracker.GameEngineDelaySkipToken);

            var taskDelay = Task.Delay(Interval, delayCancellation.Token);

            try
            {
                await taskDelay;
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private async Task HandleBoard()
    {
        _dirtyTracker.EnterUpgradeableReadLock();

        try
        {
            var tilesToCleanUp = _logic.GetDestroyedTilesToCleanUp();

            if (tilesToCleanUp.Any() || _dirtyTracker.IsDirty)
            {
                _dirtyTracker.EnterWriteLock();

                try
                {
                    if (tilesToCleanUp.Any())
                    {
                        _logic.CleanUpDestroyedTiles(tilesToCleanUp);
                        _dirtyTracker.MarkAsDirty();
                    }

                    if (_dirtyTracker.IsDirty)
                    {
                        await _hub.Clients.All.PushBoard(_board);
                        _dirtyTracker.MarkAsClean();
                    }
                }
                finally
                {
                    _dirtyTracker.ExitWriteLock();
                }
            }
        }
        finally
        {
            _dirtyTracker.ExitUpgradeableReadLock();
        }
    }
}
