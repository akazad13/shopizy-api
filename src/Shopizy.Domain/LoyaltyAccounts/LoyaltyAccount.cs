using System.Text.Json.Serialization;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.LoyaltyAccounts.Entities;
using Shopizy.Domain.LoyaltyAccounts.Enums;
using Shopizy.Domain.LoyaltyAccounts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.LoyaltyAccounts;

public sealed class LoyaltyAccount : AggregateRoot<LoyaltyAccountId, Guid>, IAuditable
{
    [JsonInclude]
    private List<LoyaltyTransaction> _transactions = [];

    public UserId UserId { get; private set; } = null!;
    public int TotalPoints { get; private set; }
    public IReadOnlyList<LoyaltyTransaction> Transactions => (_transactions ?? []).AsReadOnly();
    public DateTime CreatedOn { get; private set; }
    public DateTime? ModifiedOn { get; private set; }

    public static LoyaltyAccount Create(UserId userId) =>
        new(LoyaltyAccountId.CreateUnique(), userId);

    public void EarnPoints(int points, string description)
    {
        var transaction = LoyaltyTransaction.Create(
            points,
            LoyaltyTransactionType.Earn,
            description
        );
        _transactions.Add(transaction);
        TotalPoints += points;
        ModifiedOn = DateTime.UtcNow;
    }

    public DomainResult<bool> RedeemPoints(int points, string description)
    {
        if (TotalPoints < points)
        {
            return CustomErrors.LoyaltyAccount.InsufficientPoints;
        }

        var transaction = LoyaltyTransaction.Create(
            points,
            LoyaltyTransactionType.Redeem,
            description
        );
        _transactions.Add(transaction);
        TotalPoints -= points;
        ModifiedOn = DateTime.UtcNow;

        return true;
    }

    public void AdjustPoints(int points, string description)
    {
        var transaction = LoyaltyTransaction.Create(
            points,
            LoyaltyTransactionType.Adjustment,
            description
        );
        _transactions.Add(transaction);
        TotalPoints += points;
        ModifiedOn = DateTime.UtcNow;
    }

    private LoyaltyAccount(LoyaltyAccountId id, UserId userId)
        : base(id)
    {
        UserId = userId;
        TotalPoints = 0;
        CreatedOn = DateTime.UtcNow;
    }

    [JsonConstructor]
    private LoyaltyAccount() { }
}
