using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Admin.Queries.GetDashboardMetrics;

public class GetDashboardMetricsQueryHandler(
    IOrderReader orderReader,
    IUserRepository userRepository,
    IProductReader productReader
) : IQueryHandler<GetDashboardMetricsQuery, ErrorOr<DashboardMetricsDto>>
{
    private readonly IOrderReader _orderReader = orderReader;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IProductReader _productReader = productReader;

    public async Task<ErrorOr<DashboardMetricsDto>> Handle(
        GetDashboardMetricsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var totalRevenue = await _orderReader.GetTotalRevenueAsync();
        var totalOrders = await _orderReader.GetTotalOrdersCountAsync();
        var totalUsers = await _userRepository.GetTotalUsersCountAsync();
        var totalProducts = await _productReader.GetTotalCountAsync(cancellationToken);

        var lowStockProducts = await _productReader.GetLowStockAsync(5, cancellationToken); // threshold of 5

        var dto = new DashboardMetricsDto(
            totalRevenue,
            totalOrders,
            totalUsers,
            totalProducts,
            lowStockProducts
                .Select(p => new StockAlertDto(p.Id.Value, p.Name, p.StockQuantity))
                .ToList()
        );

        return dto.ToErrorOr();
    }
}
