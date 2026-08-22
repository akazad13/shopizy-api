using System.Text.Json.Serialization;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Orders.ValueObjects;
using Shopizy.Domain.Returns.Entities;
using Shopizy.Domain.Returns.Enums;
using Shopizy.Domain.Returns.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Returns;

public sealed class ReturnRequest : AggregateRoot<ReturnRequestId, Guid>, IAuditable
{
    [JsonInclude]
    private List<ReturnItem> _items = new();

    public OrderId OrderId { get; private set; } = null!;
    public UserId UserId { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public string? AdminNote { get; private set; }
    public ReturnStatus Status { get; private set; }

    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    public IReadOnlyList<ReturnItem> Items => (_items ?? []).AsReadOnly();

    public static ReturnRequest Create(
        OrderId orderId,
        UserId userId,
        string reason,
        IReadOnlyList<ReturnItem> items
    )
    {
        return new ReturnRequest(ReturnRequestId.CreateUnique(), orderId, userId, reason, items);
    }

    private ReturnRequest(
        ReturnRequestId id,
        OrderId orderId,
        UserId userId,
        string reason,
        IReadOnlyList<ReturnItem> items
    )
        : base(id)
    {
        OrderId = orderId;
        UserId = userId;
        Reason = reason;
        Status = ReturnStatus.Pending;
        _items = items.ToList();
    }

    public DomainResult<bool> Approve()
    {
        if (Status != ReturnStatus.Pending)
        {
            return CustomErrors.ReturnRequest.ReturnNotPending;
        }

        Status = ReturnStatus.Approved;
        return true;
    }

    public DomainResult<bool> Reject(string adminNote)
    {
        if (Status != ReturnStatus.Pending)
        {
            return CustomErrors.ReturnRequest.ReturnNotPending;
        }

        Status = ReturnStatus.Rejected;
        AdminNote = adminNote;
        return true;
    }

    public DomainResult<bool> CompleteRefund()
    {
        if (Status != ReturnStatus.Approved)
        {
            return CustomErrors.ReturnRequest.ReturnAlreadyProcessed;
        }

        Status = ReturnStatus.Refunded;
        return true;
    }

    [JsonConstructor]
    private ReturnRequest() { } // For EF Core
}
