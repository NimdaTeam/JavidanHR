using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;

namespace WebHost.Utilities;

public class RedisService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IDistributedCache cache, ILogger<RedisService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // SetStringAsync: سریالایز value به byte[] (چون IDistributedCache string مستقیم نمی‌گیره)
    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry.Value;
            }

            await _cache.SetAsync(key, bytes, options);
            _logger.LogDebug("Redis Set: {Key} = {Value} (expiry: {Expiry})", key, value, expiry?.TotalMinutes);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis Set failed for key: {Key}", key);
            return false;
        }
    }

    // GetStringAsync: deserialize از byte[] به string
    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var bytes = await _cache.GetAsync(key);
            if (bytes == null || bytes.Length == 0) return null;

            var value = Encoding.UTF8.GetString(bytes);
            _logger.LogDebug("Redis Get: {Key} = {Value}", key, value);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis Get failed for key: {Key}", key);
            return null;
        }
    }

    // RemoveAsync: ساده و امن
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Redis Remove: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis Remove failed for key: {Key}", key);
            return false;
        }
    }

    // اختیاری: چک وضعیت اتصال (fallback داره)
    public bool IsConnected => _cache.GetType().Name.Contains("Redis", StringComparison.OrdinalIgnoreCase);
}