using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Core.Features.Products;

namespace SmallMartApp.UI.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;
    private readonly List<Product> _allProductsCache = new();
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAdmin = true;

    public void SetCurrentUser(User user)
    {
        IsAdmin = user.Role == UserRole.Admin;
    }

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    // --- Search, Filter & Sort Controls ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private string _stockFilter = "All Stock";

    [ObservableProperty]
    private string _sortBy = "Newest First";

    public List<string> StockFilterOptions { get; } = new()
    {
        "All Stock",
        "In Stock (> 5)",
        "Low Stock (1 - 5)",
        "Out of Stock (0)"
    };

    public List<string> SortOptions { get; } = new()
    {
        "Newest First",
        "Name (A - Z)",
        "Name (Z - A)",
        "Price (Low - High)",
        "Price (High - Low)",
        "Stock (Low - High)",
        "Stock (High - Low)"
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

    // --- Create Product Properties ---
    [ObservableProperty]
    private string _newBarcode = string.Empty;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private decimal _newCostPrice = 0.50m;

    [ObservableProperty]
    private decimal _newSellPrice = 1.00m;

    [ObservableProperty]
    private int _newStockQuantity = 20;

    [ObservableProperty]
    private Category? _newProductCategory;

    // --- Edit Product (Update) Properties ---
    [ObservableProperty]
    private bool _isEditProductOpen;

    [ObservableProperty]
    private int _editProductId;

    [ObservableProperty]
    private string _editBarcode = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private decimal _editCostPrice;

    [ObservableProperty]
    private decimal _editSellPrice;

    [ObservableProperty]
    private int _editStockQuantity;

    [ObservableProperty]
    private Category? _editProductCategory;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ProductListViewModel(
        IProductService productService, 
        ICategoryService categoryService,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            if (Categories.Count == 0)
            {
                var catList = await _categoryService.GetAllAsync();
                foreach (var c in catList) Categories.Add(c);
            }

            var items = await _productService.GetAllAsync();
            _allProductsCache.Clear();
            _allProductsCache.AddRange(items);

            ApplyFiltersAndSort();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allProductsCache.AsEnumerable();

        // 1. Search Query (Name or Barcode)
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(p => p.Name.ToLowerInvariant().Contains(query) || p.Barcode.Contains(query));
        }

        // 2. Category Filter
        if (SelectedCategory != null && SelectedCategory.Id > 0)
        {
            filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
        }

        // 3. Stock Level Filter
        filtered = StockFilter switch
        {
            "In Stock (> 5)" => filtered.Where(p => p.StockQuantity > 5),
            "Low Stock (1 - 5)" => filtered.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 5),
            "Out of Stock (0)" => filtered.Where(p => p.StockQuantity <= 0),
            _ => filtered
        };

        // 4. Dynamic Sorting
        filtered = SortBy switch
        {
            "Name (A - Z)" => filtered.OrderBy(p => p.Name),
            "Name (Z - A)" => filtered.OrderByDescending(p => p.Name),
            "Price (Low - High)" => filtered.OrderBy(p => p.SellPrice),
            "Price (High - Low)" => filtered.OrderByDescending(p => p.SellPrice),
            "Stock (Low - High)" => filtered.OrderBy(p => p.StockQuantity),
            "Stock (High - Low)" => filtered.OrderByDescending(p => p.StockQuantity),
            "Newest First" => filtered.OrderByDescending(p => p.Id),
            _ => filtered.OrderByDescending(p => p.Id)
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

        Products.Clear();
        foreach (var item in pagedItems)
        {
            Products.Add(item);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0 
            ? "No products found" 
            : $"Showing {start}-{end} of {TotalFilteredCount} products";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchText = string.Empty;
        SelectedCategory = null;
        StockFilter = "All Stock";
        SortBy = "Newest First";
        ApplyFiltersAndSort();
    }

    // --- CRUD: CREATE ---
    [RelayCommand]
    public async Task AddProductAsync()
    {
        if (!IsAdmin)
        {
            _notificationService.ShowWarning("Only administrators can add new products.", "Permission Denied");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewBarcode) || string.IsNullOrWhiteSpace(NewName))
        {
            StatusMessage = "Barcode and Name are required.";
            _notificationService.ShowWarning("Barcode and Name are required.", "Validation");
            return;
        }

        var product = new Product
        {
            Barcode = NewBarcode.Trim(),
            Name = NewName.Trim(),
            CostPrice = NewCostPrice,
            SellPrice = NewSellPrice,
            StockQuantity = NewStockQuantity,
            CategoryId = NewProductCategory?.Id
        };

        var result = await _productService.AddOrUpdateAsync(product);
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error ?? "Error adding product.";
            _notificationService.ShowError(result.Error ?? "Error adding product.");
            return;
        }

        StatusMessage = $"Added '{product.Name}' successfully!";
        _notificationService.ShowSuccess($"Product '{product.Name}' added successfully!");
        NewBarcode = string.Empty;
        NewName = string.Empty;
        await LoadProductsAsync();
    }

    // --- CRUD: UPDATE (EDIT) ---
    [RelayCommand]
    public void OpenEditProduct(Product product)
    {
        if (!IsAdmin)
        {
            _notificationService.ShowWarning("Only administrators can edit products.", "Permission Denied");
            return;
        }

        if (product == null) return;
        EditProductId = product.Id;
        EditBarcode = product.Barcode;
        EditName = product.Name;
        EditCostPrice = product.CostPrice;
        EditSellPrice = product.SellPrice;
        EditStockQuantity = product.StockQuantity;
        EditProductCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        IsEditProductOpen = true;
    }

    [RelayCommand]
    public void CloseEditProduct()
    {
        IsEditProductOpen = false;
    }

    [RelayCommand]
    public async Task SaveEditProductAsync()
    {
        if (!IsAdmin)
        {
            _notificationService.ShowWarning("Only administrators can modify products.", "Permission Denied");
            return;
        }

        if (string.IsNullOrWhiteSpace(EditBarcode) || string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Barcode and Name are required.";
            _notificationService.ShowWarning("Barcode and Name are required.", "Validation");
            return;
        }

        var product = new Product
        {
            Id = EditProductId,
            Barcode = EditBarcode.Trim(),
            Name = EditName.Trim(),
            CostPrice = EditCostPrice,
            SellPrice = EditSellPrice,
            StockQuantity = EditStockQuantity,
            CategoryId = EditProductCategory?.Id
        };

        var result = await _productService.AddOrUpdateAsync(product);
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error ?? "Error updating product.";
            _notificationService.ShowError(result.Error ?? "Error updating product.");
            return;
        }

        IsEditProductOpen = false;
        StatusMessage = $"Updated '{product.Name}' successfully!";
        _notificationService.ShowSuccess($"Product '{product.Name}' updated successfully!");
        await LoadProductsAsync();
    }

    // --- CRUD: DELETE ---
    [RelayCommand]
    public async Task DeleteProductAsync(Product product)
    {
        if (!IsAdmin)
        {
            _notificationService.ShowWarning("Only administrators can delete products.", "Permission Denied");
            return;
        }

        if (product == null) return;
        var result = await _productService.DeleteAsync(product.Id);
        if (result.IsSuccess)
        {
            _allProductsCache.RemoveAll(p => p.Id == product.Id);
            Products.Remove(product);
            StatusMessage = $"Deleted '{product.Name}'.";
            _notificationService.ShowSuccess($"Product '{product.Name}' deleted successfully!");
        }
        else
        {
            StatusMessage = result.Error ?? $"Could not delete '{product.Name}'.";
            _notificationService.ShowError(result.Error ?? $"Could not delete '{product.Name}'.");
        }
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSelectedCategoryChanged(Category? value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnStockFilterChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSortByChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSort(); }
}
