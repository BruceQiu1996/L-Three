using StackExchange.Redis;
using System.Text.Json;
using ThreeL.Infra.Core.Serializer;

namespace ThreeL.Infra.Redis.Providers
{
    public class RedisProvider : IRedisProvider
    {
        private readonly IEnumerable<IServer> _servers;
        private readonly IDatabase _redisDb;
        private readonly JsonSerializerOptions _jsonOptions = SystemTextJson.GetAdncDefaultOptions();

        public RedisProvider(DefaultDatabaseProvider dbProvider)
        {
            this._redisDb = dbProvider.GetDatabase();
            this._servers = dbProvider.GetServerList();
        }

        public async Task<bool> HExistsAsync(string cacheKey, string field)
        {
            return await _redisDb.HashExistsAsync(cacheKey, field);
        }

        public async Task<bool> HSetAsync<T>(string cacheKey, string cacheValue, T value, TimeSpan? expiration = null, When when = When.Always)
        {
            var data = JsonSerializer.Serialize(value, _jsonOptions);
            await _redisDb.HashSetAsync(cacheKey, cacheValue, data);
            if (expiration != null)
            {
                await _redisDb.KeyExpireAsync(cacheKey, expiration);
            }

            return true;
        }

        public async Task<T> HGetAsync<T>(string cacheKey, string field)
        {
            var val = await _redisDb.HashGetAsync(cacheKey, field);
            if (val.HasValue)
            {
                return JsonSerializer.Deserialize<T>(val, _jsonOptions);
            }
            else
            {
                return default;
            }
        }

        public async Task<bool> KeyDelAsync(string key)
        {
            return await _redisDb.KeyDeleteAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string cacheKey)
        {
            var flag = await _redisDb.KeyExistsAsync(cacheKey);
            return flag;
        }

        public async Task<string> StringGetAsync(string cacheKey)
        {
            return await _redisDb.StringGetAsync(cacheKey);
        }

        public async Task<bool> StringSetAsync(string cacheKey, string cacheValue, TimeSpan? expiration = null, When when = When.Always)
        {
            bool flag = await _redisDb.StringSetAsync(cacheKey, cacheValue, expiration, when);
            return flag;
        }

        public async Task<Dictionary<string, T>> HGetAllAsync<T>(string cacheKey)
        {
            var dict = new Dictionary<string, T>();
            var vals = await _redisDb.HashGetAllAsync(cacheKey);

            foreach (var item in vals)
            {
                if (!dict.ContainsKey(item.Name))
                    dict.Add(item.Name, JsonSerializer.Deserialize<T>(item.Value, _jsonOptions));
            }

            return dict;
        }

        public async Task SetAddAsync(string cacheKey, string[] cacheValues, TimeSpan? expiration = null)
        {
            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(item);
            }

            await _redisDb.SetAddAsync(cacheKey, list.ToArray());

            if (expiration.HasValue)
            {
                _redisDb.KeyExpire(cacheKey, expiration.Value);
            }
        }

        public async Task<bool> SetIsMemberAsync(string cacheKey, string cacheValue)
        {
            return await _redisDb.SetContainsAsync(cacheKey, cacheValue);
        }
    }
}
