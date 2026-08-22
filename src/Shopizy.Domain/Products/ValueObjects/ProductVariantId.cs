using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Products.ValueObjects;

public sealed class ProductVariantId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ProductVariantId(Guid value)
    {
        Value = value;
    }

    public static ProductVariantId CreateUnique() => new(Guid.NewGuid());

    public static ProductVariantId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
