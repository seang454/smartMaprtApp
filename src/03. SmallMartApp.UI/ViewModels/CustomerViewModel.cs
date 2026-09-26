using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Customers;

namespace SmallMartApp.UI.ViewModels;

public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;
    private readonly List<Customer> _allCustomersCache = new();
    private bool _isLoading;

    public ObservableCollection<Customer> Customers { get; } = new();

    // --- Search, Filter & Sort Controls ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedPointsFilter = "All Customers";

    public List<string> PointsFilters { get; } = new()
    {
        "All Customers",
        "Has Points (> 0)",
        "Tier 100+ pts",
        "Zero Points"
    };

    [ObservableProperty]
    private string _sortBy = "Points (High - Low)";

    public List<string> SortOptions { get; } = new()
    {
        "Points (High - Low)",
        "Points (Low - High)",
        "Name (A - Z)",
        "Name (Z - A)",
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

    // --- Create Customer Properties ---
    [ObservableProperty]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    private string _newPhoneNumber = string.Empty;

    // --- Edit Customer (Update) Properties ---
    [ObservableProperty]
    private bool _isEditCustomerOpen;

    [ObservableProperty]
    private int _editCustomerId;

    [ObservableProperty]
    private string _editFullName = string.Empty;

    [ObservableProperty]
    private string _editPhoneNumber = string.Empty;

    [ObservableProperty]
    private int _editPoints;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CustomerViewModel(
        ICustomerService customerService,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _customerService = customerService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadCustomersAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var items = await _customerService.GetAllAsync();
            _allCustomersCache.Clear();
            _allCustomersCache.AddRange(items);
            ApplyFiltersAndSort();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allCustomersCache.AsEnumerable();

        // 1. Search Query
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(c => 
                (!string.IsNullOrEmpty(c.FullName) && c.FullName.ToLowerInvariant().Contains(query)) ||
                (!string.IsNullOrEmpty(c.PhoneNumber) && c.PhoneNumber.Contains(query)));
        }

        // 2. Points Filter
        filtered = SelectedPointsFilter switch
        {
            "Has Points (> 0)" => filtered.Where(c => c.Points > 0),
            "Tier 100+ pts" => filtered.Where(c => c.Points >= 100),
            "Zero Points" => filtered.Where(c => c.Points == 0),
            _ => filtered
        };

        // 3. Sorting
        filtered = SortBy switch
        {
            "Points (High - Low)" => filtered.OrderByDescending(c => c.Points).ThenBy(c => c.FullName),
            "Points (Low - High)" => filtered.OrderBy(c => c.Points).ThenBy(c => c.FullName),
            "Name (A - Z)" => filtered.OrderBy(c => c.FullName),
            "Name (Z - A)" => filtered.OrderByDescending(c => c.FullName),
            "Newest First" => filtered.OrderByDescending(c => c.Id),
            "Oldest First" => filtered.OrderBy(c => c.Id),
            _ => filtered.OrderByDescending(c => c.Points)
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

        Customers.Clear();
        foreach (var item in pagedItems)
        {
            Customers.Add(item);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0 
            ? "No customers found" 
            : $"Showing {start}-{end} of {TotalFilteredCount} customers";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchText = string.Empty;
        SelectedPointsFilter = "All Customers";
        SortBy = "Points (High - Low)";
        ApplyFiltersAndSort();
    }

    // --- CRUD: CREATE ---
    [RelayCommand]
    public async Task AddCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFullName) || string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            StatusMessage = "Name and Phone number are required.";
            _notificationService.ShowWarning("Name and Phone number are required.", "Validation");
            return;
        }

        var customer = new Customer
        {
            FullName = NewFullName.Trim(),
            PhoneNumber = NewPhoneNumber.Trim(),
            Points = 0
        };

        var res = await _customerService.AddOrUpdateAsync(customer);
        if (res.IsSuccess)
        {
            StatusMessage = $"Registered customer '{customer.FullName}' successfully!";
            _notificationService.ShowSuccess($"Customer '{customer.FullName}' registered successfully!");
            NewFullName = string.Empty;
            NewPhoneNumber = string.Empty;
            await LoadCustomersAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error adding customer.";
            _notificationService.ShowError(res.Error ?? "Error adding customer.");
        }
    }

    // --- CRUD: UPDATE (EDIT) ---
    [RelayCommand]
    public void OpenEditCustomer(Customer cust)
    {
        if (cust == null) return;
        EditCustomerId = cust.Id;
        EditFullName = cust.FullName;
        EditPhoneNumber = cust.PhoneNumber;
        EditPoints = cust.Points;
        IsEditCustomerOpen = true;
    }

    [RelayCommand]
    public void CloseEditCustomer()
    {
        IsEditCustomerOpen = false;
    }

    [RelayCommand]
    public async Task SaveEditCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(EditFullName) || string.IsNullOrWhiteSpace(EditPhoneNumber))
        {
            StatusMessage = "Customer Name and Phone number are required.";
            _notificationService.ShowWarning("Customer Name and Phone number are required.", "Validation");
            return;
        }

        var cust = new Customer
        {
            Id = EditCustomerId,
            FullName = EditFullName.Trim(),
            PhoneNumber = EditPhoneNumber.Trim(),
            Points = EditPoints < 0 ? 0 : EditPoints
        };

        var res = await _customerService.AddOrUpdateAsync(cust);
        if (!res.IsSuccess)
        {
            StatusMessage = res.Error ?? "Error updating customer.";
            _notificationService.ShowError(res.Error ?? "Error updating customer.");
            return;
        }

        IsEditCustomerOpen = false;
        StatusMessage = $"Updated customer '{cust.FullName}' successfully!";
        _notificationService.ShowSuccess($"Customer '{cust.FullName}' updated successfully!");
        await LoadCustomersAsync();
    }

    // --- CRUD: DELETE ---
    [RelayCommand]
    public async Task DeleteCustomerAsync(Customer cust)
    {
        if (cust == null) return;
        var res = await _customerService.DeleteAsync(cust.Id);
        if (res.IsSuccess)
        {
            _allCustomersCache.RemoveAll(c => c.Id == cust.Id);
            Customers.Remove(cust);
            StatusMessage = $"Deleted customer '{cust.FullName}'.";
            _notificationService.ShowSuccess($"Customer '{cust.FullName}' deleted successfully!");
        }
        else
        {
            StatusMessage = res.Error ?? $"Could not delete '{cust.FullName}'.";
            _notificationService.ShowError(res.Error ?? $"Could not delete '{cust.FullName}'.");
        }
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedPointsFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSortByChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSort(); }
}
