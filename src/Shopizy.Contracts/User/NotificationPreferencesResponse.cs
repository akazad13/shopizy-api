namespace Shopizy.Contracts.User;

/// <summary>
/// Response contract representing a customer's multi-channel notification preferences.
/// </summary>
public record NotificationPreferencesResponse(
    Guid UserId,
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool OrderUpdates,
    bool Promotions,
    bool PriceAlerts,
    bool RestockAlerts
);
