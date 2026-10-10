using ErrorOr;
using Moq;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.Products.Queries.GetProductVariants;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Common.Enums;
using Shopizy.Domain.Common.ValueObjects;
using Shopizy.Domain.Products;
using Shopizy.Domain.Products.Entities;
using Shopizy.Domain.Products.ValueObjects;
using Shouldly;
using Xunit;

namespace Shopizy.Application.UnitTests.Products.Queries.GetProductVariants;

public class GetProductVariantsQueryHandlerTests
{
    private readonly Mock<IProductReader> _mockProductReader;
    private readonly GetProductVariantsQueryHandler _handler;

    public GetProductVariantsQueryHandlerTests()
    {
        _mockProductReader = new Mock<IProductReader>();
        _handler = new GetProductVariantsQueryHandler(_mockProductReader.Object);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductReader
            .Setup(r =>
                r.GetProductVariantsAsync(
                    It.Is<ProductId>(id => id.Value == productId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((IReadOnlyList<ProductVariant>?)null);

        var query = new GetProductVariantsQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CustomErrors.Product.ProductNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenProductExists_ShouldReturnVariants()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var variant = ProductVariant.Create(
            "Red-M",
            "SKU-RED-M",
            Price.CreateNew(12, Currency.usd),
            5
        );
        IReadOnlyList<ProductVariant> variants = [variant];

        _mockProductReader
            .Setup(r =>
                r.GetProductVariantsAsync(
                    It.Is<ProductId>(id => id.Value == productId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(variants);

        var query = new GetProductVariantsQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Name.ShouldBe("Red-M");
    }
}
