using Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;

namespace Infrastructure.Services;

public class DistributedLockService : IDistributedLockService
{
    private readonly IDatabase _database;
    private readonly ILogger<DistributedLockService> _logger;

    public DistributedLockService(IConnectionMultiplexer redis, ILogger<DistributedLockService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<IDisposable?> AcquireLockAsync(string key, TimeSpan expiry, TimeSpan? waitTime = null)
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var wait = waitTime ?? TimeSpan.FromSeconds(5);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < wait)
        {
            var acquired = await _database.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);
            if (acquired)
            {
                _logger.LogDebug("Acquired lock for key {Key} with value {Value}", key, lockValue);
                return new DistributedLock(this, lockKey);
            }

            await Task.Delay(50); // Retry interval
        }

        _logger.LogWarning("Failed to acquire lock for key {Key} within {WaitTime}", key, wait);
        return null;
    }

    public async Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry, TimeSpan? waitTime = null)
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var wait = waitTime ?? TimeSpan.FromSeconds(5);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < wait)
        {
            var acquired = await _database.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);
            if (acquired)
            {
                _logger.LogDebug("Acquired lock for key {Key} with value {Value}", key, lockValue);
                return true;
            }

            await Task.Delay(50);
        }

        return false;
    }

    public async Task ReleaseLockAsync(string key)
    {
        var lockKey = $"lock:{key}";
        var script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { key });
        _logger.LogDebug("Released lock for key {Key}, result: {Result}", key, result);
    }
}
