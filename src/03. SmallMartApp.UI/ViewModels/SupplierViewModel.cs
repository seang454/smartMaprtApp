using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Suppliers;

namespace SmallMartApp.UI.ViewModels;

public partial class SupplierViewModel : ViewModelBase
{
    private readonly ISupplierService _supplierService;
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;
    private readonly List<Supplier> _allSuppliersCache = new();
    private bool _isLoading;

    public ObservableCollection<Supplier> Suppliers { get; } = new();
    public ObservableCollection<PurchaseOrder> RecentOrders { get; } = new();

    // --- Search & Sort Controls ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _sortBy = "Company (A - Z)";

    public List<string> SortOptions { get; } = new()
    {
        "Company (A - Z)",
        "Company (Z - A)",
        "Newest First",
        "Oldest First"
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

    // --- Create Supplier Properties ---
    [ObservableProperty]
    private string _newCompanyName = string.Empty;

    [ObservableProperty]
    private string _newContactPerson = string.Empty;

    [ObservableProperty]
    private string _newPhoneNumber = string.Empty;

    [ObservableProperty]
    private string _newAddress = string.Empty;

    // --- Edit Supplier (Update) Properties ---
    [ObservableProperty]
    private bool _isEditSupplierOpen;

    [ObservableProperty]
    private int _editSupplierId;

    [ObservableProperty]
    private string _editCompanyName = string.Empty;

    [ObservableProperty]
    private string _editContactPerson = string.Empty;

    [ObservableProperty]
    private string _editPhoneNumber = string.Empty;

    [ObservableProperty]
    private string _editAddress = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SupplierViewModel(
        ISupplierService supplierService,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _supplierService = supplierService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadSuppliersAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var items = await _supplierService.GetAllAsync();
            _allSuppliersCache.Clear();
            _allSuppliersCache.AddRange(items);
            ApplyFiltersAndSort();

            RecentOrders.Clear();
            var orders = await _supplierService.GetRecentOrdersAsync();
            foreach (var order in orders) RecentOrders.Add(order);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allSuppliersCache.AsEnumerable();

        // 1. Search query
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(s => s.CompanyName.ToLowerInvariant().Contains(query)
                                        || (s.ContactPerson != null && s.ContactPerson.ToLowerInvariant().Contains(query))
                                        || (s.PhoneNumber != null && s.PhoneNumber.Contains(query))
                                        || (s.Address != null && s.Address.ToLowerInvariant().Contains(query)));
        }

        // 2. Sorting
        filtered = SortBy switch
        {
            "Company (A - Z)" => filtered.OrderBy(s => s.CompanyName),
            "Company (Z - A)" => filtered.OrderByDescending(s => s.CompanyName),
            "Newest First" => filtered.OrderByDescending(s => s.Id),
            "Oldest First" => filtered.OrderBy(s => s.Id),
            _ => filtered.OrderBy(s => s.CompanyName)
        };

        var filteredList = filtered.ToList();
        TotalFilteredCount = filteredList.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalFilteredCount / PageSize));

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        var pagedItems = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Suppliers.Clear();
        foreach (var item in pagedItems)
        {
            Suppliers.Add(item);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0 
            ? "No suppliers found" 
            : $"Showing {start}-{end} of {TotalFilteredCount} suppliers";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchText = string.Empty;
        SortBy = "Company (A - Z)";
        ApplyFiltersAndSort();
    }

    // --- CRUD: CREATE ---
    [RelayCommand]
    public async Task AddSupplierAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCompanyName) || string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            StatusMessage = "Company Name and Phone are required.";
            _notificationService.ShowWarning("Company Name and Phone are required.", "Validation");
            return;
        }

        var sup = new Supplier
        {
            CompanyName = NewCompanyName.Trim(),
            ContactPerson = NewContactPerson.Trim(),
            PhoneNumber = NewPhoneNumber.Trim(),
            Address = NewAddress.Trim()
        };

        var res = await _supplierService.AddOrUpdateAsync(sup);
        if (res.IsSuccess)
        {
            StatusMessage = $"Added supplier '{sup.CompanyName}' successfully!";
            _notificationService.ShowSuccess($"Supplier '{sup.CompanyName}' added successfully!");
            NewCompanyName = string.Empty;
            NewContactPerson = string.Empty;
            NewPhoneNumber = string.Empty;
            NewAddress = string.Empty;
            await LoadSuppliersAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error adding supplier.";
            _notificationService.ShowError(res.Error ?? "Error adding supplier.");
        }
    }

    // --- CRUD: UPDATE (EDIT) ---
    [RelayCommand]
    public void OpenEditSupplier(Supplier sup)
    {
        if (sup == null) return;
        EditSupplierId = sup.Id;
        EditCompanyName = sup.CompanyName;
        EditContactPerson = sup.ContactPerson ?? string.Empty;
        EditPhoneNumber = sup.PhoneNumber ?? string.Empty;
        EditAddress = sup.Address ?? string.Empty;
        IsEditSupplierOpen = true;
    }

    [RelayCommand]
    public void CloseEditSupplier()
    {
        IsEditSupplierOpen = false;
    }

    [RelayCommand]
    public async Task SaveEditSupplierAsync()
    {
        if (string.IsNullOrWhiteSpace(EditCompanyName) || string.IsNullOrWhiteSpace(EditPhoneNumber))
        {
            StatusMessage = "Company Name and Phone are required.";
            _notificationService.ShowWarning("Company Name and Phone are required.", "Validation");
            return;
        }

        var sup = new Supplier
        {
            Id = EditSupplierId,
            CompanyName = EditCompanyName.Trim(),
            ContactPerson = EditContactPerson.Trim(),
            PhoneNumber = EditPhoneNumber.Trim(),
            Address = EditAddress.Trim()
        };

        var res = await _supplierService.AddOrUpdateAsync(sup);
        if (!res.IsSuccess)
        {
            StatusMessage = res.Error ?? "Error updating supplier.";
            _notificationService.ShowError(res.Error ?? "Error updating supplier.");
            return;
        }

        IsEditSupplierOpen = false;
        StatusMessage = $"Updated supplier '{sup.CompanyName}' successfully!";
        _notificationService.ShowSuccess($"Supplier '{sup.CompanyName}' updated successfully!");
        await LoadSuppliersAsync();
    }

    // --- CRUD: DELETE ---
    [RelayCommand]
    public async Task DeleteSupplierAsync(Supplier sup)
    {
        if (sup == null) return;
        var res = await _supplierService.DeleteAsync(sup.Id);
        if (res.IsSuccess)
        {
            _allSuppliersCache.RemoveAll(s => s.Id == sup.Id);
            Suppliers.Remove(sup);
            StatusMessage = $"Deleted supplier '{sup.CompanyName}'.";
            _notificationService.ShowSuccess($"Supplier '{sup.CompanyName}' deleted successfully!");
        }
        else
        {
            StatusMessage = res.Error ?? $"Could not delete '{sup.CompanyName}'.";
            _notificationService.ShowError(res.Error ?? $"Could not delete '{sup.CompanyName}'.");
        }
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSortByChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSort(); }
}
