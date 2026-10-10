using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Common.HealthChecks;

/// <summary>
/// Health check that reports degraded status if the outbox dead-letter backlog exceeds threshold.
/// </summary>
public sealed class OutboxHealthCheck(IServiceScopeFactory serviceScopeFactory) : IHealthCheck
{
    public const int DeadLetterWarningThreshold = 10;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deadLetterCount = await dbContext
                .OutboxMessages.AsNoTracking()
                .CountAsync(m => m.DeadLetteredOn != null, cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["dead_letter_count"] = deadLetterCount,
                ["threshold"] = DeadLetterWarningThreshold,
            };

            if (deadLetterCount >= DeadLetterWarningThreshold)
            {
                return HealthCheckResult.Degraded(
                    $"Outbox has {deadLetterCount} dead-lettered message(s) exceeding threshold of {DeadLetterWarningThreshold}.",
                    data: data
                );
            }

            return HealthCheckResult.Healthy(
                $"Outbox is healthy (dead-letter count: {deadLetterCount}).",
                data
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Outbox health check failed.", ex);
        }
    }
}
