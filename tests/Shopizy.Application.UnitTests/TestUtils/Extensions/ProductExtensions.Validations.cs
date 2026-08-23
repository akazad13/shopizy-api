using Shopizy.Application.Products.Commands.CreateProduct;
using Shopizy.Application.Products.Commands.UpdateProduct;
using Shopizy.Application.Products.Queries.GetProduct;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.ValueObjects;

namespace Shopizy.Application.UnitTests.TestUtils.Extensions;

public static partial class ProductExtensions
{
    public static void ValidateResult(this Product product, CreateProductCommand command)
    {
        Assert.Equal(command.Name, product.Name);
        Assert.Equal(command.Description, product.Description);
        Assert.Equal(command.UnitPrice.Amount, product.UnitPrice.Amount);
        Assert.Equal(command.UnitPrice.Currency, product.UnitPrice.Currency);
        Assert.Equal(command.Discount, product.Discount);
        Assert.Equal(command.Sku, product.SKU);
        Assert.Equal(command.BrandId, product.BrandId);
        Assert.Equal(command.Barcode, product.Barcode);
        Assert.Equal(command.StockQuantity, product.StockQuantity);
        Assert.Equal(command.Tags, product.Tags);
        Assert.IsType<CategoryId>(product.CategoryId);
        Assert.Null(product.ModifiedOn);
    }

    public static void ValidateResult(this Product product, GetProductQuery query)
    {
        Assert.IsType<ProductId>(product.Id);
        Assert.IsType<string>(product.Name);
    }

    public static void ValidateResult(this Product product, UpdateProductCommand command)
    {
        Assert.Equal(command.Name, product.Name);
        Assert.Equal(command.Description, product.Description);
        Assert.Equal(command.UnitPrice.Amount, product.UnitPrice.Amount);
        Assert.Equal(command.UnitPrice.Currency, product.UnitPrice.Currency);
        Assert.Equal(command.Discount, product.Discount);
        Assert.Equal(command.Sku, product.SKU);
        Assert.Equal(command.BrandId, product.BrandId);
        Assert.Equal(command.Barcode, product.Barcode);
        Assert.Equal(command.Tags, product.Tags);
        Assert.IsType<CategoryId>(product.CategoryId);
        Assert.True(product.ModifiedOn > product.CreatedOn);
    }
}
