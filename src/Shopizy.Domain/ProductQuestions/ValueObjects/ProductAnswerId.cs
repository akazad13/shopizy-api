using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.ProductQuestions.ValueObjects;

public sealed class ProductAnswerId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private ProductAnswerId(Guid value)
    {
        Value = value;
    }

    public static ProductAnswerId CreateUnique() => new(Guid.NewGuid());

    public static ProductAnswerId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
