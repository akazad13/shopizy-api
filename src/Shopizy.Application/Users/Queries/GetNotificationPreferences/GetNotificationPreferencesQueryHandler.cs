using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Queries.GetNotificationPreferences;

/// <summary>
/// Handles retrieving notification preferences for a user.
/// </summary>
public class GetNotificationPreferencesQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetNotificationPreferencesQuery, ErrorOr<NotificationPreferencesDto>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ErrorOr<NotificationPreferencesDto>> Handle(
        GetNotificationPreferencesQuery request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetUserByIdAsync(UserId.Create(request.UserId));
        if (user is null)
        {
            return (Error)CustomErrors.User.UserNotFound;
        }

        var prefs = user.NotificationPreferences;
        return new NotificationPreferencesDto(
            EmailEnabled: prefs.EmailEnabled,
            OrderUpdates: prefs.OrderUpdates,
            Promotions: prefs.Promotions,
            PriceAlerts: prefs.PriceAlerts,
            RestockAlerts: prefs.RestockAlerts
        );
    }
}
