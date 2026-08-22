using System.Text.Json.Serialization;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Products;
using Shopizy.SharedKernel.Domain.Models;

namespace Shopizy.Domain.Categories;

/// <summary>
/// Represents a product category in the system.
/// </summary>
public class Category : AggregateRoot<CategoryId, Guid>
{
    [JsonInclude]
    private List<Product> _products = [];

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the parent category ID for hierarchical categories.
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Gets the read-only list of products in this category.
    /// </summary>
    public IReadOnlyList<Product> Products => (_products ?? []).AsReadOnly();

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="parentId">The parent category ID (null for root categories).</param>
    /// <returns>A new <see cref="Category"/> instance.</returns>
    public static Category Create(string name, Guid? parentId)
    {
        var category = new Category(CategoryId.CreateUnique(), name, parentId);
        category.AddDomainEvent(new Events.CategoryCreatedDomainEvent(category));

        return category;
    }

    /// <summary>
    /// Updates the category information.
    /// </summary>
    /// <param name="name">The new category name.</param>
    /// <param name="parentId">The new parent category ID.</param>
    public void Update(string name, Guid? parentId)
    {
        Name = name;
        ParentId = parentId;

        this.AddDomainEvent(new Events.CategoryUpdatedDomainEvent(this));
    }

    private Category(CategoryId categoryId, string name, Guid? parentId)
        : base(categoryId)
    {
        Name = name;
        ParentId = parentId;
    }

    [JsonConstructor]
    private Category() { }
}
