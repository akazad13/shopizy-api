namespace Shopizy.Contracts.User;

/// <summary>
/// Request contract for updating a customer's multi-channel notification preferences.
/// </summary>
public record UpdateNotificationPreferencesRequest(
    bool EmailEnabled,
    bool SmsEnabled,
    bool PushEnabled,
    bool OrderUpdates,
    bool Promotions,
    bool PriceAlerts,
    bool RestockAlerts
);
