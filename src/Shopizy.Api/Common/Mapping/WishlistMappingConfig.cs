using Mapster;
using Shopizy.Application.Wishlists.Commands.CreateWishlist;
using Shopizy.Application.Wishlists.Commands.UpdateWishlist;
using Shopizy.Application.Wishlists.Commands.UpdateWishlistSettings;
using Shopizy.Application.Wishlists.Queries.GetWishlist;
using Shopizy.Contracts.Wishlist;
using Shopizy.Domain.Wishlists;
using Shopizy.Domain.Wishlists.Entities;

namespace Shopizy.Api.Common.Mapping;

public class WishlistMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<Guid, CreateWishlistCommand>()
            .MapWith(userId => new CreateWishlistCommand(userId));

        config
            .NewConfig<(Guid UserId, CreateWishlistRequest? request), CreateWishlistCommand>()
            .MapWith(src => new CreateWishlistCommand(
                src.UserId,
                src.request != null ? src.request.Name : null,
                src.request != null ? src.request.IsPublic : false
            ));

        config.NewConfig<Guid, GetWishlistQuery>().MapWith(userId => new GetWishlistQuery(userId));

        config
            .NewConfig<(Guid UserId, UpdateWishlistRequest request), UpdateWishlistCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.request.ProductId)
            .Map(
                dest => dest.Action,
                src =>
                    src.request.Action.Equals("Remove", StringComparison.OrdinalIgnoreCase)
                        ? WishlistAction.Remove
                        : WishlistAction.Add
            );

        config
            .NewConfig<
                (Guid UserId, UpdateWishlistSettingsRequest request),
                UpdateWishlistSettingsCommand
            >()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Name, src => src.request.Name)
            .Map(dest => dest.IsPublic, src => src.request.IsPublic);

#pragma warning disable CS8625
        config
            .NewConfig<Wishlist, WishlistResponse>()
            .Map(
                dest => dest.WishlistId,
                src => (object?)src.Id != null ? src.Id.Value : Guid.Empty
            )
            .Map(
                dest => dest.UserId,
                src => (object?)src.UserId != null ? src.UserId.Value : Guid.Empty
            )
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.IsPublic, src => src.IsPublic)
            .Map(dest => dest.WishlistItems, src => src.WishlistItems);

        config
            .NewConfig<WishlistItem, WishlistItemResponse>()
            .Map(
                dest => dest.WishlistItemId,
                src => (object?)src.Id != null ? src.Id.Value : Guid.Empty
            )
            .Map(
                dest => dest.ProductId,
                src => (object?)src.ProductId != null ? src.ProductId.Value : Guid.Empty
            );
#pragma warning restore CS8625
    }
}
