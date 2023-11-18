using System.Threading;
using Shared.Model;

namespace Server.Services;

public class GameEngine : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1000);
    private readonly GameBoard _gameBoard;
    private readonly TileSmasher _tileSmasher;

    public GameEngine(
        GameBoard gameBoard,
        TileSmasher tileSmasher)
    {
        _gameBoard = gameBoard;
        _tileSmasher = tileSmasher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var taskDelay = Task.Delay(Interval, stoppingToken);

            // TODO here, do stuff we want to do the board e.g. create new tiles
            if (_gameBoard.Tiles.Length == 0)
            {
                int xSize = GlobalConstants.xSize;
                int ySize = GlobalConstants.ySize;
                _gameBoard.InitializeTiles(xSize,ySize);

                do
                {
                    for (var rowIndex = 0; rowIndex < xSize; rowIndex++)
                    {
                        for (var colIndex = 0; colIndex < ySize; colIndex++)
                        {
                            if (_gameBoard.Tiles[rowIndex][colIndex] == Tile.EmptyCell)
                            {
                                _gameBoard.Tiles[rowIndex][colIndex] = _gameBoard.GenerateRandomTile();
                            }
                        }
                    }

                    _tileSmasher.DestoryTilesIfMatched();
                }
                while (_gameBoard.Tiles.Any(tc => tc.Any(t => t == Tile.EmptyCell)));
            }

            await taskDelay;
        }
    }
}
