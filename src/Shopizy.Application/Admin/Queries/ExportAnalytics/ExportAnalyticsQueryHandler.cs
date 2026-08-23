using System.Text;
using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Admin.Queries.ExportAnalytics;

public class ExportAnalyticsQueryHandler(
    IOrderReader orderReader,
    IUserRepository userRepository,
    IProductReader productReader
) : IQueryHandler<ExportAnalyticsQuery, ErrorOr<AnalyticsExportFile>>
{
    private readonly IOrderReader _orderReader = orderReader;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IProductReader _productReader = productReader;

    public async Task<ErrorOr<AnalyticsExportFile>> Handle(
        ExportAnalyticsQuery request,
        CancellationToken cancellationToken
    )
    {
        var totalRevenue = await _orderReader.GetTotalRevenueAsync();
        var totalOrders = await _orderReader.GetTotalOrdersCountAsync();
        var totalUsers = await _userRepository.GetTotalUsersCountAsync();
        var totalProducts = await _productReader.GetTotalCountAsync(cancellationToken);
        var lowStockProducts = await _productReader.GetLowStockAsync(5, cancellationToken);

        var isPdf = request.Format.Equals("pdf", StringComparison.OrdinalIgnoreCase);

        if (isPdf)
        {
            var pdfBuilder = new StringBuilder();
            pdfBuilder.AppendLine("%PDF-1.4");
            pdfBuilder.AppendLine("Shopizy Executive Analytics Report");
            pdfBuilder.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            pdfBuilder.AppendLine("----------------------------------------");
            pdfBuilder.AppendLine($"Total Revenue: ${totalRevenue:F2}");
            pdfBuilder.AppendLine($"Total Orders: {totalOrders}");
            pdfBuilder.AppendLine($"Total Registered Users: {totalUsers}");
            pdfBuilder.AppendLine($"Total Products: {totalProducts}");
            pdfBuilder.AppendLine("----------------------------------------");
            pdfBuilder.AppendLine("Low Stock Alerts:");
            foreach (var p in lowStockProducts)
            {
                pdfBuilder.AppendLine(
                    $" - Product ID: {p.Id.Value} | Name: {p.Name} | Stock: {p.StockQuantity}"
                );
            }
            pdfBuilder.AppendLine("%%EOF");

            var pdfBytes = Encoding.UTF8.GetBytes(pdfBuilder.ToString());
            return new AnalyticsExportFile(
                pdfBytes,
                "application/pdf",
                $"shopizy_analytics_{DateTime.UtcNow:yyyyMMdd}.pdf"
            );
        }
        else
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Metric,Value");
            csvBuilder.AppendLine($"Total Revenue,${totalRevenue:F2}");
            csvBuilder.AppendLine($"Total Orders,{totalOrders}");
            csvBuilder.AppendLine($"Total Users,{totalUsers}");
            csvBuilder.AppendLine($"Total Products,{totalProducts}");
            csvBuilder.AppendLine();
            csvBuilder.AppendLine("LowStockProductId,ProductName,StockQuantity");
            foreach (var p in lowStockProducts)
            {
                csvBuilder.AppendLine(
                    $"\"{p.Id.Value}\",\"{p.Name.Replace("\"", "\"\"")}\",{p.StockQuantity}"
                );
            }

            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return new AnalyticsExportFile(
                csvBytes,
                "text/csv",
                $"shopizy_analytics_{DateTime.UtcNow:yyyyMMdd}.csv"
            );
        }
    }
}
