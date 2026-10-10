using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shopizy.Infrastructure.Common.Caching;
using Shouldly;
using StackExchange.Redis;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Common.Caching;

public class RedisCacheHelperTests
{
    private readonly Mock<IConnectionMultiplexer> _mockMultiplexer;
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<ILogger<RedisCacheHelper>> _mockLogger;
    private readonly RedisCacheHelper _cacheHelper;

    public RedisCacheHelperTests()
    {
        _mockMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();
        _mockLogger = new Mock<ILogger<RedisCacheHelper>>();

        _mockMultiplexer
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);

        _cacheHelper = new RedisCacheHelper(_mockMultiplexer.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-key";
        var value = new TestDto { Name = "Test" };
        var serializedValue = JsonSerializer.Serialize(value);
        _mockDatabase
            .Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheHelper.GetAsync<TestDto>(key);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Test");
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Arrange
        _mockDatabase
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _cacheHelper.GetAsync<TestDto>("missing");

        // Assert
        result.Success.ShouldBeFalse();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldCallStringSetAsync()
    {
        // Arrange
        var key = "set-key";
        var value = new TestDto { Name = "Set" };

        _mockDatabase
            .Setup(db =>
                db.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<bool>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                )
            )
            .ReturnsAsync(true);

        _mockDatabase
            .Setup(db =>
                db.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                )
            )
            .ReturnsAsync(true);

        // Act
        await _cacheHelper.SetAsync(key, value);

        // Assert
        // Explicitly cast to IDatabaseAsync to ensure we're verifying the interface member correctly
        var databaseAsync = _mockDatabase.As<IDatabaseAsync>();

        try
        {
            databaseAsync.Verify(
                db =>
                    db.StringSetAsync(
                        It.IsAny<RedisKey>(),
                        It.IsAny<RedisValue>(),
                        It.IsAny<TimeSpan?>(),
                        It.IsAny<bool>(),
                        It.IsAny<When>(),
                        It.IsAny<CommandFlags>()
                    ),
                Times.Once
            );
        }
        catch
        {
            databaseAsync.Verify(
                db =>
                    db.StringSetAsync(
                        It.IsAny<RedisKey>(),
                        It.IsAny<RedisValue>(),
                        It.IsAny<TimeSpan?>(),
                        It.IsAny<When>(),
                        It.IsAny<CommandFlags>()
                    ),
                Times.Once
            );
        }
    }

    [Fact]
    public async Task GetAsync_WhenRedisFailsAndMemoryCacheHasValue_ShouldReturnFallbackValue()
    {
        // Arrange
        var key = "fallback-key";
        var expected = new TestDto { Name = "Fallback" };
        using var memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(
            new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()
        );
        memoryCache.Set(key, expected);

        _mockDatabase
            .Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(
                new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis down")
            );

        var helperWithMemory = new RedisCacheHelper(
            _mockMultiplexer.Object,
            _mockLogger.Object,
            memoryCache
        );

        // Act
        var result = await helperWithMemory.GetAsync<TestDto>(key);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Fallback");
    }

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
