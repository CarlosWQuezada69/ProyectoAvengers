using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class StatsService : IStatsService
{
    private readonly AppDbContext _context;

    public StatsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OverviewStats> GetOverviewAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalProducts = await _context.Products.AsNoTracking().CountAsync(ct);
        var activeProducts = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive, ct);
        var totalCategories = await _context.Categories.AsNoTracking().CountAsync(ct);
        var totalUsers = await _context.Users.AsNoTracking().CountAsync(ct);
        var todayViews = await _context.ProductStatsDailies
            .AsNoTracking()
            .Where(s => s.Date == today)
            .SumAsync(s => s.Views, ct);
        var todayPageViews = await _context.PageViewDailies
            .AsNoTracking()
            .Where(s => s.Date == today)
            .SumAsync(s => s.Views, ct);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthlyViews = await _context.ProductStatsDailies
            .AsNoTracking()
            .Where(s => s.Date >= monthStart)
            .SumAsync(s => s.Views, ct);
        var monthlyPageViews = await _context.PageViewDailies
            .AsNoTracking()
            .Where(s => s.Date >= monthStart)
            .SumAsync(s => s.Views, ct);
        var lowStockCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.Stock > 0 && p.Stock <= 5, ct);

        return new OverviewStats
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            TotalCategories = totalCategories,
            TotalUsers = totalUsers,
            TodayViews = todayViews,
            TodayPageViews = todayPageViews,
            MonthlyPageViews = monthlyPageViews,
            LowStockCount = lowStockCount,
            MonthlyViews = monthlyViews
        };
    }

    public async Task<List<TopProductStat>> GetTopViewedAsync(DateTime? from, DateTime? to, int limit = 10, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        var fromDate = from.HasValue ? DateOnly.FromDateTime(from.Value) : DateOnly.MinValue;
        var toDate = to.HasValue ? DateOnly.FromDateTime(to.Value) : DateOnly.MaxValue;

        return await _context.ProductStatsDailies
            .AsNoTracking()
            .Where(s => s.Date >= fromDate && s.Date <= toDate)
            .GroupBy(s => new
            {
                s.ProductId,
                s.Product.Name,
                ImageUrl = s.Product.ProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()
            })
            .Select(g => new TopProductStat
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                Count = g.Sum(s => s.Views)
            })
            .OrderByDescending(s => s.Count)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<TopProductStat>> GetLowStockAsync(int threshold = 5, CancellationToken ct = default)
        => await _context.Products
            .AsNoTracking()
            .Where(p => p.Stock > 0 && p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .Take(50)
            .Select(p => new TopProductStat
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ImageUrl = p.ProductImages
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                Count = p.Stock
            })
            .ToListAsync(ct);

    public async Task<List<DailyViewsStat>> GetDailyViewsAsync(int days = 7, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 60);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.AddDays(-(days - 1));

        var productRows = await _context.ProductStatsDailies
            .AsNoTracking()
            .Where(s => s.Date >= start)
            .GroupBy(s => s.Date)
            .Select(g => new { Date = g.Key, Views = g.Sum(s => s.Views) })
            .ToListAsync(ct);

        var pageRows = await _context.PageViewDailies
            .AsNoTracking()
            .Where(s => s.Date >= start)
            .GroupBy(s => s.Date)
            .Select(g => new { Date = g.Key, Views = g.Sum(s => s.Views) })
            .ToListAsync(ct);

        var result = new List<DailyViewsStat>(days);
        for (var d = start; d <= today; d = d.AddDays(1))
        {
            var productViews = productRows.FirstOrDefault(r => r.Date == d)?.Views ?? 0;
            var pageViews = pageRows.FirstOrDefault(r => r.Date == d)?.Views ?? 0;

            result.Add(new DailyViewsStat
            {
                Label = d.ToString("ddd dd/MM", CultureInfo.InvariantCulture),
                ProductViews = productViews,
                PageViews = pageViews,
                Total = productViews + pageViews
            });
        }

        return result;
    }

    public async Task<List<PageViewsStat>> GetPageViewsAsync(CancellationToken ct = default)
    {
        var rows = await _context.PageViewDailies
            .AsNoTracking()
            .GroupBy(s => s.PageKey)
            .Select(g => new { Key = g.Key, Count = g.Sum(s => s.Views) })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["home"] = "Inicio",
            ["catalog"] = "Catálogo",
            ["about"] = "Acerca de",
            ["product"] = "Detalle de producto",
            ["contact"] = "Contacto",
            ["not-found"] = "No encontrada"
        };

        return rows.Select(r => new PageViewsStat
        {
            PageKey = r.Key,
            PageLabel = labels.TryGetValue(r.Key, out var label) ? label : r.Key,
            Count = r.Count
        }).ToList();
    }
}
