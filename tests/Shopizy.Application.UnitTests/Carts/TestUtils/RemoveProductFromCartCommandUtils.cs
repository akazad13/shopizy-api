using Shopizy.Application.Carts.Commands.RemoveProductFromCart;
using Shopizy.Application.UnitTests.TestUtils.Constants;

namespace Shopizy.Application.UnitTests.Carts.TestUtils;

public static class RemoveProductFromCartCommandUtils
{
    public static RemoveProductFromCartCommand CreateCommand() =>
        new(Constants.User.Id, Constants.CartItem.Id);
}
