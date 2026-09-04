using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class StatsController : AdminBaseController
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("stats/overview")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<OverviewStats>> GetOverview()
        => Ok(await _statsService.GetOverviewAsync());

    [HttpGet("stats/products/top-viewed")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<List<TopProductStat>>> GetTopViewed(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10)
        => Ok(await _statsService.GetTopViewedAsync(from, to, limit));

    [HttpGet("stats/products/top-sellers")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<List<TopProductStat>>> GetTopSellers(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10)
        => Ok(await _statsService.GetTopSellersAsync(from, to, limit));

    [HttpGet("stats/products/low-stock")]
    [RequirePermission("stats.view")]
    public async Task<ActionResult<List<TopProductStat>>> GetLowStock([FromQuery] int threshold = 5)
        => Ok(await _statsService.GetLowStockAsync(threshold));
}
