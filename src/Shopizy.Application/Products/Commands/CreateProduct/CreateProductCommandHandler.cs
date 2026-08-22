using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Products;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<CreateProductCommand, ErrorOr<Product>>
{
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<ErrorOr<Product>> Handle(
        CreateProductCommand cmd,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return CustomErrors.Product.ProductNotCreated.ToError();

        var product = Product.Create(
            name: cmd.Name,
            shortDescription: cmd.ShortDescription,
            description: cmd.Description,
            categoryId: cmd.CategoryId,
            sku: cmd.Sku,
            stockQuantity: cmd.StockQuantity,
            unitPrice: cmd.UnitPrice,
            discount: cmd.Discount,
            brandId: cmd.BrandId,
            barcode: cmd.Barcode,
            colors: cmd.Colors,
            sizes: cmd.Sizes,
            tags: cmd.Tags,
            highlights: cmd.Highlights
        );

        await _productRepository.AddAsync(product);

        return product;
    }
}
