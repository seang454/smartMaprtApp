using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Products;

namespace SmallMartApp.UI.ViewModels;

public partial class CategoryViewModel : ViewModelBase
{
    private readonly ICategoryService _categoryService;
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;
    private readonly List<Category> _allCategoriesCache = new();
    private bool _isLoading;

    public ObservableCollection<Category> Categories { get; } = new();

    // --- Search & Sort Controls ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _sortBy = "Name (A - Z)";

    public List<string> SortOptions { get; } = new()
    {
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

    // --- Create Category Properties ---
    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    // --- Edit Category (Update) Properties ---
    [ObservableProperty]
    private bool _isEditCategoryOpen;

    [ObservableProperty]
    private int _editCategoryId;

    [ObservableProperty]
    private string _editCategoryName = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CategoryViewModel(
        ICategoryService categoryService,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _categoryService = categoryService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            var items = await _categoryService.GetAllAsync();
            _allCategoriesCache.Clear();
            _allCategoriesCache.AddRange(items);
            ApplyFiltersAndSort();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allCategoriesCache.AsEnumerable();

        // 1. Search Query
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string query = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(c => c.Name.ToLowerInvariant().Contains(query) 
                                        || (c.Description != null && c.Description.ToLowerInvariant().Contains(query)));
        }

        // 2. Sorting
        filtered = SortBy switch
        {
            "Name (A - Z)" => filtered.OrderBy(c => c.Name),
            "Name (Z - A)" => filtered.OrderByDescending(c => c.Name),
            "Newest First" => filtered.OrderByDescending(c => c.Id),
            "Oldest First" => filtered.OrderBy(c => c.Id),
            _ => filtered.OrderBy(c => c.Name)
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

        Categories.Clear();
        foreach (var item in pagedItems)
        {
            Categories.Add(item);
        }

        int start = TotalFilteredCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
        int end = Math.Min(CurrentPage * PageSize, TotalFilteredCount);
        PaginationInfo = TotalFilteredCount == 0 
            ? "No categories found" 
            : $"Showing {start}-{end} of {TotalFilteredCount} categories";
        PageDisplay = $"Page {CurrentPage} of {TotalPages}";

        OnPropertyChanged(nameof(CanPreviousPage));
        OnPropertyChanged(nameof(CanNextPage));
    }

    [RelayCommand]
    public void ResetFilters()
    {
        CurrentPage = 1;
        SearchText = string.Empty;
        SortBy = "Name (A - Z)";
        ApplyFiltersAndSort();
    }

    // --- CRUD: CREATE ---
    [RelayCommand]
    public async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            StatusMessage = "Category name is required.";
            _notificationService.ShowWarning("Category name is required.", "Validation");
            return;
        }

        var cat = new Category 
        { 
            Name = NewCategoryName.Trim(), 
            Description = NewDescription.Trim() 
        };
        
        var res = await _categoryService.AddOrUpdateAsync(cat);
        if (res.IsSuccess)
        {
            StatusMessage = $"Added category '{cat.Name}' successfully!";
            _notificationService.ShowSuccess($"Category '{cat.Name}' created successfully!");
            NewCategoryName = string.Empty;
            NewDescription = string.Empty;
            await LoadCategoriesAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error adding category.";
            _notificationService.ShowError(res.Error ?? "Error adding category.");
        }
    }

    // --- CRUD: UPDATE (EDIT) ---
    [RelayCommand]
    public void OpenEditCategory(Category cat)
    {
        if (cat == null) return;
        EditCategoryId = cat.Id;
        EditCategoryName = cat.Name;
        EditDescription = cat.Description ?? string.Empty;
        IsEditCategoryOpen = true;
    }

    [RelayCommand]
    public void CloseEditCategory()
    {
        IsEditCategoryOpen = false;
    }

    [RelayCommand]
    public async Task SaveEditCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(EditCategoryName))
        {
            StatusMessage = "Category name is required.";
            _notificationService.ShowWarning("Category name is required.", "Validation");
            return;
        }

        var cat = new Category
        {
            Id = EditCategoryId,
            Name = EditCategoryName.Trim(),
            Description = EditDescription.Trim()
        };

        var res = await _categoryService.AddOrUpdateAsync(cat);
        if (!res.IsSuccess)
        {
            StatusMessage = res.Error ?? "Error updating category.";
            _notificationService.ShowError(res.Error ?? "Error updating category.");
            return;
        }

        IsEditCategoryOpen = false;
        StatusMessage = $"Updated category '{cat.Name}' successfully!";
        _notificationService.ShowSuccess($"Category '{cat.Name}' updated successfully!");
        await LoadCategoriesAsync();
    }

    // --- CRUD: DELETE ---
    [RelayCommand]
    public async Task DeleteCategoryAsync(Category cat)
    {
        if (cat == null) return;
        var res = await _categoryService.DeleteAsync(cat.Id);
        if (res.IsSuccess)
        {
            _allCategoriesCache.RemoveAll(c => c.Id == cat.Id);
            Categories.Remove(cat);
            StatusMessage = $"Deleted category '{cat.Name}'.";
            _notificationService.ShowSuccess($"Category '{cat.Name}' deleted successfully!");
        }
        else
        {
            StatusMessage = res.Error ?? $"Could not delete '{cat.Name}'.";
            _notificationService.ShowError(res.Error ?? $"Could not delete '{cat.Name}'.");
        }
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnSortByChanged(string value) { CurrentPage = 1; ApplyFiltersAndSort(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; ApplyFiltersAndSort(); }
}
