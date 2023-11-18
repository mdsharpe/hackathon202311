using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.SignalR;

// Based on https://blog.hagoodit.com/2021/08/28/strongly-typed-signal-r-client-and-server/
public abstract class SignalRClientBase
    : ISignalRClient, IAsyncDisposable
{
    protected SignalRClientBase(NavigationManager navigationManager, string hubPath)
    {
        HubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri(hubPath))
            .WithAutomaticReconnect()
            ////.AddMessagePackProtocol()
            .Build();

        HubConnection.Closed += async (arg) =>
        {
            if (Closed is not null)
            {
                await Closed.Invoke(arg);
            }
        };
    }

    public bool IsConnected => HubConnection.State == HubConnectionState.Connected;

    public event Func<Task>? Opened;
    public event Func<Exception?, Task>? Closed;

    protected HubConnection HubConnection { get; private set; }

    public async ValueTask DisposeAsync()
    {
        if (HubConnection != null)
        {
            await HubConnection.DisposeAsync();
        }
    }

    public async Task Start()
    {
        await HubConnection.StartAsync();

        if (Opened is not null)
        {
            await Opened.Invoke();
        }
    }

    public async Task Stop()
    {
        await HubConnection.StopAsync();
    }
}
