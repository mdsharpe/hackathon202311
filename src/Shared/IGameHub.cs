using Shared.Model;

namespace Shared;

public interface IGameHub
{
    Task<GameBoard> GetBoard();
    Task MoveTile(Coordinates sourceCoordinates, Direction direction);
}
