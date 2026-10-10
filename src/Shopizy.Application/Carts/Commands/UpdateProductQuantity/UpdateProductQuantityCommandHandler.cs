using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Carts.Commands.UpdateProductQuantity;

/// <summary>
/// Handles the <see cref="UpdateProductQuantityCommand"/> to update item quantities in a cart.
/// </summary>
/// <param name="cartRepository"></param>
public class UpdateProductQuantityCommandHandler(ICartRepository cartRepository)
    : ICommandHandler<UpdateProductQuantityCommand, ErrorOr<Cart>>
{
    private readonly ICartRepository _cartRepository = cartRepository;

    /// <summary>
    /// Handles updating the quantity of a product in the cart.
    /// </summary>
    /// <param name="cmd">The update product quantity command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated cart or an error.</returns>
    public async Task<ErrorOr<Cart>> Handle(
        UpdateProductQuantityCommand cmd,
        CancellationToken cancellationToken
    )
    {
        var userId = cmd.UserId;
        var cart = await _cartRepository.GetCartByUserIdForUpdateAsync(userId);

        if (cart is null)
        {
            return (Error)CustomErrors.Cart.CartNotFound;
        }

        cart.UpdateLineItem(cmd.CartItemId, cmd.Quantity);

        return cart;
    }
}
