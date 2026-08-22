using System.Text.Json.Serialization;
using Shopizy.Domain.Carts.Entities;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Carts;

/// <summary>
/// Represents a shopping cart in the system.
/// </summary>
public sealed class Cart : AggregateRoot<CartId, Guid>, IAuditable
{
    [JsonInclude]
    private List<CartItem> _cartItems = [];

    /// <summary>
    /// Gets the user ID who owns this cart.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the date and time when the cart was created.
    /// </summary>
    public DateTime CreatedOn { get; }

    /// <summary>
    /// Gets the date and time when the cart was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; private set; }

    /// <summary>
    /// Gets the timestamp when an abandoned cart reminder email was last dispatched.
    /// </summary>
    public DateTime? LastAbandonedReminderSentOn { get; private set; }

    /// <summary>
    /// Gets the read-only list of items in the cart.
    /// </summary>
    public IReadOnlyList<CartItem> CartItems => (_cartItems ?? []).AsReadOnly();

    /// <summary>
    /// Creates a new shopping cart for a user.
    /// </summary>
    /// <param name="userId">The user ID who owns the cart.</param>
    /// <returns>A new <see cref="Cart"/> instance.</returns>
    public static Cart Create(UserId userId) => new(CartId.CreateUnique(), userId);

    /// <summary>
    /// Adds an item to the cart.
    /// </summary>
    /// <param name="lineItem">The cart item to add.</param>
    public void AddLineItem(CartItem lineItem)
    {
        _cartItems.Add(lineItem);
        LastAbandonedReminderSentOn = null;
        this.AddDomainEvent(new Events.CartItemAddedDomainEvent(this, lineItem));
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    /// <param name="lineItem">The cart item to remove.</param>
    public void RemoveLineItem(CartItem lineItem)
    {
        _cartItems.Remove(lineItem);
        LastAbandonedReminderSentOn = null;
        this.AddDomainEvent(new Events.CartItemRemovedDomainEvent(this, lineItem));
    }

    /// <summary>
    /// Removes all items from the cart without raising per-item domain events.
    /// Used for system-initiated clearing (e.g. after order placement).
    /// </summary>
    public void Clear()
    {
        _cartItems.Clear();
        LastAbandonedReminderSentOn = null;
    }

    /// <summary>
    /// Updates the quantity of a cart item.
    /// </summary>
    /// <param name="cartItemId">The cart item identifier.</param>
    /// <param name="quantity">The new quantity.</param>
    public void UpdateLineItem(CartItemId cartItemId, int quantity)
    {
        _cartItems.Find(li => li.Id == cartItemId)?.UpdateQuantity(quantity);
        LastAbandonedReminderSentOn = null;
    }

    /// <summary>
    /// Records the timestamp when an abandoned cart reminder was sent.
    /// </summary>
    /// <param name="sentOnUtc">The UTC timestamp of the sent reminder.</param>
    public void RecordAbandonedReminderSent(DateTime sentOnUtc) =>
        LastAbandonedReminderSentOn = sentOnUtc;

    private Cart(CartId cartId, UserId userId)
        : base(cartId)
    {
        UserId = userId;
    }

#pragma warning disable CS8618 // EF Core parameterless constructor
    [JsonConstructor]
    private Cart() { }
#pragma warning restore CS8618
}
