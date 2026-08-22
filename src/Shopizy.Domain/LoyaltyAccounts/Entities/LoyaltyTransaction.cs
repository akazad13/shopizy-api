using System.Text.Json.Serialization;
using Shopizy.Domain.LoyaltyAccounts.Enums;
using Shopizy.Domain.LoyaltyAccounts.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.LoyaltyAccounts.Entities;

public sealed class LoyaltyTransaction : Entity<LoyaltyTransactionId>
{
    public int Points { get; private set; }
    public LoyaltyTransactionType Type { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime CreatedOn { get; private set; }

    public static LoyaltyTransaction Create(
        int points,
        LoyaltyTransactionType type,
        string description
    ) => new(LoyaltyTransactionId.CreateUnique(), points, type, description);

    private LoyaltyTransaction(
        LoyaltyTransactionId id,
        int points,
        LoyaltyTransactionType type,
        string description
    )
        : base(id)
    {
        Points = points;
        Type = type;
        Description = description;
        CreatedOn = DateTime.UtcNow;
    }

    [JsonConstructor]
    private LoyaltyTransaction() { }
}
