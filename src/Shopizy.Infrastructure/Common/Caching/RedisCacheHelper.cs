using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shopizy.SharedKernel.Application.Caching;
using StackExchange.Redis;

namespace Shopizy.Infrastructure.Common.Caching;

/// <summary>
/// Helper class for interacting with Redis cache.
/// </summary>
/// <param name="connectionMultiplexer"></param>
/// <param name="logger"></param>
public class RedisCacheHelper(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheHelper> logger
) : ICacheHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new ErrorOrConverterFactory() },
        TypeInfoResolver = new PrivateSetterContractResolver(),
    };

    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly ILogger<RedisCacheHelper> _logger = logger;

    /// <summary>
    /// Gets the cached value for the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The cache result containing the value if found; otherwise, a miss result.</returns>
    public async Task<CacheResult<T>> GetAsync<T>(string key)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var data = await db.StringGetAsync(key);

            if (!data.HasValue)
            {
                return CacheResult<T>.Miss();
            }

            var value = JsonSerializer.Deserialize<T>(data.ToString(), _jsonOptions)!;
            return CacheResult<T>.Hit(value);
        }
        catch (RedisConnectionException)
        {
            _logger.RedisUnavailable(key);
            return CacheResult<T>.Miss();
        }
        catch (Exception ex)
        {
            _logger.RedisGetError(ex, key);
            return CacheResult<T>.Miss();
        }
    }

    /// <summary>
    /// Sets the specified value in the cache with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The expiration time for the cached value. If null, the value does not expire.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            if (expiration.HasValue)
            {
                await db.StringSetAsync(
                    key,
                    serializedValue,
                    expiry: expiration.Value,
                    when: When.Always,
                    flags: CommandFlags.None
                );
            }
            else
            {
                await db.StringSetAsync(
                    key,
                    serializedValue,
                    expiry: null,
                    when: When.Always,
                    flags: CommandFlags.None
                );
            }
        }
        catch (RedisConnectionException)
        {
            _logger.RedisUnavailable(key);
        }
        catch (Exception ex)
        {
            _logger.RedisSetError(ex, key);
        }
    }

    /// <summary>
    /// Removes the cached value for the specified key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    public async Task RemoveAsync(string key)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException)
        {
            _logger.RedisUnavailable(key);
        }
        catch (Exception ex)
        {
            _logger.RedisRemoveError(ex, key);
        }
    }
}
