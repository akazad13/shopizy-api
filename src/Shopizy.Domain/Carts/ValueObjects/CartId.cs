using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Carts.ValueObjects;

public sealed class CartId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private CartId(Guid value)
    {
        Value = value;
    }

    public static CartId CreateUnique() => new(Guid.NewGuid());

    public static CartId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
