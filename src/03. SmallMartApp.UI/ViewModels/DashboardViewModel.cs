using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Dashboard;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;

namespace SmallMartApp.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;

    [ObservableProperty]
    private decimal _todayRevenue;

    [ObservableProperty]
    private int _todayTransactions;

    [ObservableProperty]
    private int _lowStockWarningsCount;

    [ObservableProperty]
    private string _activeShiftCashier = "None";

    public ObservableCollection<Sale> RecentSales { get; } = new();
    public ObservableCollection<Product> LowStockProducts { get; } = new();

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        TodayRevenue = summary.TodayRevenue;
        TodayTransactions = summary.TodayTransactions;
        LowStockWarningsCount = summary.LowStockWarningsCount;
        ActiveShiftCashier = summary.ActiveShiftCashier;

        RecentSales.Clear();
        foreach (var s in summary.RecentSales) RecentSales.Add(s);

        LowStockProducts.Clear();
        foreach (var p in summary.LowStockProducts) LowStockProducts.Add(p);
    }
}
