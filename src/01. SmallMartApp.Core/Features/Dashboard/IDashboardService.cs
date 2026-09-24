using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;

namespace SmallMartApp.Core.Features.Dashboard;

public class DashboardSummaryDto
{
    public decimal TodayRevenue { get; set; }
    public int TodayTransactions { get; set; }
    public int LowStockWarningsCount { get; set; }
    public string ActiveShiftCashier { get; set; } = "None";
    public List<Sale> RecentSales { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
