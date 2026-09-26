using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Core.Features.Shifts;

namespace SmallMartApp.UI.ViewModels;

public partial class ShiftViewModel : ViewModelBase
{
    private readonly IShiftService _shiftService;
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private CashierShift? _activeShift;

    [ObservableProperty]
    private bool _hasActiveShift;

    [ObservableProperty]
    private decimal _startingCashInput = 50.00m;

    [ObservableProperty]
    private decimal _actualCashInput = 0m;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Filter & Search Properties
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedRoleFilter = "All Roles";

    [ObservableProperty]
    private string _selectedStatusFilter = "All Statuses";

    [ObservableProperty]
    private string _selectedShiftFilter = "All Shifts";

    [ObservableProperty]
    private string _selectedSortOption = "Newest First (Time)";

    [ObservableProperty]
    private string _cashComparisonFilter = "All Amounts";

    // --- Pagination Controls ---
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalFilteredCount = 0;

    [ObservableProperty]
    private string _paginationInfo = string.Empty;

    [ObservableProperty]
    private string _pageDisplay = "Page 1 of 1";

    public List<int> PageSizeOptions { get; } = new() { 5, 10, 20, 50 };

    public bool CanPreviousPage => CurrentPage > 1;
    public bool CanNextPage => CurrentPage < TotalPages;

