using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Permissions.ValueObjects;
using Shopizy.Infrastructure.Common.Persistence;

namespace Shopizy.Infrastructure.Permissions.Persistence;

/// <summary>
/// Singleton-cached permission name → id lookup. Permissions are seeded data and rarely change,
/// so the map is loaded once on first use and reloaded on <see cref="Invalidate"/>.
/// </summary>
/// <param name="scopeFactory"></param>
public sealed class PermissionLookup(IServiceScopeFactory scopeFactory) : IPermissionLookup
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IReadOnlyDictionary<string, PermissionId>? _byName;

    public async Task<IReadOnlyList<PermissionId>> GetIdsByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(names);

        var map = _byName ?? await LoadAsync(cancellationToken).ConfigureAwait(false);

        var ids = new List<PermissionId>();
        foreach (var name in names)
        {
            if (!string.IsNullOrWhiteSpace(name) && map.TryGetValue(name, out var id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    public async Task<IReadOnlyList<PermissionId>> GetAllIdsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var map = _byName ?? await LoadAsync(cancellationToken).ConfigureAwait(false);
        return map.Values.ToList();
    }

    public void Invalidate() => _byName = null;

    private async Task<IReadOnlyDictionary<string, PermissionId>> LoadAsync(
        CancellationToken cancellationToken
    )
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_byName is not null)
            {
                return _byName;
            }

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var rows = await dbContext
                .Permissions.AsNoTracking()
                .Select(p => new { p.Name, p.Id })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var map = rows.ToDictionary(r => r.Name, r => r.Id, StringComparer.OrdinalIgnoreCase);
            _byName = map;
            return map;
        }
        finally
        {
            _gate.Release();
        }
    }
}
