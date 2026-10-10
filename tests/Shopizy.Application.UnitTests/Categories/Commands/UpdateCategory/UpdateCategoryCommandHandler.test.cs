using ErrorOr;
using Moq;
using Shopizy.Application.Categories.Commands.UpdateCategory;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Categories.TestUtils;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shouldly;

namespace Shopizy.Application.UnitTests.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly UpdateCategoryCommandHandler _sut;

    public UpdateCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _sut = new UpdateCategoryCommandHandler(_mockCategoryRepository.Object);
    }

    [Fact]
    public async Task Should_ReturnCategory_WhenCategoryIsUpdatedSuccessfully()
    {
        // Arrange
        var command = UpdateCategoryCommandUtils.CreateCommand();
        var category = CategoryFactory.Create();

        _mockCategoryRepository
            .Setup(c => c.GetCategoryByIdAsync(command.CategoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Success);
    }

    [Fact]
    public async Task Should_ReturnCategoryNotFound_WhenCategoryDoesNotExist()
    {
        var command = UpdateCategoryCommandUtils.CreateCommand();
        _mockCategoryRepository
            .Setup(c => c.GetCategoryByIdAsync(It.IsAny<CategoryId>()))
            .ReturnsAsync((Shopizy.Domain.Categories.Category?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CustomErrors.Category.CategoryNotFound.Code);
    }
}
