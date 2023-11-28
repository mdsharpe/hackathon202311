using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Shared;

namespace Client.SignalR;

public class GameSignalRClient
    : SignalRClientBase, IGameSignalRClient
{
    public GameSignalRClient(NavigationManager navigationManager)
        : base(navigationManager, "/gamehub")
    {
    }

    public async Task<GameBoard> GetGameBoard()
    {
        return await HubConnection.InvokeAsync<GameBoard>(nameof(IGameHub.GetBoard));
    }

    public async Task Move(Coordinates coordinates, Direction direction)
    {
        await HubConnection.InvokeAsync(nameof(IGameHub.Move), new Move(coordinates, direction));
    }

    public async Task StartNewGame()
    {
        await HubConnection.InvokeAsync(nameof(IGameHub.StartNewGame));
    }

    public IDisposable OnBoardChanged(Action<GameBoard> action)
        => HubConnection.On(nameof(OnBoardChanged), action);
}
