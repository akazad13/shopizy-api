using FluentValidation.TestHelper;
using Shopizy.Application.Categories.Commands.DeleteCategory;
using Shopizy.Domain.Categories.ValueObjects;

namespace Shopizy.Application.UnitTests.Categories.Commands.DeleteCategory;

public class DeleteCategoryValidatorTests
{
    private readonly DeleteCategoryValidator _validator;

    public DeleteCategoryValidatorTests()
    {
        _validator = new DeleteCategoryValidator();
    }

    [Fact]
    public async Task Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid(), CategoryId.CreateUnique());

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_NotHaveError_When_UserIdIsEmpty()
    {
        // Arrange - No validation rules exist, so even empty GUID should pass
        var command = new DeleteCategoryCommand(Guid.Empty, CategoryId.CreateUnique());

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_HaveError_When_CategoryIdIsEmpty()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid(), CategoryId.Create(Guid.Empty));

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Should_HaveError_When_CategoryIdIsNull()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid(), null!);

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
