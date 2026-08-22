using Mapster;
using Shopizy.Application.Products.Commands.AddVariant;
using Shopizy.Application.Products.Commands.UpdateVariant;
using Shopizy.Contracts.Product;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Products.Entities;

namespace Shopizy.Api.Common.Mapping;

/// <summary>
/// Configures mapping for product variant-related models.
/// </summary>
public class ProductVariantMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configurations.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

#pragma warning disable CS8625
        config
            .NewConfig<ProductVariant, ProductVariantResponse>()
            .Map(dest => dest.VariantId, src => (object?)src.Id != null ? src.Id.Value : Guid.Empty)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice != null ? src.UnitPrice.Amount : 0m)
            .Map(
                dest => dest.Currency,
                src => src.UnitPrice != null ? src.UnitPrice.Currency.ToString() : "usd"
            );
#pragma warning restore CS8625

        config
            .NewConfig<(Guid ProductId, AddVariantRequest req), AddVariantCommand>()
            .MapWith(src => new AddVariantCommand(
                src.ProductId,
                src.req.Name,
                src.req.SKU,
                src.req.UnitPrice,
                Enum.Parse<Currency>(src.req.Currency, ignoreCase: true),
                src.req.StockQuantity
            ));

        config
            .NewConfig<
                (Guid ProductId, Guid VariantId, UpdateVariantRequest req),
                UpdateVariantCommand
            >()
            .MapWith(src => new UpdateVariantCommand(
                src.ProductId,
                src.VariantId,
                src.req.Name,
                src.req.SKU,
                src.req.UnitPrice,
                Enum.Parse<Currency>(src.req.Currency, ignoreCase: true),
                src.req.StockQuantity,
                src.req.IsActive
            ));
    }
}
