using StackExchange.Redis;

namespace ThreeL.Infra.Redis
{
    public interface IRedisProvider
    {
        Task<bool> KeyExistsAsync(string cacheKey);
        Task<bool> StringSetAsync(string cacheKey, string cacheValue, System.TimeSpan? expiration = null, When when = When.Always);
        Task<string> StringGetAsync(string cacheKey);
        Task<bool> HSetAsync(string cacheKey, string field, string cacheValue, When when = When.Always);
        Task<T> HGetAsync<T>(string cacheKey, string field);
        Task<bool> HExistsAsync(string cacheKey, string field);
    }
}
