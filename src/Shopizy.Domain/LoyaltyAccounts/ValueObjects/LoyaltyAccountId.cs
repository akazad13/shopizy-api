using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.LoyaltyAccounts.ValueObjects;

public sealed class LoyaltyAccountId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private LoyaltyAccountId(Guid value)
    {
        Value = value;
    }

    public static LoyaltyAccountId CreateUnique() => new(Guid.NewGuid());

    public static LoyaltyAccountId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
