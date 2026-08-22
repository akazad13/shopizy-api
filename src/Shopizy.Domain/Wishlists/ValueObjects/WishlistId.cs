using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Wishlists.ValueObjects;

public sealed class WishlistId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private WishlistId(Guid value)
    {
        Value = value;
    }

    public static WishlistId CreateUnique() => new(Guid.NewGuid());

    public static WishlistId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
