using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Products.ValueObjects;

public sealed class ProductId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ProductId(Guid value)
    {
        Value = value;
    }

    public static ProductId CreateUnique() => new(Guid.NewGuid());

    public static ProductId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
