using Shopizy.Application.Categories.Commands.UpdateCategory;
using Shopizy.Application.UnitTests.TestUtils.Constants;

namespace Shopizy.Application.UnitTests.Categories.TestUtils;

public static class UpdateCategoryCommandUtils
{
    public static UpdateCategoryCommand CreateCommand() =>
        new(
            Constants.User.Id.Value,
            Constants.Category.Id,
            Constants.Category.Name,
            Constants.Category.ParentId
        );
}
