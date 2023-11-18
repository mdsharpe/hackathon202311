using Shared.Model;

namespace Client;

public class GameStateWrapper
{
    private readonly GameSignalRClient _signalRClient;
    private IDisposable? _gameStateChanged;

    public GameStateWrapper(GameSignalRClient signalRClient)
    {
        _signalRClient = signalRClient;

        _signalRClient.Opened += SignalRClient_Opened;
        _signalRClient.Closed += SignalRClient_Closed;
    }

    public readonly BehaviorSubject<GameBoard?> GameBoard = new(null);

    public void Clear()
    {
        _gameStateChanged?.Dispose();
        GameBoard.OnNext(null);
    }

    private async Task SignalRClient_Opened()
    {
        _gameStateChanged?.Dispose();
        _gameStateChanged = _signalRClient.OnBoardChanged(GameBoard.OnNext);
    }

    private async Task SignalRClient_Closed(Exception? arg)
    {
        Clear();
    }
}
