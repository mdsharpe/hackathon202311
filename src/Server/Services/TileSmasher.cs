﻿using Shared.Model;

namespace Server.Services;

/// <summary>
/// Destroys tiles if necessary and makes the remaining ones fall into place.
/// </summary>
public class TileSmasher
{
    private const int _minimumNumberMatches = 3;

    private readonly GameBoard _gameBoard;
    private readonly ILogger<TileSmasher> _logger;

    public TileSmasher(
        GameBoard gameBoard,
        ILogger<TileSmasher> logger)
    {
        _gameBoard = gameBoard ?? throw new ArgumentNullException(nameof(gameBoard));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void DestoryTilesIfMatched()
    {
        var tilesToDestroy = new List<Coordinates>();

        // Check rows first
        _logger.LogInformation("Checking rows");
        tilesToDestroy.AddRange(CheckDimension(checkRows: true));

        // Now columns
        _logger.LogInformation("Checking columns");
        tilesToDestroy.AddRange(CheckDimension(checkRows: false));

        DestroyTiles(tilesToDestroy);
    }

    private List<Coordinates> CheckDimension(bool checkRows)
    {
        // Rows correspond to "0", columns to "1"
        var dimensionA = checkRows ? 0 : 1;
        var dimensionB = checkRows ? 1 : 0;

        var confirmedTilesToDestroyForDimension = new List<Coordinates>();

        for (var a = 0; a < _gameBoard.Tiles.GetLength(dimensionA); a++)
        {
            // If we're checking rows, we are iterating along each column here, & vice versa
            // We need to reset once we start a new row/column
            var matchedTile = Tile.EmptyCell; // Default
            var matchedTilesCounter = 0;
            var potentialTilesToDestroy = new List<Coordinates>();

            for (var b = 0; b < _gameBoard.Tiles.GetLength(dimensionB); b++)
            {
                // If we're checking columns, we are iterating along each row here; & vice versa
                if (_gameBoard.Tiles[GetRow(checkRows, a, b)][GetColumn(checkRows, a, b)] == matchedTile)
                {
                    matchedTilesCounter++;
                    if (matchedTilesCounter < _minimumNumberMatches)
                    {
                        // Not enough tiles matched to mark them for destroyed - but there still could be on the next pass
                        potentialTilesToDestroy.Add(
                            new Coordinates(
                                GetRow(checkRows, a, b),
                                GetColumn(checkRows, a, b)));
                    }
                    else
                    {
                        _logger.LogInformation("Tiles to destroy found");
                        confirmedTilesToDestroyForDimension.AddRange(potentialTilesToDestroy);
                        potentialTilesToDestroy = new List<Coordinates>(); // Clear the list so it's not added multiple times
                        confirmedTilesToDestroyForDimension.Add(
                            new Coordinates(
                                GetRow(checkRows, a, b),
                                GetColumn(checkRows, a, b))); // Don't forget the tile we're on
                    }
                }
                else
                {
                    _logger.LogInformation("No match found");
                    matchedTilesCounter = 1; // It matches with itself, so we have 1 match
                    matchedTile = _gameBoard.Tiles[
                        GetRow(checkRows, a, b)][
                        GetColumn(checkRows, a, b)];
                    potentialTilesToDestroy = new List<Coordinates>() 
                    {
                        new Coordinates(
                            GetRow(checkRows, a, b),
                            GetColumn(checkRows, a, b))
                    };
                }
            }
        }

        return confirmedTilesToDestroyForDimension;
    }

    private static int GetRow(bool getRow, int indexDimensionA, int indexDimensionB)
        => getRow ? indexDimensionA : indexDimensionB;

    private static int GetColumn(bool getRow, int indexDimensionA, int indexDimensionB)
        => getRow ? indexDimensionB : indexDimensionA;

    private void DestroyTiles(List<Coordinates> tilesToDestroy)
    {
        foreach(var tile in tilesToDestroy)
        {
            _logger.LogInformation($"Destorying tile at ({tile.X}, {tile.Y})");
            _gameBoard.Tiles[tile.X][tile.Y] = Tile.EmptyCell;
        }
    }
}