using System.Text.Json.Serialization;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Products.Entities;

public sealed class ProductImage : Entity<ProductImageId>
{
    public string ImageUrl { get; set; } = null!;
    public int Seq { get; set; }
    public string PublicId { get; set; } = null!;

    public static ProductImage Create(string productUrl, int seq, string publicId) =>
        new(ProductImageId.CreateUnique(), productUrl, seq, publicId);

    private ProductImage(ProductImageId productImageId, string imageUrl, int seq, string publicId)
        : base(productImageId)
    {
        ImageUrl = imageUrl;
        Seq = seq;
        PublicId = publicId;
    }

    [JsonConstructor]
    private ProductImage() { }
}
