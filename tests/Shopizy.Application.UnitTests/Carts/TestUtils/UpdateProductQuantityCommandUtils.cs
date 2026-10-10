using Shopizy.Application.Carts.Commands.UpdateProductQuantity;
using Shopizy.Application.UnitTests.TestUtils.Constants;

namespace Shopizy.Application.UnitTests.Carts.TestUtils;

public static class UpdateProductQuantityCommandUtils
{
    public static UpdateProductQuantityCommand CreateCommand(int quantity) =>
        new(Constants.User.Id, Constants.CartItem.Id, quantity);
}
