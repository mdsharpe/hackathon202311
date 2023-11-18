
namespace Server.Services;

public class DirtyTracker
{
    private readonly ReaderWriterLockSlim _locker = new();
    private CancellationTokenSource _gameEngineDelaySkipTokenSource = new();

    public bool IsDirty { get; private set; }
    public CancellationToken GameEngineDelaySkipToken => _gameEngineDelaySkipTokenSource.Token;

    public void EnterReadLock() => _locker.EnterReadLock();
    public void ExitReadLock() => _locker.ExitReadLock();
    public void EnterWriteLock() => _locker.EnterWriteLock();
    public void ExitWriteLock() => _locker.ExitWriteLock();
    public void EnterUpgradeableReadLock() => _locker.EnterUpgradeableReadLock();
    public void ExitUpgradeableReadLock() => _locker.ExitUpgradeableReadLock();

    public void MarkAsDirty()
    {
        if (!_locker.IsWriteLockHeld)
        {
            throw new InvalidOperationException();
        }
        IsDirty = true;

        _gameEngineDelaySkipTokenSource.Cancel();
    }

    public void MarkAsClean()
    {
        if (!_locker.IsWriteLockHeld)
        {
            throw new InvalidOperationException();
        }

        IsDirty = false;

        _gameEngineDelaySkipTokenSource.Dispose();
        _gameEngineDelaySkipTokenSource = new();
    }
}
