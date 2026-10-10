using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.Entities;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Carts.Commands.AddProductToCart;

/// <summary>
/// Handles the <see cref="AddProductToCartCommand"/> to add products to a cart.
/// </summary>
/// <param name="cartRepository"></param>
/// <param name="productRepository"></param>
public class AddProductToCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository
) : ICommandHandler<AddProductToCartCommand, ErrorOr<Cart>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICartRepository _cartRepository = cartRepository;

    /// <summary>
    /// Handles adding a product to the cart with validation.
    /// </summary>
    /// <param name="cmd">The add product to cart command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated cart or an error.</returns>
    public async Task<ErrorOr<Cart>> Handle(
        AddProductToCartCommand cmd,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var userId = cmd.UserId;
        var productId = cmd.ProductId;

        var cart = await _cartRepository.GetCartByUserIdForUpdateAsync(userId);

        if (cart is null)
        {
            return (Error)CustomErrors.Cart.CartNotFound;
        }

        if (
            cart.CartItems.Any(li =>
                li.ProductId == productId && li.Color == cmd.Color && li.Size == cmd.Size
            )
        )
        {
            return (Error)CustomErrors.Cart.ProductAlreadyExistInCart;
        }

        if (!await _productRepository.IsProductExistAsync(productId))
        {
            return (Error)CustomErrors.Product.ProductNotFound;
        }

        cart.AddLineItem(CartItem.Create(productId, cmd.Color, cmd.Size, cmd.Quantity));

        return cart;
    }
}
