using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Carts.Commands.RemoveProductFromCart;

/// <summary>
/// Handles the <see cref="RemoveProductFromCartCommand"/> to remove items from a cart.
/// </summary>
/// <param name="cartRepository"></param>
public class RemoveProductFromCartCommandHandler(ICartRepository cartRepository)
    : ICommandHandler<RemoveProductFromCartCommand, ErrorOr<Cart>>
{
    private readonly ICartRepository _cartRepository = cartRepository;

    /// <summary>
    /// Handles removing a product from the cart.
    /// </summary>
    /// <param name="cmd">The remove product from cart command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated cart or an error.</returns>
    public async Task<ErrorOr<Cart>> Handle(
        RemoveProductFromCartCommand cmd,
        CancellationToken cancellationToken = default
    )
    {
        var userId = cmd.UserId;
        var cart = await _cartRepository.GetCartByUserIdForUpdateAsync(userId);

        if (cart is null)
        {
            return (Error)CustomErrors.Cart.CartNotFound;
        }

        var lineItem = cart.CartItems.FirstOrDefault(li => li.Id == cmd.ItemId);
        if (lineItem is not null)
        {
            cart.RemoveLineItem(lineItem);
        }

        return cart;
    }
}
