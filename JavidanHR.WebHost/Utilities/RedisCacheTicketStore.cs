using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace WebHost.Utilities;

public class RedisTicketStore : ITicketStore
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _expiration;
    private readonly ILogger<RedisTicketStore> _logger;

    // Circuit Breaker
    private const int FailureThreshold = 5;
    private const int CircuitOpenDurationSeconds = 300; // ۵ دقیقه
    private int _failureCount = 0;
    private DateTime _circuitOpenedAt = DateTime.MinValue;
    private readonly object _lock = new();

    public RedisTicketStore(IDistributedCache distributedCache, IMemoryCache memoryCache,
        ILogger<RedisTicketStore> logger, TimeSpan expiration)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _logger = logger;
        _expiration = expiration;
    }

    private bool IsCircuitOpen =>
        _failureCount >= FailureThreshold &&
        DateTime.UtcNow < _circuitOpenedAt.AddSeconds(CircuitOpenDurationSeconds);

    private void RecordSuccess() { lock (_lock) { _failureCount = Math.Max(0, _failureCount - 2); } } // سریع‌تر برگرده
    private void RecordFailure() { lock (_lock) { _failureCount++; _circuitOpenedAt = DateTime.UtcNow; } }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        // اول همیشه از MemoryCache بخون (سریع‌ترین)
        if (_memoryCache.TryGetValue(key, out byte[]? memoryData) && memoryData != null)
        {
            return Deserialize(memoryData);
        }

        // اگر در Memory نبود و Redis در دسترسه، از Redis بخون و در Memory بگذار
        if (!IsCircuitOpen)
        {
            try
            {
                var redisData = await _distributedCache.GetAsync(key);
                if (redisData != null && redisData.Length > 0)
                {
                    RecordSuccess();
                    await CacheInMemoryAsync(key, redisData);
                    return Deserialize(redisData);
                }
            }
            catch (Exception ex) when (IsRedisError(ex))
            {
                RecordFailure();
                _logger.LogWarning(ex, "Redis failed on Retrieve → Relying on MemoryCache only");
            }
        }

        return null; // تیکت پیدا نشد (کاربر واقعاً لاگ‌اوت شده)
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var data = Serialize(ticket);

        // همیشه در MemoryCache ذخیره کن (مهم‌ترین قسمت!)
        await CacheInMemoryAsync(key, data);

        // اگر Redis در دسترسه، آنجا هم ذخیره کن
        if (!IsCircuitOpen)
        {
            try
            {
                await _distributedCache.SetAsync(key, data, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _expiration,
                    SlidingExpiration = TimeSpan.FromDays(1)
                });
                RecordSuccess();
                _logger.LogDebug("Ticket renewed in Redis + Memory: {Key}", key);
            }
            catch (Exception ex) when (IsRedisError(ex))
            {
                RecordFailure();
                _logger.LogWarning(ex, "Redis failed on Renew → Ticket kept only in MemoryCache");
                // اینجا لاگ‌اوت نمی‌شه! تیکت در Memory هست
            }
        }
        else
        {
            _logger.LogDebug("Redis circuit open → Ticket stored only in MemoryCache");
        }
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);

        if (!IsCircuitOpen)
        {
            try { await _distributedCache.RemoveAsync(key); RecordSuccess(); }
            catch { RecordFailure(); }
        }
    }

    private Task CacheInMemoryAsync(string key, byte[] data)
    {
        _memoryCache.Set(key, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _expiration,
            SlidingExpiration = TimeSpan.FromDays(1)
        });
        return Task.CompletedTask;
    }

    private static bool IsRedisError(Exception ex) =>
        ex is RedisConnectionException ||
        ex is RedisTimeoutException ||
        ex.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase);

    private static byte[] Serialize(AuthenticationTicket t) => TicketSerializer.Default.Serialize(t);
    private static AuthenticationTicket Deserialize(byte[] d) => TicketSerializer.Default.Deserialize(d);
}