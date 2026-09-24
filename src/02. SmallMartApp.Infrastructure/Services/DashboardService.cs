using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Features.Dashboard;
using SmallMartApp.Core.Features.Shifts;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly SmallMartDbContext _context;

    public DashboardService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;

        var todaySales = await _context.Sales
            .Where(s => s.CreatedAt >= today)
            .ToListAsync();

        decimal todayRevenue = todaySales.Sum(s => s.TotalAmount);
        int todayTransactions = todaySales.Count;

        var lowStockProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockAlertThreshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        var recentSales = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .ToListAsync();

        var activeShift = await _context.CashierShifts
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Status == ShiftStatus.Open);

        string activeCashier = activeShift?.User?.FullName ?? (activeShift != null ? "Active" : "None");

        return new DashboardSummaryDto
        {
            TodayRevenue = todayRevenue,
            TodayTransactions = todayTransactions,
            LowStockWarningsCount = lowStockProducts.Count,
            ActiveShiftCashier = activeCashier,
            RecentSales = recentSales,
            LowStockProducts = lowStockProducts
        };
    }
}
