using ErrorOr;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Queries.GetNotificationPreferences;

/// <summary>
/// Query to retrieve a user's multi-channel notification preferences.
/// </summary>
public record GetNotificationPreferencesQuery(Guid UserId)
    : IQuery<ErrorOr<NotificationPreferencesDto>>;
