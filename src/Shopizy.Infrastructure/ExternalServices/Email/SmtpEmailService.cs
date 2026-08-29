using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopizy.Application.Common.Interfaces.Services;

namespace Shopizy.Infrastructure.ExternalServices.Email;

public class SmtpEmailService(
    IOptions<EmailSettings> emailSettings,
    ILogger<SmtpEmailService> logger
) : IEmailService
{
    private readonly EmailSettings _settings = emailSettings.Value;

    private static readonly Action<ILogger, string, string, Exception?> s_emailSent =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, nameof(SmtpEmailService)),
            "Email sent successfully via SMTP → To: {To} | Subject: {Subject}"
        );

    private static readonly Action<ILogger, string, string, Exception?> s_emailFailed =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(2, nameof(SmtpEmailService)),
            "Failed to send email via SMTP → To: {To} | Subject: {Subject}"
        );

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
            };

            if (
                !string.IsNullOrWhiteSpace(_settings.SmtpUser)
                && !string.IsNullOrWhiteSpace(_settings.SmtpPassword)
            )
            {
                client.Credentials = new NetworkCredential(
                    _settings.SmtpUser,
                    _settings.SmtpPassword
                );
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            message.To.Add(to);

            await client.SendMailAsync(message, cancellationToken);
            s_emailSent(logger, to, subject, null);
        }
        catch (Exception ex)
        {
            s_emailFailed(logger, to, subject, ex);
        }
    }
}
