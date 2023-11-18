using Shared.Model;

namespace Shared;

public interface IGameHub
{
    // Client-to-server methods
    Task MoveTile(int x, int y, Direction direction);

    // Server-to-client methods
    Task OnBoardChanged(GameBoard board);
}
