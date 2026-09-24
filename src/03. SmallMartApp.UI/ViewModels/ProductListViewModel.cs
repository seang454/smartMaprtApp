using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Products;

namespace SmallMartApp.UI.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private bool _isLoading;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

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

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ProductListViewModel(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            // Load categories once if empty
            if (Categories.Count == 0)
            {
                var catList = await _categoryService.GetAllAsync();
                foreach (var c in catList) Categories.Add(c);
            }

            // Load products
            Products.Clear();
            var items = await _productService.GetAllAsync();

            var filtered = items.AsEnumerable();
            if (SelectedCategory != null && SelectedCategory.Id > 0)
            {
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
            }
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string query = SearchText.Trim().ToLowerInvariant();
                filtered = filtered.Where(p => p.Name.ToLowerInvariant().Contains(query) || p.Barcode.Contains(query));
            }

            foreach (var item in filtered)
            {
                Products.Add(item);
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBarcode) || string.IsNullOrWhiteSpace(NewName))
        {
            StatusMessage = "Barcode and Name are required.";
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
            return;
        }

        StatusMessage = $"Added '{product.Name}' successfully!";
        NewBarcode = string.Empty;
        NewName = string.Empty;
        await LoadProductsAsync();
    }

    [RelayCommand]
    public async Task DeleteProductAsync(Product product)
    {
        if (product == null) return;
        var result = await _productService.DeleteAsync(product.Id);
        if (result.IsSuccess)
        {
            Products.Remove(product);
            StatusMessage = $"Deleted '{product.Name}'.";
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!_isLoading) _ = LoadProductsAsync();
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        if (!_isLoading) _ = LoadProductsAsync();
    }
}
