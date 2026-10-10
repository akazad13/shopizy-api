using Shopizy.Application.Categories.Commands.DeleteCategory;
using Shopizy.Application.UnitTests.TestUtils.Constants;

namespace Shopizy.Application.UnitTests.Categories.TestUtils;

public static class DeleteCategoryCommandUtils
{
    public static DeleteCategoryCommand CreateCommand() =>
        new(Constants.User.Id.Value, Constants.Category.Id);
}
