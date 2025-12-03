using StackExchange.Redis;

namespace WebHost.Utilities
{
    public class RedisService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            var status = _redis.IsConnected;
        }

        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                var db =   _redis.GetDatabase();
                var status =  await db.StringSetAsync(key, value, expiry);
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<string?> GetStringAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await  db.StringGetAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                var status = await  db.KeyDeleteAsync(key);
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
