using ErrorOr;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Carts.Commands.UpdateProductQuantity;

/// <summary>
/// Represents a command to update the quantity of a product in the cart.
/// </summary>
/// <param name="UserId">The user's unique identifier.</param>
/// <param name="CartItemId">The cart item's unique identifier.</param>
/// <param name="Quantity">The new quantity.</param>
public record UpdateProductQuantityCommand(UserId UserId, CartItemId CartItemId, int Quantity)
    : ICommand<ErrorOr<Cart>>;
