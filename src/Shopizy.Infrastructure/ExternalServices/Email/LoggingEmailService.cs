using Microsoft.Extensions.Logging;
using Shopizy.Application.Common.Interfaces.Services;

namespace Shopizy.Infrastructure.ExternalServices.Email;

public class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    private static readonly Action<ILogger, string, string, Exception?> s_emailLogged =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(LoggingEmailService)),
            "[EMAIL SERVICE] Email dispatched (logging mode) → To: {To} | Subject: {Subject}"
        );

    private static readonly Action<ILogger, string, Exception?> s_emailBodyLogged =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(2, nameof(LoggingEmailService)),
            "[EMAIL BODY PREVIEW]\n{Body}"
        );

    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default
    )
    {
        s_emailLogged(logger, to, subject, null);
        s_emailBodyLogged(logger, body, null);
        return Task.CompletedTask;
    }
}
