using ErrorOr;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.Products;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.CreateProduct;

/// <summary>
/// Creates a new product. Carries domain value objects (<see cref="Price"/>, <see cref="CategoryId"/>,
/// <see cref="BrandId"/>) instead of primitives — the mapping layer converts the contract DTO
/// into typed inputs so handlers don't repeat construction logic.
/// </summary>
/// <param name="UserId"></param>
/// <param name="Name"></param>
/// <param name="ShortDescription"></param>
/// <param name="Description"></param>
/// <param name="CategoryId"></param>
/// <param name="UnitPrice"></param>
/// <param name="Discount"></param>
/// <param name="Sku"></param>
/// <param name="StockQuantity"></param>
/// <param name="BrandId"></param>
/// <param name="Colors"></param>
/// <param name="Sizes"></param>
/// <param name="Tags"></param>
/// <param name="Barcode"></param>
/// <param name="SpecificationIds"></param>
/// <param name="Highlights"></param>
public record CreateProductCommand(
    Guid UserId,
    string Name,
    string ShortDescription,
    string Description,
    CategoryId CategoryId,
    Price UnitPrice,
    decimal Discount,
    string Sku,
    int StockQuantity,
    BrandId? BrandId,
    string Colors,
    string Sizes,
    string Tags,
    string Barcode,
    IList<Guid>? SpecificationIds,
    string? Highlights = null
) : ICommand<ErrorOr<Product>>;
