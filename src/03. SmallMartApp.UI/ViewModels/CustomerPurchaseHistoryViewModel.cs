using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Hardware;
using SmallMartApp.UI.Services;

namespace SmallMartApp.UI.ViewModels;

public class CustomerPurchaseItemViewModel
{
    public Sale RawSale { get; }

    public int Id => RawSale.Id;
    public string ReceiptNumber => RawSale.ReceiptNumber;
    public DateTime CreatedAt => RawSale.CreatedAt;
    public string FormattedDate => RawSale.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
    public string ShortDate => RawSale.CreatedAt.ToString("MMM dd, yyyy");
    public string TimeDisplay => RawSale.CreatedAt.ToString("hh:mm tt");

    public bool HasCustomer => RawSale.Customer != null;
    public string CustomerName => RawSale.Customer?.FullName ?? "Walk-in Guest";
    public string CustomerPhone => RawSale.Customer?.PhoneNumber ?? "No Phone";
    public string CustomerDisplay => RawSale.Customer != null ? $"{RawSale.Customer.FullName} ({RawSale.Customer.PhoneNumber})" : "Walk-in Guest";

    public string CashierName => RawSale.Shift?.User?.FullName ?? "Staff Cashier";
    public string ShiftDisplay => RawSale.ShiftId.HasValue ? $"Shift #{RawSale.ShiftId.Value}" : "Direct Sale";
    public string CashierDisplay => RawSale.ShiftId.HasValue ? $"{CashierName} (Shift #{RawSale.ShiftId.Value})" : CashierName;

    public int TotalItemsCount => RawSale.Items?.Sum(i => i.Quantity) ?? 0;
    public string ItemsCountDisplay => $"{TotalItemsCount} items";

    public decimal TotalAmount => RawSale.TotalAmount;
    public string TotalDisplay => $"${RawSale.TotalAmount:F2}";

    public decimal DiscountAmount => RawSale.DiscountAmount;
    public string DiscountDisplay => RawSale.DiscountAmount > 0 ? $"-${RawSale.DiscountAmount:F2}" : "$0.00";

    public decimal CashReceived => RawSale.CashReceived;
    public decimal ChangeGiven => RawSale.ChangeGiven;

    public PaymentMethod PaymentMethod => RawSale.PaymentMethod;
    public bool IsKhqr => RawSale.PaymentMethod == PaymentMethod.KHQR;
    public string PaymentMethodDisplay => RawSale.PaymentMethod == PaymentMethod.KHQR ? "Bakong KHQR" : "Cash";

    public List<SaleItem> Items => RawSale.Items ?? new();

    public CustomerPurchaseItemViewModel(Sale sale)
    {
        RawSale = sale;
    }
}

public partial class CustomerPurchaseHistoryViewModel : ViewModelBase
{
    private readonly ISalesService _salesService;
    private readonly IReceiptPrinter _receiptPrinter;
    private readonly INotificationService _notificationService;
    private readonly List<Sale> _allSalesCache = new();
    private bool _isLoading;

    public ObservableCollection<CustomerPurchaseItemViewModel> FilteredSales { get; } = new();

    // --- KPI Statistics ---
    [ObservableProperty]
    private int _totalOrdersCount;

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private decimal _totalCashSales;

    [ObservableProperty]
    private decimal _totalKhqrSales;

    [ObservableProperty]
    private int _memberOrdersCount;

    // --- Search, Filter & Sort Controls ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedPaymentFilter = "All Payments";

    public List<string> PaymentFilters { get; } = new()
    {
        "All Payments",
        "Cash Only",
        "Bakong KHQR Only"
    };

    [ObservableProperty]
    private string _selectedCustomerFilter = "All Customers";

    public List<string> CustomerFilters { get; } = new()
    {
        "All Customers",
        "Registered Members",
        "Walk-in Guests"
    };

    [ObservableProperty]
    private string _selectedDateFilter = "All Time";

    public List<string> DateFilters { get; } = new()
    {
        "All Time",
        "Today",
        "Last 7 Days",
        "This Month"
    };

    [ObservableProperty]
    private string _sortBy = "Date (Newest First)";

