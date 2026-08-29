namespace Shopizy.Contracts.User;

/// <summary>
/// Request contract for updating a customer's notification preferences.
/// </summary>
public record UpdateNotificationPreferencesRequest(
    bool EmailEnabled,
    bool PushEnabled,
    bool OrderUpdates,
    bool Promotions,
    bool PriceAlerts,
    bool RestockAlerts
);
