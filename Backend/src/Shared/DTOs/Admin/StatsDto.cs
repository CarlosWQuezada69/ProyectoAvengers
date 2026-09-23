namespace ProyectoAvengers.Shared.DTOs.Admin;

public class OverviewStats
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalUsers { get; set; }
    public int TodayViews { get; set; }
    public int TodayPageViews { get; set; }
    public int MonthlyPageViews { get; set; }
    public int LowStockCount { get; set; }
    public int MonthlyViews { get; set; }
}

public class TopProductStat
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Count { get; set; }
}

public class DailyViewsStat
{
    public string Label { get; set; } = string.Empty;
    public int ProductViews { get; set; }
    public int PageViews { get; set; }
    public int Total { get; set; }
}

public class PageViewsStat
{
    public string PageKey { get; set; } = string.Empty;
    public string PageLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}
