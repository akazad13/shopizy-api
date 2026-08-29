using Microsoft.Extensions.Logging;
using Shopizy.Application.Common.Interfaces.Services;

namespace Shopizy.Infrastructure.Services.Notifications;

public class NotificationDispatcher(
    IEmailService emailService,
    IPushNotificationService pushService,
    ILogger<NotificationDispatcher> logger
) : INotificationDispatcher
{
    private static readonly Action<ILogger, Guid, string, Exception?> LogDispatchCompleted =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(1, nameof(DispatchNotificationAsync)),
            "Notification dispatched for User: {UserId}, Subject: '{Subject}'"
        );

    private readonly IEmailService _emailService = emailService;
    private readonly IPushNotificationService _pushService = pushService;
    private readonly ILogger<NotificationDispatcher> _logger = logger;

    public async Task DispatchNotificationAsync(
        Guid userId,
        string? email,
        string subject,
        string message,
        string? targetUrl = null,
        NotificationPreferencesDto? preferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var prefs = preferences ?? new NotificationPreferencesDto();
        var tasks = new List<Task>();

        // 1. Email Channel
        if (prefs.EmailEnabled && !string.IsNullOrWhiteSpace(email))
        {
            tasks.Add(_emailService.SendAsync(email, subject, message, cancellationToken));
        }

        // 2. Push Channel
        if (prefs.PushEnabled && userId != Guid.Empty)
        {
            tasks.Add(
                _pushService.SendPushNotificationAsync(
                    userId,
                    subject,
                    message,
                    targetUrl,
                    cancellationToken
                )
            );
        }

        await Task.WhenAll(tasks);
        LogDispatchCompleted(_logger, userId, subject, null);
    }
}
