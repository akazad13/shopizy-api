using Mapster;
using Shopizy.Application.Users.Commands.UpdatePassword;
using Shopizy.Application.Users.Commands.UpdateUser;
using Shopizy.Application.Users.Queries.GetUser;
using Shopizy.Contracts.Order;
using Shopizy.Contracts.User;
using Shopizy.Domain.Users;
using Shopizy.Domain.Users.Entities;

namespace Shopizy.Api.Common.Mapping;

/// <summary>
/// Configures mapping for user-related models.
/// </summary>
public class UserMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configurations.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<(Guid UserId, UpdateUserRequest request), UpdateUserCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.request)
            .Map(dest => dest.Street, src => src.request.Address.Street)
            .Map(dest => dest.City, src => src.request.Address.City)
            .Map(dest => dest.State, src => src.request.Address.State)
            .Map(dest => dest.Country, src => src.request.Address.Country)
            .Map(dest => dest.ZipCode, src => src.request.Address.ZipCode);

        config
            .NewConfig<(Guid UserId, UpdatePasswordRequest request), UpdatePasswordCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.request);

        config.NewConfig<Guid, GetUserQuery>().MapWith(userId => new GetUserQuery(userId));

        config.NewConfig<UserDto, UserDetails>().Map(dest => dest.Id, src => src.Id.Value);

#pragma warning disable CS8625
        config
            .NewConfig<User, UserDetails>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(
                dest => dest.Address,
                src =>
                    (object?)src.Address == null
                        ? null
                        : new Address(
                            src.Address.Street,
                            src.Address.City,
                            src.Address.State,
                            src.Address.Country,
                            src.Address.ZipCode
                        )
            )
            .Map(dest => dest.TotalOrders, src => src.OrderIds.Count)
            .Map(dest => dest.TotalReviewed, src => src.ProductReviewIds.Count)
            .Map(dest => dest.TotalFavorites, src => 0)
            .Map(dest => dest.TotalReturns, src => 0);
#pragma warning restore CS8625

        config
            .NewConfig<UserAddress, UserAddressResponse>()
            .Map(dest => dest.AddressId, src => src.Id.Value)
            .Map(dest => dest.Street, src => src.Street)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.State, src => src.State)
            .Map(dest => dest.Country, src => src.Country)
            .Map(dest => dest.ZipCode, src => src.ZipCode)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.CreatedOn, src => src.CreatedOn);

        config
            .NewConfig<
                (Guid UserId, UpdateNotificationPreferencesRequest request),
                Shopizy.Application.Users.Commands.UpdateNotificationPreferences.UpdateNotificationPreferencesCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.EmailEnabled, src => src.request.EmailEnabled)
            .Map(dest => dest.SmsEnabled, src => src.request.SmsEnabled)
            .Map(dest => dest.PushEnabled, src => src.request.PushEnabled)
            .Map(dest => dest.OrderUpdates, src => src.request.OrderUpdates)
            .Map(dest => dest.Promotions, src => src.request.Promotions)
            .Map(dest => dest.PriceAlerts, src => src.request.PriceAlerts)
            .Map(dest => dest.RestockAlerts, src => src.request.RestockAlerts);

        config
            .NewConfig<
                (
                    Guid UserId,
                    Shopizy.Application.Common.Interfaces.Services.NotificationPreferencesDto dto
                ),
                NotificationPreferencesResponse
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.EmailEnabled, src => src.dto.EmailEnabled)
            .Map(dest => dest.SmsEnabled, src => src.dto.SmsEnabled)
            .Map(dest => dest.PushEnabled, src => src.dto.PushEnabled)
            .Map(dest => dest.OrderUpdates, src => src.dto.OrderUpdates)
            .Map(dest => dest.Promotions, src => src.dto.Promotions)
            .Map(dest => dest.PriceAlerts, src => src.dto.PriceAlerts)
            .Map(dest => dest.RestockAlerts, src => src.dto.RestockAlerts);
    }
}
