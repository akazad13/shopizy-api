using Moq;
using Shopizy.Application.Common.Interfaces.Services;
using Shopizy.Application.Shipping.Queries.GetShippingMethods;
using Shouldly;
using Xunit;

namespace Shopizy.Application.UnitTests.Shipping.Queries;

public class GetShippingMethodsQueryHandlerTests
{
    private readonly Mock<IShippingCarrierService> _mockCarrierService = new();
    private readonly GetShippingMethodsQueryHandler _sut;

    public GetShippingMethodsQueryHandlerTests()
    {
        _sut = new GetShippingMethodsQueryHandler(_mockCarrierService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnShippingMethodsFromService()
    {
        // Arrange
        var expectedMethods = new List<ShippingRateEstimateDto>
        {
            new("Standard", "STANDARD", "Standard Delivery", 4.99m, "USD", 3, 5),
            new("Express", "EXPRESS", "Express Delivery", 9.99m, "USD", 2, 3),
            new("Premium", "PREMIUM", "Premium Delivery", 19.99m, "USD", 1, 2),
        };

        _mockCarrierService
            .Setup(x => x.GetShippingMethodsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMethods);

        // Act
        var result = await _sut.Handle(new GetShippingMethodsQuery(), CancellationToken.None);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(3);
        result.Value.ShouldContain(m => m.ServiceCode == "STANDARD");
        result.Value.ShouldContain(m => m.ServiceCode == "EXPRESS");
        result.Value.ShouldContain(m => m.ServiceCode == "PREMIUM");
    }
}
