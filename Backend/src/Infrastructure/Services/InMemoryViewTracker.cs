using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;

namespace ProyectoAvengers.Infrastructure.Services;

public class InMemoryViewTracker : IViewTracker
{
    private readonly ConcurrentDictionary<string, int> _views = new();
    private readonly IServiceScopeFactory _scopeFactory;

    public InMemoryViewTracker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void TrackView(Guid productId)
    {
        var key = $"{productId:N}_{DateOnly.FromDateTime(DateTime.UtcNow):O}";
        _views.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        if (_views.IsEmpty)
            return;

        var snapshot = new Dictionary<string, int>(_views);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var (key, count) in snapshot)
        {
            var parts = key.Split('_');
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var productId))
                continue;

            if (!DateOnly.TryParse(parts[1], out var date))
                continue;

            await context.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO product_stats_daily (product_id, date, views, purchases)
                VALUES ({0}, {1}, {2}, 0)
                ON CONFLICT (product_id, date)
                DO UPDATE SET views = product_stats_daily.views + {2}
                """, productId, date, count, ct);
        }

        foreach (var key in snapshot.Keys)
        {
            _views.TryUpdate(key, _views[key] - snapshot[key], _views[key]);
        }

        foreach (var key in snapshot.Keys)
        {
            if (_views.TryGetValue(key, out var remaining) && remaining <= 0)
                _views.TryRemove(key, out _);
        }
    }
}
