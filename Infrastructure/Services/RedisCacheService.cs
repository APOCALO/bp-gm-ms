using Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();

            if (!redisKeys.Any())
                return new Dictionary<string, T?>();

            var values = await _database.StringGetAsync(redisKeys);

            var result = new Dictionary<string, T?>();
            for (int i = 0; i < redisKeys.Length; i++)
            {
                var rawValue = values[i];
                var key = redisKeys[i].ToString(); // Ensure the key is converted to a string
                if (key != null) // Check for null before using the key
                {
                    if (!rawValue.IsNullOrEmpty)
                    {
                        var deserialized = JsonSerializer.Deserialize<T>(rawValue!);
                        result[key] = deserialized;
                    }
                    else
                    {
                        result[key] = default;
                    }
                }
            }

            return result;
        }

        // Example of how to use the RedisCacheService

        //public async Task SaveCustomerEventToCache(CustomerCreatedEvent customerEvent)
        //{
        //    var cacheKey = $"CustomerEvent:{customerEvent.CustomerId}";
        //    await _redisCacheService.SetAsync(cacheKey, customerEvent, TimeSpan.FromMinutes(10));
        //}

        //public async Task<CustomerCreatedEvent?> GetCustomerEventFromCache(Guid customerId)
        //{
        //    var cacheKey = $"CustomerEvent:{customerId}";
        //    return await _redisCacheService.GetAsync<CustomerCreatedEvent>(cacheKey);
        //}
    }
}
