using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Customers;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Hardware;
using SmallMartApp.Infrastructure.Hardware;

namespace SmallMartApp.UI.ViewModels;

public partial class PosCheckoutViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ISalesService _salesService;
    private readonly ICustomerService _customerService;
    private readonly IBarcodeScanner _barcodeScanner;
    private readonly MockBarcodeScanner _mockScanner;

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _cashReceived = 50.00m;

    [ObservableProperty]
    private decimal _changeGiven;

    [ObservableProperty]
    private PaymentMethod _selectedPayment = PaymentMethod.Cash;

    [ObservableProperty]
    private string _customerPhoneInput = string.Empty;

    [ObservableProperty]
    private Customer? _currentCustomer;

    [ObservableProperty]
    private decimal _discountAmount = 0m;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan items.";

    [ObservableProperty]
    private bool _isSuccessMessage;

    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    public decimal SubtotalAmount => CartItems.Sum(i => i.Subtotal);
    public decimal TotalAmount => Math.Max(0, SubtotalAmount - DiscountAmount);

    public PosCheckoutViewModel(
        IProductService productService,
        ISalesService salesService,
        ICustomerService customerService,
        IBarcodeScanner barcodeScanner,
        MockBarcodeScanner mockScanner)
    {
        _productService = productService;
        _salesService = salesService;
        _customerService = customerService;
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

    [RelayCommand]
    public async Task LookupCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerPhoneInput)) return;

        var customer = await _customerService.GetByPhoneAsync(CustomerPhoneInput);
        if (customer != null)
        {
            CurrentCustomer = customer;
            SetStatus($"Customer found: {customer.FullName} ({customer.Points} pts)", true);
        }
        else
        {
            CurrentCustomer = null;
            SetStatus("Customer not found. Sale will be for Walk-in guest.", false);
        }
    }

    [RelayCommand]
    public void SelectCashPayment()
    {
        SelectedPayment = PaymentMethod.Cash;
        CalculateChange();
    }

    [RelayCommand]
    public void SelectKhqrPayment()
    {
        SelectedPayment = PaymentMethod.KHQR;
        CashReceived = TotalAmount;
        ChangeGiven = 0;
    }

    private async Task AddProductByBarcodeAsync(string barcode)
    {
        var product = await _productService.GetByBarcodeAsync(barcode);
        if (product == null)
        {
            SetStatus($"Product barcode '{barcode}' not found.", false);
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
            cartItem.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(SubtotalAmount));
                OnPropertyChanged(nameof(TotalAmount));
                CalculateChange();
            };
            CartItems.Add(cartItem);
        }

        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
        SetStatus($"Added '{product.Name}' to cart.", true);
    }

    [RelayCommand]
    public void IncreaseQuantity(CartItemViewModel item)
    {
        item.Quantity++;
        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    [RelayCommand]
    public void DecreaseQuantity(CartItemViewModel item)
    {
        if (item.Quantity > 1) item.Quantity--;
        else CartItems.Remove(item);

        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    [RelayCommand]
    public void RemoveCartItem(CartItemViewModel item)
    {
        CartItems.Remove(item);
        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    [RelayCommand]
    public void ClearCart()
    {
        CartItems.Clear();
        CurrentCustomer = null;
        CustomerPhoneInput = string.Empty;
        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
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

        var saleRequests = CartItems.Select(i => (i.ProductId, i.Quantity)).ToList();
        var result = await _salesService.ProcessSaleAsync(
            saleRequests, 
            CashReceived, 
            shiftId: 1, 
            customerId: CurrentCustomer?.Id, 
            discountAmount: DiscountAmount, 
            paymentMethod: SelectedPayment);

        if (!result.IsSuccess)
        {
            SetStatus(result.Error ?? "Sale failed.", false);
            return;
        }

        var sale = result.Value!;
        ChangeGiven = sale.ChangeGiven;
        SetStatus($"Sale completed! Receipt: {sale.ReceiptNumber}. Change: ${sale.ChangeGiven:F2}", true);
        CartItems.Clear();
        CurrentCustomer = null;
        CustomerPhoneInput = string.Empty;
        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
    }

    partial void OnCashReceivedChanged(decimal value) => CalculateChange();

    private void CalculateChange()
    {
        if (SelectedPayment == PaymentMethod.Cash && CashReceived >= TotalAmount && TotalAmount > 0)
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
