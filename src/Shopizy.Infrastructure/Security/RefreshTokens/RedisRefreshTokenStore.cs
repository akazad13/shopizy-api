using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Shopizy.Application.Common.Interfaces.Authentication;
using Shopizy.Domain.Users.ValueObjects;
using StackExchange.Redis;

namespace Shopizy.Infrastructure.Security.RefreshTokens;

public sealed class RedisRefreshTokenStore(
    IConnectionMultiplexer connection,
    ILogger<RedisRefreshTokenStore> logger
) : IRefreshTokenStore
{
    private const string KeyPrefix = "refresh:";
    private const string UserIndexPrefix = "refresh-user:";

    private static readonly Action<ILogger, string, Exception?> _redisStoreRefreshTokenUnavailable =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(RedisRefreshTokenStore)),
            "Redis unavailable while storing refresh token for {UserId}"
        );

    private static readonly Action<ILogger, Exception?> _redisConsumeRefreshTokenUnavailable =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(2, nameof(RedisRefreshTokenStore)),
            "Redis unavailable while consuming refresh token"
        );

    private static readonly Action<ILogger, Exception?> _redisRevokeRefreshTokenUnavailable =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(3, nameof(RedisRefreshTokenStore)),
            "Redis unavailable while revoking refresh token"
        );

    private static readonly Action<
        ILogger,
        string,
        Exception?
    > _redisRevokeAllRefreshTokensUnavailable = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(4, nameof(RedisRefreshTokenStore)),
        "Redis unavailable while revoking refresh tokens for {UserId}"
    );

    public async Task StoreAsync(
        string token,
        UserId userId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(userId);
        var hash = Hash(token);
        try
        {
            var db = connection.GetDatabase();
            var batch = db.CreateBatch();
            var setTask = batch.StringSetAsync(
                KeyPrefix + hash,
                userId.Value.ToString(),
                ttl,
                when: When.Always
            );
            var indexTask = batch.SetAddAsync(UserIndexPrefix + userId.Value, hash);
            var indexExpireTask = batch.KeyExpireAsync(UserIndexPrefix + userId.Value, ttl);
            batch.Execute();
            await setTask;
            await indexTask;
            await indexExpireTask;
        }
        catch (RedisException ex)
        {
            _redisStoreRefreshTokenUnavailable(logger, userId.Value.ToString(), ex);
        }
    }

    public async Task<UserId?> ConsumeAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        var hash = Hash(token);
        try
        {
            var db = connection.GetDatabase();
            var value = await db.StringGetDeleteAsync(KeyPrefix + hash);
            if (!value.HasValue || !Guid.TryParse(value.ToString(), out var userGuid))
            {
                return null;
            }
            await db.SetRemoveAsync(UserIndexPrefix + userGuid, hash);
            return UserId.Create(userGuid);
        }
        catch (RedisException ex)
        {
            _redisConsumeRefreshTokenUnavailable(logger, ex);
            return null;
        }
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        var hash = Hash(token);
        try
        {
            var db = connection.GetDatabase();
            await db.KeyDeleteAsync(KeyPrefix + hash);
        }
        catch (RedisException ex)
        {
            _redisRevokeRefreshTokenUnavailable(logger, ex);
        }
    }

    public async Task RevokeAllForUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(userId);
        try
        {
            var db = connection.GetDatabase();
            var hashes = await db.SetMembersAsync(UserIndexPrefix + userId.Value);
            foreach (var entry in hashes)
            {
                if (entry.HasValue)
                {
                    await db.KeyDeleteAsync(KeyPrefix + entry.ToString());
                }
            }
            await db.KeyDeleteAsync(UserIndexPrefix + userId.Value);
        }
        catch (RedisException ex)
        {
            _redisRevokeAllRefreshTokensUnavailable(logger, userId.Value.ToString(), ex);
        }
    }

    private static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