    [RelayCommand]
    public void FirstPage()
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1;
            ApplyFiltersAndSorting();
        }
    }

    [RelayCommand]
    public void PreviousPage()
    {
        if (CanPreviousPage)
        {
            CurrentPage--;
            ApplyFiltersAndSorting();
        }
    }

    [RelayCommand]
    public void NextPage()
    {
        if (CanNextPage)
        {
            CurrentPage++;
            ApplyFiltersAndSorting();
        }
    }

    [RelayCommand]
    public void LastPage()
    {
        if (CurrentPage != TotalPages)
        {
            CurrentPage = TotalPages;
            ApplyFiltersAndSorting();
        }
    }

    // Summary Metric Badges
    [ObservableProperty]
    private int _totalShiftsCount;

    [ObservableProperty]
    private int _openShiftsCount;

    [ObservableProperty]
    private int _closedShiftsCount;

    [ObservableProperty]
    private decimal _totalReconciledCash;

    [ObservableProperty]
    private decimal _totalDiscrepancyAmount;

    // Internal cache of all shifts from DB
    private List<CashierShift> _allShiftsCache = new();

    // Bound Observable Collection for DataGrid/ListView
    public ObservableCollection<CashierShiftItemViewModel> FilteredShifts { get; } = new();

    public ObservableCollection<string> RoleFilterOptions { get; } = new()
    {
        "All Roles",
        "Admin",
        "Cashier"
    };

    public ObservableCollection<string> StatusFilterOptions { get; } = new() 
    { 
        "All Statuses", 
        "Open", 
        "Closed" 
    };

    public ObservableCollection<string> ShiftFilterOptions { get; } = new() 
    { 
        "All Shifts", 
        "Morning", 
        "Afternoon", 
        "Night", 
        "FullTime" 
    };

    public ObservableCollection<string> SortOptions { get; } = new() 
    { 
        "Newest First (Time)", 
        "Oldest First (Time)", 
        "Shift # (High-Low)", 
        "Shift # (Low-High)", 
        "Staff Name (A-Z)", 
        "Staff Name (Z-A)", 
        "Expected Cash (High-Low)", 
        "Actual Cash (High-Low)",
        "Discrepancy (Largest Shortage First)"
    };

    public ObservableCollection<string> CashFilterOptions { get; } = new()
    {
        "All Amounts",
        "Balanced / Over (Actual >= Expected)",
        "Shortages Only (Actual < Expected)",
        "Expected >= $100",
        "Expected >= $300"
    };

    public ShiftViewModel(
        IShiftService shiftService,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _shiftService = shiftService;
        _notificationService = notificationService;
    }

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
        IsAdmin = user.Role == UserRole.Admin;
        _ = RefreshShiftAsync();
    }

    partial void OnSearchQueryChanged(string value) { CurrentPage = 1; ApplyFiltersAndSorting(); }
    partial void OnSelectedRoleFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSorting(); }
    partial void OnSelectedStatusFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSorting(); }
    partial void OnSelectedShiftFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSorting(); }
    partial void OnSelectedSortOptionChanged(string value) { CurrentPage = 1; ApplyFiltersAndSorting(); }
    partial void OnCashComparisonFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSorting(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSorting(); }

    [RelayCommand]
    public async Task RefreshShiftAsync()
    {
        int userId = CurrentUser?.Id ?? 2;
        ActiveShift = await _shiftService.GetCurrentActiveShiftAsync(userId);
        HasActiveShift = ActiveShift != null;

        _allShiftsCache = await _shiftService.GetRecentShiftsAsync(100);
        ApplyFiltersAndSorting();
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchQuery = string.Empty;
        SelectedRoleFilter = "All Roles";
        SelectedStatusFilter = "All Statuses";
        SelectedShiftFilter = "All Shifts";
        SelectedSortOption = "Newest First (Time)";
        CashComparisonFilter = "All Amounts";
        ApplyFiltersAndSorting();
    }

    public void ApplyFiltersAndSorting()
    {
        IEnumerable<CashierShift> query = _allShiftsCache;

        // 0. Role-Based Privacy: Cashiers can only view their own shifts, Admins view all
        if (!IsAdmin && CurrentUser != null)
        {
            query = query.Where(s => s.UserId == CurrentUser.Id);
        }

        // 1. Text Search Filter (Staff Name, Username, or Shift ID)
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim().ToLower();
            query = query.Where(s => 
                (s.User != null && s.User.FullName.ToLower().Contains(term)) ||
                (s.User != null && s.User.Username.ToLower().Contains(term)) ||
                s.Id.ToString().Contains(term)
            );
        }

        // 2. Role Filter
        if (SelectedRoleFilter != "All Roles" && !string.IsNullOrEmpty(SelectedRoleFilter))
        {
            query = query.Where(s => s.User != null && 
                s.User.Role.ToString().Equals(SelectedRoleFilter, StringComparison.OrdinalIgnoreCase));
        }

        // 3. Status Filter
        if (SelectedStatusFilter == "Open")
            query = query.Where(s => s.Status == ShiftStatus.Open);
        else if (SelectedStatusFilter == "Closed")
            query = query.Where(s => s.Status == ShiftStatus.Closed);

        // 4. Working Shift Filter
        if (SelectedShiftFilter != "All Shifts" && !string.IsNullOrEmpty(SelectedShiftFilter))
        {
            query = query.Where(s => s.User != null && 
                s.User.WorkingShift.Equals(SelectedShiftFilter, StringComparison.OrdinalIgnoreCase));
        }

        // 5. Cash Range / Comparison Filter
        if (CashComparisonFilter == "Balanced / Over (Actual >= Expected)")
        {
            query = query.Where(s => s.Status == ShiftStatus.Closed && (s.ActualCash ?? 0) >= s.ExpectedCash);
        }
        else if (CashComparisonFilter == "Shortages Only (Actual < Expected)")
        {
            query = query.Where(s => s.Status == ShiftStatus.Closed && (s.ActualCash ?? 0) < s.ExpectedCash);
        }
        else if (CashComparisonFilter == "Expected >= $100")
        {
            query = query.Where(s => s.ExpectedCash >= 100m);
        }
        else if (CashComparisonFilter == "Expected >= $300")
        {
            query = query.Where(s => s.ExpectedCash >= 300m);
        }

        // 6. Dynamic Sorting
        query = SelectedSortOption switch
        {
            "Oldest First (Time)" => query.OrderBy(s => s.StartTime),
            "Shift # (High-Low)" => query.OrderByDescending(s => s.Id),
            "Shift # (Low-High)" => query.OrderBy(s => s.Id),
            "Staff Name (A-Z)" => query.OrderBy(s => s.User?.FullName ?? ""),
            "Staff Name (Z-A)" => query.OrderByDescending(s => s.User?.FullName ?? ""),
            "Expected Cash (High-Low)" => query.OrderByDescending(s => s.ExpectedCash),
            "Actual Cash (High-Low)" => query.OrderByDescending(s => s.ActualCash ?? 0),
            "Discrepancy (Largest Shortage First)" => query.OrderBy(s => (s.ActualCash ?? 0) - s.ExpectedCash),
            _ => query.OrderByDescending(s => s.StartTime)
        };

        var allFilteredList = query.Select(s => new CashierShiftItemViewModel(s)).ToList();

        // Update Summary Metrics on the entire filtered dataset
        TotalShiftsCount = allFilteredList.Count;
        OpenShiftsCount = allFilteredList.Count(s => s.IsOpen);
        ClosedShiftsCount = allFilteredList.Count(s => !s.IsOpen);
        TotalReconciledCash = allFilteredList.Where(s => !s.IsOpen).Sum(s => s.ActualCash ?? 0);
        TotalDiscrepancyAmount = allFilteredList.Where(s => !s.IsOpen).Sum(s => s.Discrepancy);

        // Apply Pagination Slicing
        TotalFilteredCount = allFilteredList.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalFilteredCount / PageSize));

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        var pagedShifts = allFilteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredShifts.Clear();
        foreach (var shift in pagedShifts)
        {
            FilteredShifts.Add(shift);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0 
            ? "No shift records found" 
            : $"Showing {start}-{end} of {TotalFilteredCount} shift audits";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public async Task OpenShiftAsync()
    {
        int userId = CurrentUser?.Id ?? 2;
        var res = await _shiftService.OpenShiftAsync(userId, StartingCashInput);
        if (res.IsSuccess)
        {
            StatusMessage = "Shift opened successfully! Cash drawer ready.";
            _notificationService.ShowSuccess($"Shift #{res.Value?.Id} opened successfully! Starting drawer cash: ${StartingCashInput:F2}");
            await RefreshShiftAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Could not open shift.";
            _notificationService.ShowError(res.Error ?? "Could not open shift.");
        }
    }

    [RelayCommand]
    public async Task CloseShiftAsync()
    {
        if (ActiveShift == null) return;

        var res = await _shiftService.CloseShiftAsync(ActiveShift.Id, ActualCashInput);
        if (res.IsSuccess)
        {
            decimal diff = (res.Value?.ActualCash ?? 0) - (res.Value?.ExpectedCash ?? 0);
            string diffMsg = diff >= 0 ? $"+${diff:F2} (Over/Balanced)" : $"-${Math.Abs(diff):F2} (Shortage)";
            StatusMessage = $"Shift closed! Discrepancy: ${diff:F2}";
            _notificationService.ShowSuccess($"Shift #{ActiveShift.Id} closed successfully! Discrepancy: {diffMsg}");
            await RefreshShiftAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Could not close shift.";
            _notificationService.ShowError(res.Error ?? "Could not close shift.");
        }
    }
}

