using FluentValidation.TestHelper;
using Shopizy.Application.Categories.Commands.UpdateCategory;
using Shopizy.Domain.Categories.ValueObjects;

namespace Shopizy.Application.UnitTests.Categories.Commands.UpdateCategory;

public class UpdateCategoryValidatorTests
{
    private readonly UpdateCategoryValidator _validator;

    public UpdateCategoryValidatorTests()
    {
        _validator = new UpdateCategoryValidator();
    }

    [Fact]
    public async Task Should_HaveError_When_CategoryIdIsNull()
    {
        // Arrange
        var command = new UpdateCategoryCommand(Guid.NewGuid(), null!, "Valid Name", null);

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Should_HaveError_When_CategoryIdIsEmpty()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.Create(Guid.Empty),
            "Valid Name",
            null
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsEmpty()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.CreateUnique(),
            "",
            null
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsNull()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.CreateUnique(),
            null!,
            null
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_HaveError_When_NameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('A', 101);
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.CreateUnique(),
            longName,
            null
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_NotHaveError_When_NameIsValid()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.CreateUnique(),
            "Updated Electronics",
            null
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_NotHaveError_When_NameIsAtMaxLength()
    {
        // Arrange
        var maxLengthName = new string('A', 100);
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.CreateUnique(),
            maxLengthName,
            null
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Should_NotHaveError_When_AllFieldsAreValid()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            CategoryId.CreateUnique(),
            "Electronics",
            Guid.NewGuid()
        );

        // Act
        var result = await _validator.TestValidateAsync(
            command,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
