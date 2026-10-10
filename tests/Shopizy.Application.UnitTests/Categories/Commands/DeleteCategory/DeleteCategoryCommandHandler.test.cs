using ErrorOr;
using Moq;
using Shopizy.Application.Categories.Commands.DeleteCategory;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Categories.TestUtils;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shouldly;

namespace Shopizy.Application.UnitTests.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandlerTests
{
    private readonly DeleteCategoryCommandHandler _sut;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;

    public DeleteCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _sut = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object);
    }

    [Fact]
    public async Task Should_DeleteCategoryAndReturnSuccessResponse_WhenCategoryExists()
    {
        // Arrange
        var command = DeleteCategoryCommandUtils.CreateCommand();
        var category = CategoryFactory.Create();

        _mockCategoryRepository
            .Setup(c => c.GetCategoryByIdAsync(command.CategoryId))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Success);

        _mockCategoryRepository.Verify(x => x.Remove(category), Times.Once);
    }

    [Fact]
    public async Task Should_ReturnCategoryNotFound_WhenCategoryDoesNotExist()
    {
        var command = DeleteCategoryCommandUtils.CreateCommand();
        _mockCategoryRepository
            .Setup(c => c.GetCategoryByIdAsync(It.IsAny<CategoryId>()))
            .ReturnsAsync((Shopizy.Domain.Categories.Category?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CustomErrors.Category.CategoryNotFound.Code);
    }
}
