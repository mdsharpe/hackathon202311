using Shared.Model;

namespace Shared;

public interface IGameHub
{
    Task<GameBoard> GetBoard();
    Task Move(Coordinates sourceCoordinates, Direction direction);
}
