using ErrorOr;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Carts.Commands.RemoveProductFromCart;

/// <summary>
/// Represents a command to remove a product from a shopping cart.
/// </summary>
/// <param name="UserId">The user's unique identifier.</param>
/// <param name="ItemId">The cart item's unique identifier to remove.</param>
public record RemoveProductFromCartCommand(UserId UserId, CartItemId ItemId)
    : ICommand<ErrorOr<Cart>>;
