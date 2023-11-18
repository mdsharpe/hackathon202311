using Shared.Model;

namespace Shared;

public interface IGameHub
{
    Task<GameBoard> GetBoard();
    Task StartNewGame();
    Task Move(Coordinates sourceCoordinates, Direction direction);
}
