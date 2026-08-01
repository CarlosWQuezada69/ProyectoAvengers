namespace ProyectoAvengers.Shared.DTOs.Admin;

public class OverviewStats
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalUsers { get; set; }
    public int TotalOrders { get; set; }
    public int TodayViews { get; set; }
    public int LowStockCount { get; set; }
    public int MonthlyViews { get; set; }
    public int MonthlyPurchases { get; set; }
}

public class TopProductStat
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Count { get; set; }
}
