using System.Text.Json.Serialization;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.LoyaltyAccounts.ValueObjects;

public sealed class LoyaltyTransactionId : AggregateRootId<Guid>
{
    public override Guid Value { get; protected set; }

    [JsonConstructor]
    private LoyaltyTransactionId(Guid value)
    {
        Value = value;
    }

    public static LoyaltyTransactionId CreateUnique() => new(Guid.NewGuid());

    public static LoyaltyTransactionId Create(Guid value) => new(value);

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
