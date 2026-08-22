namespace Shopizy.Domain.Users.Entities;

/// <summary>
/// Owned type encapsulating multi-channel notification preferences for a <see cref="User"/>.
/// Stored in the same Users table row via EF owned-entity mapping.
/// </summary>
public sealed class NotificationPreference
{
    /// <summary>Gets whether email notifications are enabled.</summary>
    public bool EmailEnabled { get; private set; } = true;

    /// <summary>Gets whether SMS notifications are enabled.</summary>
    public bool SmsEnabled { get; private set; } = true;

    /// <summary>Gets whether push notifications are enabled.</summary>
    public bool PushEnabled { get; private set; } = true;

    /// <summary>Gets whether order update alerts are enabled.</summary>
    public bool OrderUpdates { get; private set; } = true;

    /// <summary>Gets whether marketing promotion alerts are enabled.</summary>
    public bool Promotions { get; private set; } = true;

    /// <summary>Gets whether price drop alerts are enabled.</summary>
    public bool PriceAlerts { get; private set; } = true;

    /// <summary>Gets whether restock / back-in-stock alerts are enabled.</summary>
    public bool RestockAlerts { get; private set; } = true;

    /// <summary>
    /// Creates a default notification preferences instance with all channels enabled.
    /// </summary>
    public static NotificationPreference CreateDefault() => new();

    /// <summary>
    /// Creates a notification preference instance with specified values.
    /// </summary>
    public static NotificationPreference Create(
        bool emailEnabled,
        bool smsEnabled,
        bool pushEnabled,
        bool orderUpdates,
        bool promotions,
        bool priceAlerts,
        bool restockAlerts
    ) =>
        new()
        {
            EmailEnabled = emailEnabled,
            SmsEnabled = smsEnabled,
            PushEnabled = pushEnabled,
            OrderUpdates = orderUpdates,
            Promotions = promotions,
            PriceAlerts = priceAlerts,
            RestockAlerts = restockAlerts,
        };

    /// <summary>
    /// Updates the notification preferences.
    /// </summary>
    public void Update(
        bool emailEnabled,
        bool smsEnabled,
        bool pushEnabled,
        bool orderUpdates,
        bool promotions,
        bool priceAlerts,
        bool restockAlerts
    )
    {
        EmailEnabled = emailEnabled;
        SmsEnabled = smsEnabled;
        PushEnabled = pushEnabled;
        OrderUpdates = orderUpdates;
        Promotions = promotions;
        PriceAlerts = priceAlerts;
        RestockAlerts = restockAlerts;
    }

    // Required by EF Core
    private NotificationPreference() { }
}
