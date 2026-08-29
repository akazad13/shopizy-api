namespace Shopizy.Infrastructure.Services.Shipping;

/// <summary>
/// Settings for shipping options and fixed rate defaults.
/// </summary>
public class ShippingSettings
{
    public const string Section = "ShippingSettings";

    /// <summary>
    /// Fixed rate for Standard delivery (default $4.99).
    /// </summary>
    public decimal StandardShippingRate { get; set; } = 4.99m;

    /// <summary>
    /// Fixed rate for Express delivery (default $9.99).
    /// </summary>
    public decimal ExpressShippingRate { get; set; } = 9.99m;

    /// <summary>
    /// Fixed rate for Premium delivery (default $19.99).
    /// </summary>
    public decimal PremiumShippingRate { get; set; } = 19.99m;
}
