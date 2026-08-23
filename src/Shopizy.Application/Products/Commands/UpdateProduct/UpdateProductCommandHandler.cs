using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<UpdateProductCommand, ErrorOr<Success>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ErrorOr<Success>> Handle(
        UpdateProductCommand cmd,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var product = await _productRepository.GetProductByIdForUpdateAsync(
            ProductId.Create(cmd.ProductId)
        );

        if (product is null)
        {
            return (Error)CustomErrors.Product.ProductNotFound;
        }

        product.Update(
            name: cmd.Name,
            shortDescription: cmd.ShortDescription,
            description: cmd.Description,
            categoryId: cmd.CategoryId,
            sku: cmd.Sku,
            unitPrice: cmd.UnitPrice,
            discount: cmd.Discount,
            brandId: cmd.BrandId,
            barcode: cmd.Barcode,
            colors: cmd.Colors,
            sizes: cmd.Sizes,
            tags: cmd.Tags,
            stockQuantity: cmd.StockQuantity,
            highlights: cmd.Highlights
        );

        return Result.Success;
    }
}
