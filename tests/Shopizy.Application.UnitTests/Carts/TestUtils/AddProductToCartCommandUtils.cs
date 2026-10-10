using Shopizy.Application.Carts.Commands.AddProductToCart;
using Shopizy.Application.UnitTests.TestUtils.Constants;

namespace Shopizy.Application.UnitTests.Carts.TestUtils;

public static class AddProductToCartCommandUtils
{
    public static AddProductToCartCommand CreateCommand() =>
        new(
            Constants.User.Id,
            Constants.Product.Id,
            Constants.CartItem.Color,
            Constants.CartItem.Size,
            Constants.CartItem.Quantity
        );
}
