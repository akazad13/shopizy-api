using ErrorOr;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.UpdateNotificationPreferences;

/// <summary>
/// Command to update a user's multi-channel notification preferences.
/// </summary>
public record UpdateNotificationPreferencesCommand(
    Guid UserId,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool OrderUpdates,
    bool Promotions,
    bool PriceAlerts,
    bool RestockAlerts
) : ICommand<ErrorOr<NotificationPreferencesDto>>;
