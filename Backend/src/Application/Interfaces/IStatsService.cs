using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface IStatsService
{
    Task<OverviewStats> GetOverviewAsync(CancellationToken ct = default);
    Task<List<TopProductStat>> GetTopViewedAsync(DateTime? from, DateTime? to, int limit = 10, CancellationToken ct = default);
    Task<List<TopProductStat>> GetTopSellersAsync(DateTime? from, DateTime? to, int limit = 10, CancellationToken ct = default);
    Task<List<TopProductStat>> GetLowStockAsync(int threshold = 5, CancellationToken ct = default);
}
