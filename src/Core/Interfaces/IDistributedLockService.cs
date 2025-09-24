namespace Core.Interfaces;

public interface IDistributedLockService
{
    Task<IDisposable?> AcquireLockAsync(string key, TimeSpan expiry, TimeSpan? waitTime = null);
    Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry, TimeSpan? waitTime = null);
    Task ReleaseLockAsync(string key);
}

public class DistributedLock : IDisposable
{
    private readonly IDistributedLockService _lockService;
    private readonly string _key;
    private bool _disposed = false;

    public DistributedLock(IDistributedLockService lockService, string key)
    {
        _lockService = lockService;
        _key = key;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _lockService.ReleaseLockAsync(_key).ConfigureAwait(false);
            _disposed = true;
        }
    }
}
