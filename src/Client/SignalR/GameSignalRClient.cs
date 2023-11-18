using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Model;

namespace Client.SignalR;

public class GameSignalRClient
    : SignalRClientBase, IGameSignalRClient
{
    public GameSignalRClient(NavigationManager navigationManager)
        : base(navigationManager, "/gamehub")
    {
    }

    public IDisposable OnBoardChanged(Action<GameBoard> action)
        => HubConnection.On(nameof(OnBoardChanged), action);
}
