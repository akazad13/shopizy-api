using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopizy.Application.Orders.Commands.ExpirePendingOrders;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Infrastructure.Orders.Services;

/// <summary>
/// Background worker that periodically finds and cancels unpaid pending orders that have exceeded the expiration threshold.
/// </summary>
public sealed class PendingOrderExpirationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<OrderSettings> orderSettings,
    ILogger<PendingOrderExpirationWorker> logger
) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> s_expirationCheckError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(PendingOrderExpirationWorker)),
            "Error occurred during pending order expiration check."
        );

    private static readonly Action<ILogger, int, Exception?> s_ordersExpired =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(2, nameof(PendingOrderExpirationWorker)),
            "PendingOrderExpirationWorker: Expired and cancelled {Count} unpaid order(s)."
        );

    private static readonly System.Diagnostics.Metrics.Meter s_meter = new(
        "Shopizy.Workers",
        "1.0"
    );
    private static readonly System.Diagnostics.Metrics.Counter<long> s_ordersExpiredCounter =
        s_meter.CreateCounter<long>(
            "shopizy.orders.expired",
            unit: "{orders}",
            description: "Number of unpaid orders expired and cancelled."
        );

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly OrderSettings _orderSettings = orderSettings.Value;
    private readonly ILogger<PendingOrderExpirationWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(
            Math.Max(10, _orderSettings.ExpirationCheckIntervalSeconds)
        );

        using var timer = new PeriodicTimer(interval);

        while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            try
            {
                await ProcessExpiredOrdersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                s_expirationCheckError(_logger, ex);
            }
        }
    }

    private async Task ProcessExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        var thresholdUtc = DateTime.UtcNow.AddMinutes(
            -_orderSettings.PendingOrderExpirationMinutes
        );

        using var scope = _scopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

        var command = new ExpirePendingOrdersCommand(thresholdUtc);
        var result = await dispatcher.SendAsync(command, cancellationToken);

        if (!result.IsError && result.Value > 0)
        {
            s_ordersExpired(_logger, result.Value, null);
            s_ordersExpiredCounter.Add(result.Value);
        }
    }
}
