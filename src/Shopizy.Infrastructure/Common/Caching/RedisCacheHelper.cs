using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shopizy.SharedKernel.Application.Caching;
using StackExchange.Redis;

namespace Shopizy.Infrastructure.Common.Caching;

/// <summary>
/// Helper class for interacting with Redis cache with local in-memory fallback.
/// </summary>
/// <param name="connectionMultiplexer"></param>
/// <param name="logger"></param>
/// <param name="memoryCache"></param>
public class RedisCacheHelper(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheHelper> logger,
    IMemoryCache? memoryCache = null
) : ICacheHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new ErrorOrConverterFactory() },
        TypeInfoResolver = new PrivateSetterContractResolver(),
    };

    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
    private readonly ILogger<RedisCacheHelper> _logger = logger;
    private readonly IMemoryCache? _memoryCache = memoryCache;

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
        catch (RedisException)
        {
            _logger.RedisUnavailable(key);
            return TryFallbackMemoryGet<T>(key);
        }
        catch (Exception ex)
        {
            _logger.RedisGetError(ex, key);
            return TryFallbackMemoryGet<T>(key);
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
        if (_memoryCache != null)
        {
            if (expiration.HasValue)
            {
                _memoryCache.Set(key, value, expiration.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }
        }

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await db.StringSetAsync(
                key,
                serializedValue,
                expiry: expiration,
                when: When.Always,
                flags: CommandFlags.None
            );
        }
        catch (RedisException)
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
        _memoryCache?.Remove(key);

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (RedisException)
        {
            _logger.RedisUnavailable(key);
        }
        catch (Exception ex)
        {
            _logger.RedisRemoveError(ex, key);
        }
    }

    private CacheResult<T> TryFallbackMemoryGet<T>(string key)
    {
        if (_memoryCache != null && _memoryCache.TryGetValue(key, out T? value) && value != null)
        {
            return CacheResult<T>.Hit(value);
        }

        return CacheResult<T>.Miss();
    }
}
