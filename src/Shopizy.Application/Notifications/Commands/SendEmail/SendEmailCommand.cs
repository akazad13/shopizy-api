using ErrorOr;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Notifications.Commands.SendEmail;

public record SendEmailCommand(string To, string Subject, string Body) : ICommand<ErrorOr<bool>>;
