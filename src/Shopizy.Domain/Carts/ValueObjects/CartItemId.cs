using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Carts.ValueObjects;

public sealed class CartItemId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private CartItemId(Guid value)
    {
        Value = value;
    }

    public static CartItemId CreateUnique() => new(Guid.NewGuid());

    public static CartItemId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
