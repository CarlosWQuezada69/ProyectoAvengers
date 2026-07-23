using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

public class StatsController : AdminBaseController
{
    private readonly AppDbContext _context;

    public StatsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats/overview")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<OverviewStats>> GetOverview()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalProducts = await _context.Products.CountAsync();
        var activeProducts = await _context.Products.CountAsync(p => p.IsActive);
        var totalCategories = await _context.Categories.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var todayViews = await _context.ProductStatsDailies
            .Where(s => s.Date == today)
            .SumAsync(s => s.Views);
        var lowStockCount = await _context.Products
            .CountAsync(p => p.Stock > 0 && p.Stock <= 5);

        return Ok(new OverviewStats
        {
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            TotalCategories = totalCategories,
            TotalUsers = totalUsers,
            TodayViews = todayViews,
            LowStockCount = lowStockCount
        });
    }

    [HttpGet("stats/products/top-viewed")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<List<TopProductStat>>> GetTopViewed(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10)
    {
        limit = Math.Clamp(limit, 1, 100);
        var fromDate = from.HasValue ? DateOnly.FromDateTime(from.Value) : DateOnly.MinValue;
        var toDate = to.HasValue ? DateOnly.FromDateTime(to.Value) : DateOnly.MaxValue;

        var stats = await _context.ProductStatsDailies
            .Where(s => s.Date >= fromDate && s.Date <= toDate)
            .GroupBy(s => new { s.ProductId, s.Product.Name, ImageUrl = s.Product.ProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault() })
            .Select(g => new TopProductStat
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                Count = g.Sum(s => s.Views)
            })
            .OrderByDescending(s => s.Count)
            .Take(limit)
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("stats/products/top-sellers")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<List<TopProductStat>>> GetTopSellers(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10)
    {
        limit = Math.Clamp(limit, 1, 100);
        var fromDate = from.HasValue ? DateOnly.FromDateTime(from.Value) : DateOnly.MinValue;
        var toDate = to.HasValue ? DateOnly.FromDateTime(to.Value) : DateOnly.MaxValue;

        var stats = await _context.ProductStatsDailies
            .Where(s => s.Date >= fromDate && s.Date <= toDate)
            .GroupBy(s => new { s.ProductId, s.Product.Name, ImageUrl = s.Product.ProductImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault() })
            .Select(g => new TopProductStat
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                Count = g.Sum(s => s.Purchases)
            })
            .OrderByDescending(s => s.Count)
            .Take(limit)
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("stats/products/low-stock")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<List<TopProductStat>>> GetLowStock([FromQuery] int threshold = 5)
    {
        var products = await _context.Products
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
            .ToListAsync();

        return Ok(products);
    }
}
