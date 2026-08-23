using ErrorOr;
using Moq;
using Shopizy.Application.Admin.Queries.GetDashboardMetrics;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Application.UnitTests.Products.TestUtils;
using Shopizy.Domain.Products;
using Shouldly;

namespace Shopizy.Application.UnitTests.Admin.Queries.GetDashboardMetrics;

public class GetDashboardMetricsQueryHandlerTests
{
    private readonly Mock<IOrderReader> _mockOrderReader;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IProductReader> _mockProductReader;
    private readonly GetDashboardMetricsQueryHandler _handler;

    public GetDashboardMetricsQueryHandlerTests()
    {
        _mockOrderReader = new Mock<IOrderReader>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockProductReader = new Mock<IProductReader>();

        _handler = new GetDashboardMetricsQueryHandler(
            _mockOrderReader.Object,
            _mockUserRepository.Object,
            _mockProductReader.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAggregatedMetricsAndLowStockAlerts()
    {
        // Arrange
        var query = new GetDashboardMetricsQuery();
        var lowStockProduct = ProductFactory.CreateProduct();

        _mockOrderReader.Setup(r => r.GetTotalRevenueAsync()).ReturnsAsync(15000.50m);
        _mockOrderReader.Setup(r => r.GetTotalOrdersCountAsync()).ReturnsAsync(120);
        _mockUserRepository.Setup(r => r.GetTotalUsersCountAsync()).ReturnsAsync(45);
        _mockProductReader
            .Setup(r => r.GetTotalCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(300);
        _mockProductReader
            .Setup(r => r.GetLowStockAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { lowStockProduct });

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.TotalRevenue.ShouldBe(15000.50m);
        result.Value.TotalOrders.ShouldBe(120);
        result.Value.TotalUsers.ShouldBe(45);
        result.Value.TotalProducts.ShouldBe(300);
        result.Value.StockAlerts.Count.ShouldBe(1);
        result.Value.StockAlerts[0].ProductId.ShouldBe(lowStockProduct.Id.Value);
        result.Value.StockAlerts[0].ProductName.ShouldBe(lowStockProduct.Name);
        result.Value.StockAlerts[0].CurrentStock.ShouldBe(lowStockProduct.StockQuantity);
    }
}
