namespace Shopizy.Application.Common.Interfaces.Services;

/// <summary>
/// Unified notification dispatcher routing messages across Email and Push.
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches a notification to the user across enabled channels according to preferences.
    /// </summary>
    Task DispatchNotificationAsync(
        Guid userId,
        string? email,
        string subject,
        string message,
        string? targetUrl = null,
        NotificationPreferencesDto? preferences = null,
        CancellationToken cancellationToken = default
    );
}
