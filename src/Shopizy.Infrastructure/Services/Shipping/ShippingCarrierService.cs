using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shopizy.Application.Common.Interfaces.Services;

namespace Shopizy.Infrastructure.Services.Shipping;

/// <summary>
/// Service providing fixed shipping rates for Standard, Express, and Premium tiers and package tracking.
/// </summary>
public class ShippingCarrierService(
    IOptions<ShippingSettings> options,
    ILogger<ShippingCarrierService> logger
) : IShippingCarrierService
{
    private static readonly Action<ILogger, string, string, Exception?> LogShipmentTracked =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(2, nameof(TrackShipmentAsync)),
            "Queried tracking for Carrier: {Carrier}, TrackingNumber: {TrackingNumber}"
        );

    private readonly ShippingSettings _settings = options.Value;
    private readonly ILogger<ShippingCarrierService> _logger = logger;

    public Task<IReadOnlyList<ShippingRateEstimateDto>> GetShippingMethodsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var rates = new List<ShippingRateEstimateDto>
        {
            // 1. Standard Delivery (Fixed price e.g. $4.99, 3-5 business days)
            new(
                Carrier: "Standard",
                ServiceCode: "STANDARD",
                ServiceName: "Standard Delivery",
                Rate: _settings.StandardShippingRate,
                Currency: "USD",
                EstimatedDaysMin: 3,
                EstimatedDaysMax: 5
            ),
            // 2. Express Delivery (Fixed price e.g. $9.99, 2-3 business days)
            new(
                Carrier: "Express",
                ServiceCode: "EXPRESS",
                ServiceName: "Express Delivery",
                Rate: _settings.ExpressShippingRate,
                Currency: "USD",
                EstimatedDaysMin: 2,
                EstimatedDaysMax: 3
            ),
            // 3. Premium Delivery (Fixed price e.g. $19.99, 1-2 business days)
            new(
                Carrier: "Premium",
                ServiceCode: "PREMIUM",
                ServiceName: "Premium Delivery",
                Rate: _settings.PremiumShippingRate,
                Currency: "USD",
                EstimatedDaysMin: 1,
                EstimatedDaysMax: 2
            ),
        };

        return Task.FromResult<IReadOnlyList<ShippingRateEstimateDto>>(rates.AsReadOnly());
    }

    public Task<ShippingTrackingInfoDto?> TrackShipmentAsync(
        string carrier,
        string trackingNumber,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(carrier) || string.IsNullOrWhiteSpace(trackingNumber))
        {
            return Task.FromResult<ShippingTrackingInfoDto?>(null);
        }

        var now = DateTime.UtcNow;
        var checkpoints = new List<TrackingCheckpointDto>
        {
            new(now.AddDays(-2), "Origin Facility, New York NY", "Shipment information received"),
            new(now.AddDays(-1.5), "Sorting Hub, Philadelphia PA", "Arrived at sort facility"),
            new(
                now.AddDays(-1),
                "In Transit, Chicago IL",
                "Departed facility in transit to destination"
            ),
            new(now.AddHours(-3), "Local Depot, Destination City", "Out for delivery"),
        };

        var trackingInfo = new ShippingTrackingInfoDto(
            Carrier: carrier,
            TrackingNumber: trackingNumber,
            Status: "InTransit",
            CurrentLocation: "Local Depot, Destination City",
            EstimatedDelivery: now.AddHours(4),
            Checkpoints: checkpoints.AsReadOnly()
        );

        LogShipmentTracked(_logger, carrier, trackingNumber, null);
        return Task.FromResult<ShippingTrackingInfoDto?>(trackingInfo);
    }
}
