using Newtonsoft.Json;
using StackExchange.Redis;

namespace CoreAdvanceConcepts.Caching
{
    public class RedisCacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(ConfigurationOptions options)
        {
            options.AbortOnConnectFail = false; // Retry connection attempts
            options.ConnectTimeout = 5000; // Timeout for connection attempts
            options.SyncTimeout = 5000; // Timeout for synchronous operations

            var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                    return default;
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"Redis connection exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching data from Redis: {ex.Message}");
            }
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                await _database.StringSetAsync(key, JsonConvert.SerializeObject(value), expiry);
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine($"Redis connection exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching data from Redis: {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
