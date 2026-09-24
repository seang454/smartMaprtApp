using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Products;

namespace SmallMartApp.UI.ViewModels;

public partial class ProductListViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    public ObservableCollection<Product> Products { get; } = new();

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
    private string _statusMessage = string.Empty;

    public ProductListViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        Products.Clear();
        var items = await _productService.GetAllAsync();
        foreach (var item in items)
        {
            Products.Add(item);
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
            StockQuantity = NewStockQuantity
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
}
