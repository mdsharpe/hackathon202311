using System.Threading;
using Server.Hubs;
using Shared;
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

            if (_gameBoard.Tiles.Length == 0)
            {
                int xSize = GlobalConstants.xSize;
                int ySize = GlobalConstants.ySize;
                _gameBoard.InitializeTiles(xSize, ySize);

                EnsureNoMatchesOnInitialization(xSize, ySize);
            }

            CleanUpDestroyedTiles();

            await taskDelay;
        }
    }

    private void EnsureNoMatchesOnInitialization(int xSize, int ySize)
    {
        do
        {
            for (var rowIndex = 0; rowIndex < xSize; rowIndex++)
            {
                for (var colIndex = 0; colIndex < ySize; colIndex++)
                {
                    if (_gameBoard.Tiles[rowIndex][colIndex].TileColour == TileColour.EmptyCell)
                    {
                        _gameBoard.Tiles[rowIndex][colIndex].TileColour = _gameBoard.GenerateRandomTileColour();
                    }
                }
            }

            _tileSmasher.GetMatchedTiles();
            CleanUpDestroyedTiles();
        }
        while (_gameBoard.Tiles.Any(tc => tc.Any(t => t.TileColour == TileColour.EmptyCell)));
    }

    private void CleanUpDestroyedTiles()
    {
        while (_gameBoard.Tiles.Any(tc => tc.Any(t => t.IsDestroyed)))
        {
            for (var rowIndex = 0; rowIndex < _gameBoard.Tiles.Length; rowIndex++)
            {
                for (var colIndex = 0; colIndex < _gameBoard.Tiles[0].Length; colIndex++)
                {
                    if (_gameBoard.Tiles[rowIndex][colIndex].IsDestroyed)
                    {
                        _gameBoard.Tiles[rowIndex][colIndex].TileColour = TileColour.EmptyCell;
                        _gameBoard.Tiles[rowIndex][colIndex].IsDestroyed = false;
                    }
                }
            }
        }
    }
}
