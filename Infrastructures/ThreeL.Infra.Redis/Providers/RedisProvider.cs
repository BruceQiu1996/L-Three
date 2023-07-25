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

        public async Task<bool> HSetAsync(string cacheKey, string field, string cacheValue, When when = When.Always)
        {
            return await _redisDb.HashSetAsync(cacheKey, field, cacheValue);
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
    }
}
