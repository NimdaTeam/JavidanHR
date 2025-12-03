using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace WebHost.Utilities;

public class RedisTicketStore : ITicketStore
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _expiration;

    public RedisTicketStore(IDistributedCache cache, TimeSpan expiration)
    {
        _cache = cache;
        _expiration = expiration;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var data = await _cache.GetAsync(key);
        return data == null ? null : Deserialize(data);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var data = Serialize(ticket);
        await _cache.SetAsync(key, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _expiration,
            SlidingExpiration = TimeSpan.FromDays(1)
        });
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    private static byte[] Serialize(AuthenticationTicket source)
    {
        return TicketSerializer.Default.Serialize(source);
    }

    private static AuthenticationTicket Deserialize(byte[] source)
    {
        return TicketSerializer.Default.Deserialize(source);
    }
}