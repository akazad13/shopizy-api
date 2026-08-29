using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shopizy.Infrastructure.Services.Shipping;
using Shouldly;
using Xunit;

namespace Shopizy.Infrastructure.UnitTests.Services;

public class ShippingCarrierServiceTests
{
    private readonly Mock<ILogger<ShippingCarrierService>> _mockLogger = new();
    private readonly ShippingSettings _settings = new();
    private readonly ShippingCarrierService _sut;

    public ShippingCarrierServiceTests()
    {
        var options = Options.Create(_settings);
        _sut = new ShippingCarrierService(options, _mockLogger.Object);
    }

    [Fact]
    public async Task GetShippingMethodsAsync_ShouldReturnStandardExpressPremiumOptions()
    {
        // Act
        var rates = await _sut.GetShippingMethodsAsync();

        // Assert
        rates.Count.ShouldBe(3);

        var standard = rates.First(r => r.ServiceCode == "STANDARD");
        standard.Rate.ShouldBe(_settings.StandardShippingRate);
        standard.ServiceName.ShouldBe("Standard Delivery");

        var express = rates.First(r => r.ServiceCode == "EXPRESS");
        express.Rate.ShouldBe(_settings.ExpressShippingRate);
        express.ServiceName.ShouldBe("Express Delivery");

        var premium = rates.First(r => r.ServiceCode == "PREMIUM");
        premium.Rate.ShouldBe(_settings.PremiumShippingRate);
        premium.ServiceName.ShouldBe("Premium Delivery");
    }

    [Fact]
    public async Task TrackShipmentAsync_ValidCarrierAndNumber_ShouldReturnTrackingCheckpoints()
    {
        // Act
        var tracking = await _sut.TrackShipmentAsync("FedEx", "123456789012");

        // Assert
        tracking.ShouldNotBeNull();
        tracking.Carrier.ShouldBe("FedEx");
        tracking.TrackingNumber.ShouldBe("123456789012");
        tracking.Checkpoints.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task TrackShipmentAsync_EmptyCarrier_ShouldReturnNull()
    {
        // Act
        var tracking = await _sut.TrackShipmentAsync("", "123456789012");

        // Assert
        tracking.ShouldBeNull();
    }
}
