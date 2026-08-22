using System.Text.Json.Serialization;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Brands;

/// <summary>
/// Represents a product brand in the system.
/// </summary>
public sealed class Brand : AggregateRoot<BrandId, Guid>
{
    /// <summary>
    /// Gets the brand name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the brand logo URL.
    /// </summary>
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Gets the brand country.
    /// </summary>
    public string Country { get; private set; } = null!;

    /// <summary>
    /// Creates a new brand.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="logoUrl"></param>
    /// <param name="country"></param>
    public static Brand Create(string name, string? logoUrl, string country) =>
        new(BrandId.CreateUnique(), name, logoUrl, country);

    /// <summary>
    /// Updates the brand information.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="logoUrl"></param>
    /// <param name="country"></param>
    public void Update(string name, string? logoUrl, string country)
    {
        Name = name;
        LogoUrl = logoUrl;
        Country = country;
    }

    private Brand(BrandId id, string name, string? logoUrl, string country)
        : base(id)
    {
        Name = name;
        LogoUrl = logoUrl;
        Country = country;
    }

    [JsonConstructor]
    private Brand() { }
}
