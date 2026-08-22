using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Products.ValueObjects;

public sealed class ProductImageId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ProductImageId(Guid value)
    {
        Value = value;
    }

    public static ProductImageId CreateUnique() => new(Guid.NewGuid());

    public static ProductImageId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
