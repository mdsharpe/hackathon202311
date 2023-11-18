using Shared.Model;

namespace Shared;

public interface IGameHub
{
    Task<GameBoard> GetBoard();
    Task MoveTile(int x, int y, Direction direction);
}
