using System.Text.Json.Serialization;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.Domain.Wishlists.Entities;
using Shopizy.Domain.Wishlists.ValueObjects;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Wishlists;

public sealed class Wishlist : AggregateRoot<WishlistId, Guid>, IAuditable
{
    [JsonInclude]
    private List<WishlistItem> _wishlistItems = [];

    public UserId UserId { get; } = null!;
    public string? Name { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime CreatedOn { get; }
    public DateTime? ModifiedOn { get; private set; }
    public IReadOnlyList<WishlistItem> WishlistItems => (_wishlistItems ?? []).AsReadOnly();

    public static Wishlist Create(UserId userId, string? name = null, bool isPublic = false) =>
        new(WishlistId.CreateUnique(), userId, name, isPublic);

    public void UpdateSettings(string? name, bool isPublic)
    {
        Name = name;
        IsPublic = isPublic;
    }

    public void AddItem(ProductId productId) => _wishlistItems.Add(WishlistItem.Create(productId));

    public void RemoveItem(ProductId productId)
    {
        var item = _wishlistItems.Find(i => i.ProductId == productId);
        if (item is not null)
        {
            _wishlistItems.Remove(item);
        }
    }

    private Wishlist(WishlistId id, UserId userId, string? name, bool isPublic)
        : base(id)
    {
        UserId = userId;
        Name = name;
        IsPublic = isPublic;
    }

    [JsonConstructor]
    private Wishlist() { }
}
