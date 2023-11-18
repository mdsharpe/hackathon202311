using System.Threading;
using Server.Hubs;
using Shared;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000);

    private readonly GameBoard _board;
    private readonly GameHub _hub;
    private readonly GameLogic _logic;

    public GameEngine(
        GameBoard board,
        GameHub hub,
        GameLogic logic)
    {
        _board = board;
        _hub = hub;
        _logic = logic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var taskDelay = Task.Delay(Interval, stoppingToken);

            if (!_board.HasTiles())
            {
                _logic.Init();
            }

            _logic.CleanUpDestroyedTiles();

            // TODO [MS] Only update if has changes
            await _hub.UpdateClients();

            await taskDelay;
        }
    }
}
