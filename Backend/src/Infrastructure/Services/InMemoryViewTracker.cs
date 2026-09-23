using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;

namespace ProyectoAvengers.Infrastructure.Services;

public class InMemoryViewTracker : IViewTracker
{
    private readonly ConcurrentDictionary<string, int> _productViews = new();
    private readonly ConcurrentDictionary<string, int> _pageViews = new();
    private readonly IServiceScopeFactory _scopeFactory;

    public InMemoryViewTracker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void TrackView(Guid productId)
    {
        var key = $"{productId:N}_{DateOnly.FromDateTime(DateTime.UtcNow):O}";
        _productViews.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public void TrackPageView(string pageKey)
    {
        var key = $"{pageKey}_{DateOnly.FromDateTime(DateTime.UtcNow):O}";
        _pageViews.AddOrUpdate(key, 1, (_, count) => count + 1);
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        var hasProductViews = !_productViews.IsEmpty;
        var hasPageViews = !_pageViews.IsEmpty;

        if (!hasProductViews && !hasPageViews)
            return;

        var productSnapshot = new Dictionary<string, int>(_productViews);
        var pageSnapshot = new Dictionary<string, int>(_pageViews);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (hasProductViews)
        {
            foreach (var (key, count) in productSnapshot)
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
                    """,
                    new object[] { productId, date, count },
                    ct);
            }

            foreach (var key in productSnapshot.Keys)
            {
                if (_productViews.TryGetValue(key, out var current))
                {
                    var remaining = current - productSnapshot[key];
                    if (remaining <= 0)
                        _productViews.TryRemove(key, out _);
                    else
                        _productViews.TryUpdate(key, remaining, current);
                }
            }
        }

        if (hasPageViews)
        {
            foreach (var (key, count) in pageSnapshot)
            {
                var parts = key.Split('_');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
                    continue;

                if (!DateOnly.TryParse(parts[1], out var date))
                    continue;

                await context.Database.ExecuteSqlRawAsync(
                    """
                    INSERT INTO page_view_daily (page_key, date, views)
                    VALUES ({0}, {1}, {2})
                    ON CONFLICT (page_key, date)
                    DO UPDATE SET views = page_view_daily.views + {2}
                    """,
                    new object[] { parts[0], date, count },
                    ct);
            }

            foreach (var key in pageSnapshot.Keys)
            {
                if (_pageViews.TryGetValue(key, out var current))
                {
                    var remaining = current - pageSnapshot[key];
                    if (remaining <= 0)
                        _pageViews.TryRemove(key, out _);
                    else
                        _pageViews.TryUpdate(key, remaining, current);
                }
            }
        }
    }
}