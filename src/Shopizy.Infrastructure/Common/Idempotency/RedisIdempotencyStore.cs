using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shopizy.Application.Common.Interfaces.Services;
using StackExchange.Redis;

namespace Shopizy.Infrastructure.Common.Idempotency;

public sealed class RedisIdempotencyStore(
    IConnectionMultiplexer connection,
    ILogger<RedisIdempotencyStore> logger
) : IIdempotencyStore
{
    private const string KeyPrefix = "idempotency:";

    private static readonly Action<ILogger, string, Exception?> _redisReadUnavailable =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(RedisIdempotencyStore)),
            "Redis unavailable while reading idempotency key {Key}"
        );

    private static readonly Action<ILogger, string, Exception?> _redisStoreUnavailable =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(RedisIdempotencyStore)),
            "Redis unavailable while storing idempotency key {Key}"
        );

    public async Task<IdempotencyRecord?> TryGetAsync(
        string key,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var db = connection.GetDatabase();
            var data = await db.StringGetAsync(KeyPrefix + key);
            return data.HasValue
                ? JsonSerializer.Deserialize<IdempotencyRecord>(data.ToString())
                : null;
        }
        catch (RedisException ex)
        {
            _redisReadUnavailable(logger, key, ex);
            return null;
        }
    }

    public async Task StoreAsync(
        string key,
        IdempotencyRecord record,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var db = connection.GetDatabase();
            var serialized = JsonSerializer.Serialize(record);
            await db.StringSetAsync(KeyPrefix + key, serialized, ttl, when: When.Always);
        }
        catch (RedisException ex)
        {
            _redisStoreUnavailable(logger, key, ex);
        }
    }
}