public class CashierShiftItemViewModel
{
    public CashierShift Shift { get; }
    public int Id => Shift.Id;
    public string CashierName => Shift.User?.FullName ?? "Unknown";
    public string Username => Shift.User != null ? $"@{Shift.User.Username}" : "";
    public string WorkingShift => Shift.User?.WorkingShift ?? "Morning";
    public UserRole? Role => Shift.User?.Role;
    public string RoleBadge => Shift.User?.Role == UserRole.Admin ? "👑 Admin" : "🛒 Cashier";
    public bool IsAdminRole => Shift.User?.Role == UserRole.Admin;

    public DateTime StartTime => Shift.StartTime;
    public DateTime? EndTime => Shift.EndTime;
    public decimal StartingCash => Shift.StartingCash;
    public decimal ExpectedCash => Shift.ExpectedCash;
    public decimal? ActualCash => Shift.ActualCash;
    public ShiftStatus Status => Shift.Status;
    public bool IsOpen => Shift.Status == ShiftStatus.Open;

    public decimal Discrepancy => (ActualCash ?? 0) - ExpectedCash;
    public bool HasShortage => Status == ShiftStatus.Closed && Discrepancy < 0;
    public bool HasSurplus => Status == ShiftStatus.Closed && Discrepancy > 0;
    public bool IsBalanced => Status == ShiftStatus.Closed && Discrepancy == 0;

    public string ActualCashDisplay => ActualCash.HasValue 
        ? $"${ActualCash.Value:F2}" 
        : "--";

    public string DiscrepancyDisplay => Status == ShiftStatus.Open ? "--" : $"${Discrepancy:+0.00;-0.00;0.00}";

    // Shift Sales & Customer Payments Breakdown
    public decimal TotalSalesAmount => Shift.Sales?.Sum(s => s.TotalAmount) ?? 0m;
    public decimal CashSalesAmount => Shift.Sales?.Where(s => s.PaymentMethod == SmallMartApp.Core.Features.Sales.PaymentMethod.Cash).Sum(s => s.TotalAmount) ?? 0m;
    public decimal KhqrSalesAmount => Shift.Sales?.Where(s => s.PaymentMethod == SmallMartApp.Core.Features.Sales.PaymentMethod.KHQR).Sum(s => s.TotalAmount) ?? 0m;
    public int SalesCount => Shift.Sales?.Count ?? 0;

    public string TotalSalesDisplay => SalesCount > 0 
        ? $"${TotalSalesAmount:F2} ({SalesCount} tx)" 
        : "$0.00 (0)";

    public string PaymentBreakdownDisplay
    {
        get
        {
            if (SalesCount == 0) return "No sales yet";
            var parts = new List<string>();
            if (CashSalesAmount > 0) parts.Add($"💵 Cash ${CashSalesAmount:F2}");
            if (KhqrSalesAmount > 0) parts.Add($"📱 KHQR ${KhqrSalesAmount:F2}");
            return parts.Count > 0 ? string.Join(" • ", parts) : "$0.00";
        }
    }

    public string WorkingShiftBadge => WorkingShift switch
    {
        "Morning" => "🌅 Morning",
        "Afternoon" => "☀️ Afternoon",
        "Night" => "🌙 Night",
        "FullTime" => "⭐ Full-Time",
        _ => WorkingShift
    };

    public CashierShiftItemViewModel(CashierShift shift)
    {
        Shift = shift;
    }
}
