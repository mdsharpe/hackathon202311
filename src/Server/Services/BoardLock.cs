
namespace Server.Services;

public class BoardLock
{
    private readonly ReaderWriterLockSlim _locker = new();

    public void EnterReadLock() => _locker.EnterReadLock();
    public void ExitReadLock() => _locker.ExitReadLock();
    public void EnterWriteLock() => _locker.EnterWriteLock();
    public void ExitWriteLock() => _locker.ExitWriteLock();
    public void EnterUpgradeableReadLock() => _locker.EnterUpgradeableReadLock();
    public void ExitUpgradeableReadLock() => _locker.ExitUpgradeableReadLock();
}
