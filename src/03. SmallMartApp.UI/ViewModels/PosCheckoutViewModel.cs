using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Hardware;
using SmallMartApp.Infrastructure.Hardware;

namespace SmallMartApp.UI.ViewModels;

public partial class PosCheckoutViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ISalesService _salesService;
    private readonly IBarcodeScanner _barcodeScanner;
    private readonly MockBarcodeScanner _mockScanner;

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _cashReceived;

    [ObservableProperty]
    private decimal _changeGiven;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan items.";

    [ObservableProperty]
    private bool _isSuccessMessage;

    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    public decimal TotalAmount => CartItems.Sum(i => i.Subtotal);

    public PosCheckoutViewModel(
        IProductService productService,
        ISalesService salesService,
        IBarcodeScanner barcodeScanner,
        MockBarcodeScanner mockScanner)
    {
        _productService = productService;
        _salesService = salesService;
        _barcodeScanner = barcodeScanner;
        _mockScanner = mockScanner;

        _barcodeScanner.BarcodeScanned += OnBarcodeScanned;
    }

    private void OnBarcodeScanned(object? sender, string barcode)
    {
        Application.Current?.Dispatcher.Invoke(async () =>
        {
            BarcodeInput = barcode;
            await AddProductByBarcodeAsync(barcode);
        });
    }

    [RelayCommand]
    public async Task AddBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput)) return;
        await AddProductByBarcodeAsync(BarcodeInput);
        BarcodeInput = string.Empty;
    }

    [RelayCommand]
    public void SimulateQuickScan(string sampleBarcode)
    {
        _mockScanner.SimulateScan(sampleBarcode);
    }

    private async Task AddProductByBarcodeAsync(string barcode)
    {
        var product = await _productService.GetByBarcodeAsync(barcode);
        if (product == null)
        {
            SetStatus($"Product with barcode '{barcode}' not found.", false);
            return;
        }

        if (product.StockQuantity <= 0)
        {
            SetStatus($"'{product.Name}' is out of stock!", false);
            return;
        }

        var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            if (existingItem.Quantity + 1 > product.StockQuantity)
            {
                SetStatus($"Cannot add more '{product.Name}'. Only {product.StockQuantity} in stock.", false);
                return;
            }
            existingItem.Quantity++;
        }
        else
        {
            var cartItem = new CartItemViewModel
            {
                ProductId = product.Id,
                Barcode = product.Barcode,
                Name = product.Name,
                UnitPrice = product.SellPrice,
                Quantity = 1
            };
            cartItem.PropertyChanged += (s, e) => OnPropertyChanged(nameof(TotalAmount));
            CartItems.Add(cartItem);
        }

        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
        SetStatus($"Added '{product.Name}' to cart.", true);
    }

    [RelayCommand]
    public void IncreaseQuantity(CartItemViewModel item)
    {
        item.Quantity++;
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    [RelayCommand]
    public void DecreaseQuantity(CartItemViewModel item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            CartItems.Remove(item);
        }
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    [RelayCommand]
    public void RemoveCartItem(CartItemViewModel item)
    {
        CartItems.Remove(item);
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    [RelayCommand]
    public void ClearCart()
    {
        CartItems.Clear();
        CashReceived = 0;
        ChangeGiven = 0;
        OnPropertyChanged(nameof(TotalAmount));
        SetStatus("Cart cleared.", true);
    }

    [RelayCommand]
    public async Task CompleteSaleAsync()
    {
        if (CartItems.Count == 0)
        {
            SetStatus("Cart is empty.", false);
            return;
        }

        if (CashReceived < TotalAmount)
        {
            SetStatus($"Please enter at least ${TotalAmount:F2} cash.", false);
            return;
        }

        var saleRequests = CartItems.Select(i => (i.ProductId, i.Quantity)).ToList();
        var result = await _salesService.ProcessSaleAsync(saleRequests, CashReceived);

        if (!result.IsSuccess)
        {
            SetStatus(result.Error ?? "Sale failed.", false);
            return;
        }

        var sale = result.Value!;
        ChangeGiven = sale.ChangeGiven;
        SetStatus($"Sale completed! Receipt: {sale.ReceiptNumber}. Change: ${sale.ChangeGiven:F2}", true);
        CartItems.Clear();
        OnPropertyChanged(nameof(TotalAmount));
    }

    partial void OnCashReceivedChanged(decimal value) => CalculateChange();

    private void CalculateChange()
    {
        if (CashReceived >= TotalAmount && TotalAmount > 0)
            ChangeGiven = CashReceived - TotalAmount;
        else
            ChangeGiven = 0;
    }

    private void SetStatus(string message, bool isSuccess)
    {
        StatusMessage = message;
        IsSuccessMessage = isSuccess;
    }
}
