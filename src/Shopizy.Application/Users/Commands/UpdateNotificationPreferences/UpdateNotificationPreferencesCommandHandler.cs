using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Commands.UpdateNotificationPreferences;

/// <summary>
/// Handles updating a user's notification preferences.
/// </summary>
public class UpdateNotificationPreferencesCommandHandler(IUserRepository userRepository)
    : ICommandHandler<UpdateNotificationPreferencesCommand, ErrorOr<NotificationPreferencesDto>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ErrorOr<NotificationPreferencesDto>> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetUserByIdAsync(UserId.Create(request.UserId));
        if (user is null)
        {
            return (Error)CustomErrors.User.UserNotFound;
        }

        user.UpdateNotificationPreferences(
            request.EmailEnabled,
            request.PushEnabled,
            request.OrderUpdates,
            request.Promotions,
            request.PriceAlerts,
            request.RestockAlerts
        );

        _userRepository.Update(user);

        var prefs = user.NotificationPreferences;
        return new NotificationPreferencesDto(
            EmailEnabled: prefs.EmailEnabled,
            PushEnabled: prefs.PushEnabled,
            OrderUpdates: prefs.OrderUpdates,
            Promotions: prefs.Promotions,
            PriceAlerts: prefs.PriceAlerts,
            RestockAlerts: prefs.RestockAlerts
        );
    }
}
