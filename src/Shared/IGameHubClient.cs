using Shared.Model;

namespace Shared;

public interface IGameHubClient
{
    IDisposable OnBoardChanged(Action<GameBoard> action);
}
