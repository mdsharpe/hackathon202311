using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Model;

namespace Server.Hubs;

public static class GameHubExtensions
{
    public static async Task PushBoard(
        this IClientProxy clients, 
        GameBoard board, 
        CancellationToken cancellationToken)
        => await clients.SendAsync(
            nameof(IGameHubClient.OnBoardChanged), 
            board,
            cancellationToken: cancellationToken);
}
