using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Orders.Enums;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Users.Queries.GetUser;

/// <summary>
/// Handles the <see cref="GetUserQuery"/> to retrieve user information.
/// </summary>
/// <param name="userRepository"></param>
/// <param name="orderRepository"></param>
public class GetUserQueryHandler(IUserRepository userRepository, IOrderRepository orderRepository)
    : IQueryHandler<GetUserQuery, ErrorOr<UserDto>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IOrderRepository _orderRepository = orderRepository;

    /// <summary>
    /// Handles the query to retrieve user information including order statistics.
    /// </summary>
    /// <param name="request">The get user query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user data transfer object or an error if the user is not found.</returns>
    public async Task<ErrorOr<UserDto>> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetUserByIdAsync(UserId.Create(request.UserId));

        if (user is null)
        {
            return (Error)CustomErrors.User.UserNotFound;
        }

        var userOrdersList = await _orderRepository.GetOrdersByUserIdAsync(
            user.Id,
            cancellationToken
        );

        var userOrders = userOrdersList.Select(o => new { o.Id, o.OrderStatus }).ToList();

        var totalOrders = userOrders.Count;
        var totalRefundedOrders = userOrders.Count(o => o.OrderStatus == OrderStatus.Refunded);
        var totalFavorites = 0;

        var userDto = new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString(),
            user.ProfileImageUrl,
            user.Phone,
            user.Address,
            totalOrders,
            user.ProductReviewIds.Count,
            totalFavorites,
            totalRefundedOrders,
            user.CreatedOn,
            user.ModifiedOn
        );

        return userDto;
    }
}
