using ErrorOr;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Carts.Commands.AddProductToCart;

/// <summary>
/// Represents a command to add a product to a shopping cart.
/// </summary>
/// <param name="UserId">The user's unique identifier.</param>
/// <param name="ProductId">The product's unique identifier.</param>
/// <param name="Color">The selected product color.</param>
/// <param name="Size">The selected product size.</param>
/// <param name="Quantity">The quantity to add.</param>
public record AddProductToCartCommand(
    UserId UserId,
    ProductId ProductId,
    string Color,
    string Size,
    int Quantity
) : ICommand<ErrorOr<Cart>>;
