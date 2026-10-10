using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Shopizy.Infrastructure.Common.HealthChecks;
using Shopizy.Infrastructure.Common.Persistence;
using Shouldly;
using StackExchange.Redis;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Common;

public class HealthChecksTests
{
    [Fact]
    public async Task RedisHealthCheck_WhenReachable_ShouldReturnHealthy()
    {
        var mockRedis = new Mock<IConnectionMultiplexer>();
        var mockDb = new Mock<IDatabase>();
        mockDb
            .Setup(d => d.PingAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(TimeSpan.FromMilliseconds(5));
        mockRedis
            .Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mockDb.Object);

        var healthCheck = new RedisHealthCheck(mockRedis.Object);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldBe("Redis is reachable.");
    }

    [Fact]
    public async Task RedisHealthCheck_WhenThrowsException_ShouldReturnUnhealthy()
    {
        var mockRedis = new Mock<IConnectionMultiplexer>();
        mockRedis
            .Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Throws(
                new RedisConnectionException(
                    ConnectionFailureType.UnableToConnect,
                    "Cannot connect"
                )
            );

        var healthCheck = new RedisHealthCheck(mockRedis.Object);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Description.ShouldBe("Redis check failed.");
    }

    [Fact]
    public async Task DbHealthCheck_WhenCanConnect_ShouldReturnHealthy()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
        );
        services.AddHttpContextAccessor();

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var healthCheck = new DbHealthCheck(scopeFactory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldBe("Database is reachable.");
    }

    [Fact]
    public async Task OutboxHealthCheck_WhenDeadLetterCountBelowThreshold_ShouldReturnHealthy()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
        );
        services.AddHttpContextAccessor();

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var healthCheck = new OutboxHealthCheck(scopeFactory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldNotBeNull();
        result.Description.ShouldContain("healthy");
    }

    [Fact]
    public async Task OutboxHealthCheck_WhenDeadLetterCountAtOrAboveThreshold_ShouldReturnDegraded()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddHttpContextAccessor();

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            for (var i = 0; i < OutboxHealthCheck.DeadLetterWarningThreshold; i++)
            {
                dbContext.OutboxMessages.Add(
                    new Shopizy.Infrastructure.Outbox.OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        OccurredOn = DateTime.UtcNow,
                        Type = "TestType",
                        Content = "{}",
                        DeadLetteredOn = DateTime.UtcNow,
                        DeadLetterReason = "Failed",
                    }
                );
            }
            await dbContext.SaveChangesAsync();
        }

        var healthCheck = new OutboxHealthCheck(scopeFactory);
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldNotBeNull();
        result.Description.ShouldContain("exceeding threshold");
    }
}
