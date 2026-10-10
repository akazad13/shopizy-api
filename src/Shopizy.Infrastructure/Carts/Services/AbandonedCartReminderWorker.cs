using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopizy.Application.Carts.Commands.SendAbandonedCartReminders;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Infrastructure.Carts.Services;

/// <summary>
/// Background worker that periodically checks for abandoned shopping carts and dispatches recovery emails.
/// </summary>
public sealed class AbandonedCartReminderWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<CartSettings> cartSettings,
    ILogger<AbandonedCartReminderWorker> logger
) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> s_abandonedCartCheckError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, nameof(AbandonedCartReminderWorker)),
            "Error occurred during abandoned cart reminder processing."
        );

    private static readonly Action<ILogger, int, Exception?> s_abandonedCartRemindersSent =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(2, nameof(AbandonedCartReminderWorker)),
            "AbandonedCartReminderWorker: Sent {Count} abandoned cart recovery reminder email(s)."
        );

    private static readonly System.Diagnostics.Metrics.Meter s_meter = new(
        "Shopizy.Workers",
        "1.0"
    );
    private static readonly System.Diagnostics.Metrics.Counter<long> s_remindersSentCounter =
        s_meter.CreateCounter<long>(
            "shopizy.carts.reminders_sent",
            unit: "{reminders}",
            description: "Number of abandoned cart recovery reminder emails dispatched."
        );

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly CartSettings _cartSettings = cartSettings.Value;
    private readonly ILogger<AbandonedCartReminderWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, _cartSettings.CheckIntervalMinutes));

        using var timer = new PeriodicTimer(interval);

        while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            try
            {
                await ProcessAbandonedCartsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                s_abandonedCartCheckError(_logger, ex);
            }
        }
    }

    private async Task ProcessAbandonedCartsAsync(CancellationToken cancellationToken)
    {
        var inactiveBeforeUtc = DateTime.UtcNow.AddHours(
            -_cartSettings.AbandonedCartInactivityHours
        );

        using var scope = _scopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

        var command = new SendAbandonedCartRemindersCommand(inactiveBeforeUtc);
        var result = await dispatcher.SendAsync(command, cancellationToken);

        if (!result.IsError && result.Value > 0)
        {
            s_abandonedCartRemindersSent(_logger, result.Value, null);
            s_remindersSentCounter.Add(result.Value);
        }
    }
}