    public List<string> SortOptions { get; } = new()
    {
        "Date (Newest First)",
        "Date (Oldest First)",
        "Total (High - Low)",
        "Total (Low - High)",
        "Receipt # (A - Z)"
    };

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
            ApplyFiltersAndSort();
        }
    }

    [RelayCommand]
    public void PreviousPage()
    {
        if (CanPreviousPage)
        {
            CurrentPage--;
            ApplyFiltersAndSort();
        }
    }

    [RelayCommand]
    public void NextPage()
    {
        if (CanNextPage)
        {
            CurrentPage++;
            ApplyFiltersAndSort();
        }
    }

    [RelayCommand]
    public void LastPage()
    {
        if (CurrentPage != TotalPages)
        {
            CurrentPage = TotalPages;
            ApplyFiltersAndSort();
        }
    }

    // --- Detail Modal Properties ---
    [ObservableProperty]
    private bool _isDetailModalOpen;

    [ObservableProperty]
    private CustomerPurchaseItemViewModel? _selectedSale;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CustomerPurchaseHistoryViewModel(
        ISalesService salesService,
        IReceiptPrinter receiptPrinter,
        INotificationService notificationService)
    {
        _salesService = salesService;
        _receiptPrinter = receiptPrinter;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadSalesHistoryAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var sales = await _salesService.GetAllSalesAsync();
            _allSalesCache.Clear();
            _allSalesCache.AddRange(sales);

            UpdateKpis();
            ApplyFiltersAndSort();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void UpdateKpis()
    {
        TotalOrdersCount = _allSalesCache.Count;
        TotalRevenue = _allSalesCache.Sum(s => s.TotalAmount);
        TotalCashSales = _allSalesCache.Where(s => s.PaymentMethod == PaymentMethod.Cash).Sum(s => s.TotalAmount);
        TotalKhqrSales = _allSalesCache.Where(s => s.PaymentMethod == PaymentMethod.KHQR).Sum(s => s.TotalAmount);
        MemberOrdersCount = _allSalesCache.Count(s => s.CustomerId.HasValue && s.CustomerId.Value > 0);
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allSalesCache.AsEnumerable();

        // 1. Search Query
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(s =>
                (!string.IsNullOrEmpty(s.ReceiptNumber) && s.ReceiptNumber.ToLowerInvariant().Contains(query)) ||
                (s.Customer != null && !string.IsNullOrEmpty(s.Customer.FullName) && s.Customer.FullName.ToLowerInvariant().Contains(query)) ||
                (s.Customer != null && !string.IsNullOrEmpty(s.Customer.PhoneNumber) && s.Customer.PhoneNumber.Contains(query)) ||
                (s.Shift?.User != null && !string.IsNullOrEmpty(s.Shift.User.FullName) && s.Shift.User.FullName.ToLowerInvariant().Contains(query)) ||
                (s.Shift?.User != null && !string.IsNullOrEmpty(s.Shift.User.Username) && s.Shift.User.Username.ToLowerInvariant().Contains(query)));
        }

        // 2. Payment Method Filter
        filtered = SelectedPaymentFilter switch
        {
            "Cash Only" => filtered.Where(s => s.PaymentMethod == PaymentMethod.Cash),
            "Bakong KHQR Only" => filtered.Where(s => s.PaymentMethod == PaymentMethod.KHQR),
            _ => filtered
        };

        // 3. Customer Type Filter
        filtered = SelectedCustomerFilter switch
        {
            "Registered Members" => filtered.Where(s => s.CustomerId.HasValue && s.CustomerId.Value > 0),
            "Walk-in Guests" => filtered.Where(s => !s.CustomerId.HasValue || s.CustomerId.Value <= 0),
            _ => filtered
        };

        // 4. Date Range Filter
        DateTime now = DateTime.UtcNow;
        filtered = SelectedDateFilter switch
        {
            "Today" => filtered.Where(s => s.CreatedAt.Date == now.Date),
            "Last 7 Days" => filtered.Where(s => s.CreatedAt >= now.AddDays(-7)),
            "This Month" => filtered.Where(s => s.CreatedAt.Year == now.Year && s.CreatedAt.Month == now.Month),
            _ => filtered
        };

        // 5. Sorting
        filtered = SortBy switch
        {
            "Date (Newest First)" => filtered.OrderByDescending(s => s.CreatedAt),
            "Date (Oldest First)" => filtered.OrderBy(s => s.CreatedAt),
            "Total (High - Low)" => filtered.OrderByDescending(s => s.TotalAmount),
            "Total (Low - High)" => filtered.OrderBy(s => s.TotalAmount),
            "Receipt # (A - Z)" => filtered.OrderBy(s => s.ReceiptNumber),
            _ => filtered.OrderByDescending(s => s.CreatedAt)
        };

        var filteredList = filtered.Select(s => new CustomerPurchaseItemViewModel(s)).ToList();
        TotalFilteredCount = filteredList.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalFilteredCount / PageSize));

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        var pagedItems = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        FilteredSales.Clear();
        foreach (var item in pagedItems)
        {
            FilteredSales.Add(item);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0
            ? "No purchase records found"
            : $"Showing {start}-{end} of {TotalFilteredCount} customer purchases";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchText = string.Empty;
        SelectedPaymentFilter = "All Payments";
        SelectedCustomerFilter = "All Customers";
        SelectedDateFilter = "All Time";
        SortBy = "Date (Newest First)";
        ApplyFiltersAndSort();
    }

    // --- Action: Print Receipt ---
    [RelayCommand]
    public async Task PrintReceiptAsync(CustomerPurchaseItemViewModel? item)
    {
        if (item == null) return;

        var printItems = item.Items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice, i.Subtotal)).ToList();
        bool printed = await _receiptPrinter.PrintReceiptAsync(
            "Smart Mart Supermarket", 
            item.ReceiptNumber, 
            printItems, 
            item.TotalAmount, 
            item.CashReceived, 
            item.ChangeGiven);

        if (printed)
        {
            StatusMessage = $"Receipt for order '{item.ReceiptNumber}' printed successfully!";
            _notificationService.ShowSuccess(
                $"Receipt for order '{item.ReceiptNumber}' ({item.TotalDisplay}) printed successfully!", 
                "Receipt Printed 🖨️");
        }
        else
        {
            StatusMessage = $"Failed to print receipt for '{item.ReceiptNumber}'.";
            _notificationService.ShowError($"Could not print receipt for '{item.ReceiptNumber}'.");
        }
    }

    // --- Action: View Detail Modal ---
    [RelayCommand]
    public void OpenDetail(CustomerPurchaseItemViewModel? item)
    {
        if (item == null) return;
        SelectedSale = item;
        IsDetailModalOpen = true;
    }

    [RelayCommand]
    public void CloseDetail()
    {
        IsDetailModalOpen = false;
        SelectedSale = null;
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedPaymentFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedCustomerFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedDateFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSortByChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSort(); }
}
