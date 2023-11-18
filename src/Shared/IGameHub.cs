using Shared.Model;

namespace Shared;

public interface IGameHub
{
    Task MoveTile(int x, int y, Direction direction);
}
