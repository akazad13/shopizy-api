using Microsoft.EntityFrameworkCore;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Carts;
using Shopizy.Domain.Carts.ValueObjects;
using Shopizy.Domain.Users.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Carts.Persistence;

/// <summary>
/// Repository for managing cart data persistence.
/// </summary>
/// <param name="dbContext"></param>
public class CartRepository(AppDbContext dbContext) : ICartRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Retrieves all carts from the database.
    /// </summary>
    /// <returns>A list of all carts.</returns>
    public async Task<IReadOnlyList<Cart>> GetCartsAsync() =>
        await _dbContext.Carts.AsNoTracking().ToListAsync();

    /// <summary>
    /// Retrieves a cart by its unique identifier.
    /// </summary>
    /// <param name="id">The cart identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cart if found; otherwise, null.</returns>
    public Task<Cart?> GetCartByIdAsync(CartId id, CancellationToken cancellationToken) =>
        _dbContext.Carts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    /// <summary>
    /// Retrieves a cart by user identifier, including cart items and products.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The cart if found; otherwise, null.</returns>
    public Task<Cart?> GetCartByUserIdAsync(UserId id) =>
        _dbContext
            .Carts.Include(c => c.CartItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == id);

    /// <summary>
    /// Retrieves a cart by user identifier for mutation, including only cart items (no Product navigation).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The cart if found; otherwise, null.</returns>
    public Task<Cart?> GetCartByUserIdForUpdateAsync(UserId id) =>
        _dbContext.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == id);

    /// <summary>
    /// Retrieves inactive carts with items for which no reminder has been sent recently.
    /// </summary>
    public async Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(
        DateTime inactiveBeforeUtc,
        int maxCount = 50,
        CancellationToken cancellationToken = default
    ) =>
        await _dbContext
            .Carts.Include(c => c.CartItems)
            .Where(c =>
                c.CartItems.Any()
                && (c.ModifiedOn ?? c.CreatedOn) < inactiveBeforeUtc
                && (
                    c.LastAbandonedReminderSentOn == null
                    || c.LastAbandonedReminderSentOn < (c.ModifiedOn ?? c.CreatedOn)
                )
            )
            .Take(maxCount)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Adds a new cart to the database.
    /// </summary>
    /// <param name="cart">The cart to add.</param>
    public async Task AddAsync(Cart cart) => await _dbContext.Carts.AddAsync(cart);

    /// <summary>
    /// Updates an existing cart in the database.
    /// </summary>
    /// <param name="cart">The cart to update.</param>
    public void Update(Cart cart) => _dbContext.Update(cart);

    /// <summary>
    /// Removes a cart from the database.
    /// </summary>
    /// <param name="cart">The cart to remove.</param>
    public void Remove(Cart cart) => _dbContext.Remove(cart);
}
