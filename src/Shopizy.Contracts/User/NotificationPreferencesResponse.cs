namespace Shopizy.Contracts.User;

/// <summary>
/// Response contract representing a customer's notification preferences.
/// </summary>
public record NotificationPreferencesResponse(
    Guid UserId,
    bool EmailEnabled,
    bool OrderUpdates,
    bool Promotions,
    bool PriceAlerts,
    bool RestockAlerts
);
