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
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthlyViews = await _context.ProductStatsDailies
            .AsNoTracking()
            .Where(s => s.Date >= monthStart)
            .SumAsync(s => s.Views, ct);
        var monthlyPurchases = await _context.ProductStatsDailies
            .AsNoTracking()
            .Where(s => s.Date >= monthStart)
            .SumAsync(s => s.Purchases, ct);
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
            LowStockCount = lowStockCount,
            MonthlyViews = monthlyViews,
            MonthlyPurchases = monthlyPurchases
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

    public async Task<List<TopProductStat>> GetTopSellersAsync(DateTime? from, DateTime? to, int limit = 10, CancellationToken ct = default)
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
                Count = g.Sum(s => s.Purchases)
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
}
