using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Shopizy.Infrastructure.DependencyInjection;
using Shouldly;

namespace Shopizy.Infrastructure.UnitTests.Common.Caching;

public class SecondLevelCacheTests
{
    [Fact]
    public void AddPersistence_InTestingEnvironment_RegistersSecondLevelCacheWithMemoryProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");

        // Act
        services.AddPersistence(config, mockEnv.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var interceptor = serviceProvider.GetService<SecondLevelCacheInterceptor>();
        interceptor.ShouldNotBeNull();

        var cacheProvider = serviceProvider.GetService<IEFCacheServiceProvider>();
        cacheProvider.ShouldNotBeNull();
    }

    [Fact]
    public void AddPersistence_WithoutRedisConfigured_GracefullyFallsBackToMemoryProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["RedisCacheSettings:Endpoint"] = "", // Empty endpoint
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        // Act - should not throw ArgumentException
        services.AddPersistence(config);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var interceptor = serviceProvider.GetService<SecondLevelCacheInterceptor>();
        interceptor.ShouldNotBeNull();

        var cacheProvider = serviceProvider.GetService<IEFCacheServiceProvider>();
        cacheProvider.ShouldNotBeNull();
    }
}
