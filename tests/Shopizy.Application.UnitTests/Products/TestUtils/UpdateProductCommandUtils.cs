using Shopizy.Application.Products.Commands.UpdateProduct;
using Shopizy.Application.UnitTests.TestUtils.Constants;
using Shopizy.Domain.Common.ValueObjects;

namespace Shopizy.Application.UnitTests.Products.TestUtils;

public static class UpdateProductCommandUtils
{
    public static UpdateProductCommand CreateCommand() =>
        new(
            Constants.User.Id.Value,
            Constants.Product.Id.Value,
            Constants.Product.Name,
            Constants.Product.ShortDescription,
            Constants.Product.Description,
            Constants.Category.Id,
            Price.CreateNew(Constants.Product.UnitPrice, Constants.Product.Currency),
            Constants.Product.Discount,
            Constants.Product.Sku,
            Constants.Product.BrandId,
            Constants.Product.Colors,
            Constants.Product.Sizes,
            Constants.Product.Tags,
            Constants.Product.Barcode,
            Constants.Product.StockQuantity,
            []
        );
}
