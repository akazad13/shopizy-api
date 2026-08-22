using Mapster;
using Shopizy.Application.Carts.Commands.AddProductToCart;
using Shopizy.Application.Carts.Commands.RemoveProductFromCart;
using Shopizy.Application.Carts.Commands.UpdateProductQuantity;
using Shopizy.Application.Carts.Queries.GetCart;
using Shopizy.Contracts.Cart;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.Entities;

namespace Shopizy.Api.Common.Mapping;

/// <summary>
/// Configures mapping for cart-related models.
/// </summary>
public class CartMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configurations.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<(Guid UserId, AddProductToCartRequest request), AddProductToCartCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest, src => src.request);

        config
            .NewConfig<
                (Guid UserId, Guid CartItemId, UpdateProductQuantityRequest request),
                UpdateProductQuantityCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CartItemId, src => src.CartItemId)
            .Map(dest => dest, src => src.request);

        config
            .NewConfig<(Guid UserId, Guid ItemId), RemoveProductFromCartCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ItemId, src => src.ItemId);

        config.NewConfig<Guid, GetCartQuery>().MapWith(userId => new GetCartQuery(userId));

#pragma warning disable CS8625
        config
            .NewConfig<Cart, CartResponse>()
            .Map(dest => dest.CartId, src => (object?)src.Id != null ? src.Id.Value : Guid.Empty)
            .Map(
                dest => dest.UserId,
                src => (object?)src.UserId != null ? src.UserId.Value : Guid.Empty
            )
            .Map(dest => dest.CartItems, src => src.CartItems);

        config
            .NewConfig<CartItem, CartItemResponse>()
            .Map(
                dest => dest.CartItemId,
                src => (object?)src.Id != null ? src.Id.Value : Guid.Empty
            )
            .Map(
                dest => dest.ProductId,
                src => (object?)src.ProductId != null ? src.ProductId.Value : Guid.Empty
            )
            .Map(dest => dest.Color, src => src.Color)
            .Map(dest => dest.Size, src => src.Size)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Ignore(dest => dest.Product);
#pragma warning restore CS8625
    }
}
