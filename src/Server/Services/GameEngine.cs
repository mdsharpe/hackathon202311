using System.Threading;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000);
    private readonly GameBoard _gameBoard;

    public GameEngine(GameBoard gameBoard)
    {
        _gameBoard = gameBoard;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var taskDelay = Task.Delay(Interval, stoppingToken);

            // TODO here, do stuff we want to do the board e.g. create new tiles
            if (_gameBoard.Tiles.Length == 0)
            {
                int xSize = 10;
                int ySize = 15;
                _gameBoard.InitializeTiles(xSize, ySize);
            } 

            await taskDelay;
        }
    }
}
