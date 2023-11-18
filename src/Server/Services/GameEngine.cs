using System.Threading;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000);
    private readonly GameBoard _gameBoard;

    public GameEngine(GameBoard gameState)
    {
        _gameBoard = gameState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var taskDelay = Task.Delay(Interval, stoppingToken);

            // TODO here, do stuff we want to do the board e.g. create new tiles

            await taskDelay;
        }
    }
}
