using Mapster;
using Shopizy.Application.Products.Commands.CreateProduct;
using Shopizy.Application.Products.Commands.DeleteProduct;
using Shopizy.Application.Products.Commands.DeleteProductImage;
using Shopizy.Application.Products.Commands.UpdateProduct;
using Shopizy.Application.Products.Queries.GetProduct;
using Shopizy.Application.Products.Queries.GetProducts;
using Shopizy.Contracts.Product;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.ProductReviews;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.Entities;

namespace Shopizy.Api.Common.Mapping;

/// <summary>
/// Configures mapping for product-related models.
/// </summary>
public class ProductMappingConfig : IRegister
{
    /// <summary>
    /// Registers the mapping configurations.
    /// </summary>
    /// <param name="config">The type adapter configuration.</param>
    public void Register(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        config
            .NewConfig<ProductsResult, ProductsPagedResponse>()
            .Map(dest => dest.Items, src => src.Products)
            .Map(dest => dest.TotalCount, src => src.TotalCount);

        config
            .NewConfig<ProductsCriteria, GetProductsQuery>()
            .MapWith(src => new GetProductsQuery(
                src.ProductIds,
                src.Name,
                src.CategoryIds,
                src.BrandIds,
                src.AverageRating,
                src.MinPrice,
                src.MaxPrice,
                src.InStockOnly,
                src.SortBy,
                src.PageNumber,
                src.PageSize
            ));

        config
            .NewConfig<(Guid UserId, CreateProductRequest request), CreateProductCommand>()
            .MapWith(src => new CreateProductCommand(
                src.UserId,
                src.request.Name,
                src.request.ShortDescription,
                src.request.Description,
                CategoryId.Create(src.request.CategoryId),
                Price.CreateNew(src.request.UnitPrice, (Currency)src.request.Currency),
                src.request.Discount,
                src.request.Sku,
                src.request.StockQuantity,
                src.request.BrandId.HasValue ? BrandId.Create(src.request.BrandId.Value) : null,
                src.request.Colors,
                src.request.Sizes,
                src.request.Tags,
                src.request.Barcode,
                src.request.SpecificationIds,
                src.request.Highlights
            ));

        config
            .NewConfig<
                (Guid UserId, Guid ProductId, UpdateProductRequest request),
                UpdateProductCommand
            >()
            .MapWith(src => new UpdateProductCommand(
                src.UserId,
                src.ProductId,
                src.request.Name,
                src.request.ShortDescription,
                src.request.Description,
                CategoryId.Create(src.request.CategoryId),
                Price.CreateNew(src.request.UnitPrice, (Currency)src.request.Currency),
                src.request.Discount,
                src.request.Sku,
                src.request.BrandId.HasValue ? BrandId.Create(src.request.BrandId.Value) : null,
                src.request.Colors,
                src.request.Sizes,
                src.request.Tags,
                src.request.Barcode,
                src.request.StockQuantity,
                src.request.SpecificationIds,
                src.request.Highlights
            ));

        config
            .NewConfig<(Guid UserId, Guid ProductId), DeleteProductCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.ProductId);

        config.NewConfig<Guid, GetProductQuery>().MapWith(src => new GetProductQuery(src));

#pragma warning disable CS8625
        config
            .NewConfig<Product, ProductResponse>()
            .ConstructUsing(src => new ProductResponse(
                (object?)src.Id != null ? src.Id.Value : Guid.Empty,
                src.Name,
                src.ShortDescription,
                src.Description,
                (object?)src.CategoryId != null ? src.CategoryId.Value : Guid.Empty,
                (object?)src.UnitPrice != null ? src.UnitPrice.Amount.ToString() : "0",
                src.Discount ?? 0m,
                (object?)src.BrandId != null ? (Guid?)src.BrandId.Value : null,
                src.Sizes,
                src.Colors,
                src.Tags,
                src.Barcode,
                src.StockQuantity,
                (object?)src.AverageRating != null
                    ? new Contracts.Product.AverageRating(
                        src.AverageRating.Value,
                        src.AverageRating.NumRatings
                    )
                    : new Contracts.Product.AverageRating(0, 0),
                src.ProductImages.Select(img => new ProductImageResponse(
                        img.Id.Value,
                        img.ImageUrl,
                        img.Seq,
                        img.PublicId
                    ))
                    .ToList(),
                src.Highlights
            ));

        config
            .NewConfig<Product, ProductDetailResponse>()
            .ConstructUsing(src => new ProductDetailResponse(
                (object?)src.Id != null ? src.Id.Value : Guid.Empty,
                src.Name,
                src.ShortDescription,
                src.Description,
                (object?)src.CategoryId != null ? src.CategoryId.Value : Guid.Empty,
                (object?)src.UnitPrice != null ? src.UnitPrice.Amount.ToString() : "0",
                src.Discount ?? 0m,
                src.SKU,
                (object?)src.BrandId != null ? (Guid?)src.BrandId.Value : null,
                src.Sizes,
                src.Colors,
                src.Tags,
                src.Barcode,
                src.StockQuantity,
                (object?)src.AverageRating != null
                    ? new Contracts.Product.AverageRating(
                        src.AverageRating.Value,
                        src.AverageRating.NumRatings
                    )
                    : new Contracts.Product.AverageRating(0, 0),
                src.Favourites,
                null,
                src.ProductImages.Select(img => new ProductImageResponse(
                        img.Id.Value,
                        img.ImageUrl,
                        img.Seq,
                        img.PublicId
                    ))
                    .ToList(),
                new List<ProductDetailReviewResponse>(),
                src.Highlights
            ));

        config
            .NewConfig<ProductImage, ProductImageResponse>()
            .Map(
                dest => dest.ProductImageId,
                src => (object?)src.Id != null ? src.Id.Value : Guid.Empty
            );

        config
            .NewConfig<ProductReview, ProductDetailReviewResponse>()
            .Map(
                dest => dest.ProductReviewId,
                src => (object?)src.Id != null ? src.Id.Value : Guid.Empty
            )
            .Map(dest => dest.Rating, src => src.Rating != null ? src.Rating.Value : 0m)
            .Map(
                dest => dest.Reviewer,
                src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty
            )
            .Map(
                dest => dest.ReviewerImageUrl,
                src => src.User != null ? src.User.ProfileImageUrl : null
            );
#pragma warning restore CS8625

        config
            .NewConfig<(Guid UserId, Guid ProductId, Guid ImageId), DeleteProductImageCommand>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ImageId, src => src.ImageId);
    }
}
