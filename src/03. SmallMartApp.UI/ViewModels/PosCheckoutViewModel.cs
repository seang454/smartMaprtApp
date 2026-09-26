using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Core.Features.Customers;
using SmallMartApp.Core.Features.Payments;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Features.Shifts;
using SmallMartApp.Core.Hardware;
using SmallMartApp.Infrastructure.Hardware;

namespace SmallMartApp.UI.ViewModels;

public partial class PosCheckoutViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private readonly ISalesService _salesService;
    private readonly ICustomerService _customerService;
    private readonly IShiftService _shiftService;
    private readonly IKhqrService _khqrService;
    private readonly IBarcodeScanner _barcodeScanner;
    private readonly MockBarcodeScanner _mockScanner;
    private readonly IReceiptPrinter _receiptPrinter;
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;
    private User? _currentUser;

    public event Action? RequestNavigateToShifts;

    [ObservableProperty]
    private bool _hasActiveShift;

    [ObservableProperty]
    private CashierShift? _currentActiveShift;

    [ObservableProperty]
    private bool _isShiftGateOpen;

    [ObservableProperty]
    private decimal _openingFloatInput = 50.00m;

    [ObservableProperty]
    private string _activeShiftSummary = "Drawer Shift Closed";

    [ObservableProperty]
    private string _activeShiftCashierDisplay = string.Empty;

    public void SetCurrentUser(User? user)
    {
        _currentUser = user;
        ActiveShiftCashierDisplay = user != null ? $"{user.FullName} ({user.Role})" : "No Cashier";
        _ = RefreshActiveShiftAsync();
    }

    [RelayCommand]
    public async Task RefreshActiveShiftAsync()
    {
        if (_currentUser == null)
        {
            HasActiveShift = false;
            CurrentActiveShift = null;
            IsShiftGateOpen = true;
            ActiveShiftSummary = "No Cashier Logged In";
            return;
        }

        var shift = await _shiftService.GetCurrentActiveShiftAsync(_currentUser.Id);
        if (shift != null)
        {
            HasActiveShift = true;
            CurrentActiveShift = shift;
            IsShiftGateOpen = false;
            ActiveShiftSummary = $"Shift #{shift.Id} Active • Started: {shift.StartTime:hh:mm tt} • Float: ${shift.StartingCash:F2}";
            ActiveShiftCashierDisplay = $"{_currentUser.FullName} ({_currentUser.Role})";
        }
        else
        {
            HasActiveShift = false;
            CurrentActiveShift = null;
            IsShiftGateOpen = true;
            ActiveShiftSummary = "Cash Drawer Shift Closed";
            ActiveShiftCashierDisplay = $"{_currentUser.FullName} ({_currentUser.Role})";
        }
    }

    [RelayCommand]
    public void SetFloatPreset(string amountStr)
    {
        if (decimal.TryParse(amountStr, out var amt))
        {
            OpeningFloatInput = amt;
        }
    }

    [RelayCommand]
    public async Task OpenShiftFromPosAsync()
    {
        if (_currentUser == null)
        {
            _notificationService.ShowError("User is not authenticated.", "Authentication");
            return;
        }

        if (OpeningFloatInput < 0)
        {
            _notificationService.ShowWarning("Starting cash float cannot be negative.", "Validation");
            return;
        }

        var res = await _shiftService.OpenShiftAsync(_currentUser.Id, OpeningFloatInput);
        if (res.IsSuccess)
        {
            IsShiftGateOpen = false;
            _notificationService.ShowSuccess(
                $"Cash drawer opened! Shift #{res.Value!.Id} started with ${OpeningFloatInput:F2} cash float.", 
                "Shift Started Successfully 🎉");
            await RefreshActiveShiftAsync();
        }
        else
        {
            _notificationService.ShowError(res.Error ?? "Could not open shift.", "Shift Error");
        }
    }

    [RelayCommand]
    public void OpenShiftGate()
    {
        IsShiftGateOpen = true;
    }

    [RelayCommand]
    public void CloseShiftGate()
    {
        IsShiftGateOpen = false;
    }

    [RelayCommand]
    public void GoToShiftReconciliation()
    {
        IsShiftGateOpen = false;
        RequestNavigateToShifts?.Invoke();
    }

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _cashReceived = 0m;

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
    private int _pointsToRedeem = 0;

    [ObservableProperty]
    private string _pointRedemptionFeedback = string.Empty;

    public bool HasCustomerWithPoints => CurrentCustomer != null && CurrentCustomer.Points > 0;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan items.";

    [ObservableProperty]
    private bool _isSuccessMessage;

    [ObservableProperty]
    private bool _isQuickRegisterOpen;

    [ObservableProperty]
    private string _quickRegisterName = string.Empty;

    [ObservableProperty]
    private string _quickRegisterPhone = string.Empty;

    [ObservableProperty]
    private bool _isKhqrModalOpen;

    [ObservableProperty]
    private BitmapImage? _khqrImageSource;

    [ObservableProperty]
    private string _khqrPayload = string.Empty;

    [ObservableProperty]
    private string _khqrStoreName = "SIM PENGSEANG";

    [ObservableProperty]
    private string _khqrBakongId = "016901360@aclb";

    [ObservableProperty]
    private string _khqrAmountDisplay = "$0.00";

    [ObservableProperty]
    private string _khqrCurrency = "USD";

    // --- PAYMENT SUCCESS MODAL ALERT PROPERTIES ---
    [ObservableProperty]
    private bool _isPaymentSuccessModalOpen;

    [ObservableProperty]
    private string _lastReceiptNumber = string.Empty;

    [ObservableProperty]
    private string _lastPaymentMethod = string.Empty;

    [ObservableProperty]
    private decimal _lastTotalPaid = 0m;

    [ObservableProperty]
    private string _lastTotalPaidDisplay = string.Empty;

    [ObservableProperty]
    private decimal _lastCashReceived = 0m;

    [ObservableProperty]
    private decimal _lastChangeGiven = 0m;

    [ObservableProperty]
    private string _lastCustomerSummary = string.Empty;

    [ObservableProperty]
    private string _lastCashierName = string.Empty;

    [ObservableProperty]
    private string _lastSaleTimeDisplay = string.Empty;

    [ObservableProperty]
    private string _lastLoyaltyPointsEarned = string.Empty;

    [ObservableProperty]
    private string _lastItemsCountDisplay = string.Empty;

    private List<(string ItemName, int Qty, decimal Price, decimal Subtotal)> _lastPrintItems = new();

    public System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    public decimal SubtotalAmount => CartItems.Sum(i => i.Subtotal);
    public decimal TotalAmount => Math.Max(0, SubtotalAmount - DiscountAmount);

    partial void OnCurrentCustomerChanged(Customer? value)
    {
        PointsToRedeem = 0;
        DiscountAmount = 0;
        PointRedemptionFeedback = string.Empty;
        OnPropertyChanged(nameof(HasCustomerWithPoints));
        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    partial void OnPointsToRedeemChanged(int value)
    {
        if (CurrentCustomer == null || CurrentCustomer.Points <= 0)
        {
            DiscountAmount = 0;
            PointRedemptionFeedback = string.Empty;
            OnPropertyChanged(nameof(TotalAmount));
            CalculateChange();
            return;
        }

        int maxAllowedByBalance = CurrentCustomer.Points;
        int maxAllowedBySubtotal = (int)(SubtotalAmount * 100);
        int effectiveMax = Math.Min(maxAllowedByBalance, maxAllowedBySubtotal);

        int clamped = Math.Clamp(value, 0, effectiveMax);
        if (clamped != value)
        {
            PointsToRedeem = clamped;
            return;
        }

        // 100 points = $1.00 USD (1 pt = $0.01)
        DiscountAmount = Math.Round(clamped / 100.00m, 2);
        PointRedemptionFeedback = clamped > 0
            ? $"🎁 Redeeming {clamped} pts (-${DiscountAmount:F2} discount)"
            : string.Empty;

        OnPropertyChanged(nameof(TotalAmount));
        CalculateChange();
    }

    public PosCheckoutViewModel(
        IProductService productService,
        ISalesService salesService,
        ICustomerService customerService,
        IShiftService shiftService,
        IKhqrService khqrService,
        IBarcodeScanner barcodeScanner,
        MockBarcodeScanner mockScanner,
        IReceiptPrinter receiptPrinter,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _productService = productService;
        _salesService = salesService;
        _customerService = customerService;
        _shiftService = shiftService;
        _khqrService = khqrService;
        _barcodeScanner = barcodeScanner;
        _mockScanner = mockScanner;
        _receiptPrinter = receiptPrinter;
        _notificationService = notificationService;

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
            SetStatus($"Customer '{CustomerPhoneInput}' not found. Click '+ New Member' to sign them up!", false);
        }
    }

    [RelayCommand]
    public void OpenQuickRegister()
    {
        QuickRegisterPhone = CustomerPhoneInput.Trim();
        QuickRegisterName = string.Empty;
        IsQuickRegisterOpen = true;
    }

    [RelayCommand]
    public void CloseQuickRegister()
    {
        IsQuickRegisterOpen = false;
    }

    [RelayCommand]
    public async Task SaveQuickRegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(QuickRegisterName) || string.IsNullOrWhiteSpace(QuickRegisterPhone))
        {
            SetStatus("Both Name and Phone number are required.", false);
            return;
        }

        var customer = new Customer
        {
            FullName = QuickRegisterName.Trim(),
            PhoneNumber = QuickRegisterPhone.Trim(),
            Points = 0
        };

        var res = await _customerService.AddOrUpdateAsync(customer);
        if (res.IsSuccess)
        {
            CurrentCustomer = customer;
            CustomerPhoneInput = customer.PhoneNumber;
            IsQuickRegisterOpen = false;
            SetStatus($"Registered & linked new member: {customer.FullName} (0 pts)!", true);
        }
        else
        {
            SetStatus(res.Error ?? "Error registering customer.", false);
        }
    }

    [RelayCommand]
    public void SelectCashPayment()
    {
        SelectedPayment = PaymentMethod.Cash;
        CashReceived = TotalAmount;
        CalculateChange();
    }

    [RelayCommand]
    public void SelectKhqrPayment()
    {
        if (TotalAmount <= 0)
        {
            SetStatus("Cart is empty. Please add items to scan KHQR.", false);
            return;
        }

        SelectedPayment = PaymentMethod.KHQR;
        CashReceived = TotalAmount;
        ChangeGiven = 0;

        GenerateCurrentKhqr();
        IsKhqrModalOpen = true;
        SetStatus("Bakong KHQR code generated! Ready for customer to scan.", true);
    }

    public void GenerateCurrentKhqr()
    {
        if (TotalAmount <= 0 || string.IsNullOrWhiteSpace(KhqrBakongId)) return;

        try
        {
            decimal effectiveAmount = TotalAmount;
            if (KhqrCurrency == "KHR")
            {
                // Official standard approx exchange rate or 4,100 KHR / USD
                effectiveAmount = Math.Round(TotalAmount * 4100m, 0);
            }

            KhqrPayload = _khqrService.GenerateKhqrString(
                KhqrBakongId.Trim(), 
                KhqrStoreName, 
                "Phnom Penh", 
                effectiveAmount, 
                KhqrCurrency, 
                $"REC-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                mobileNumber: "016901360",
                merchantRoutingId: "85521566549",
                acquiringBank: "ACLEDA");

            byte[] pngBytes = _khqrService.GenerateQrCodePng(KhqrPayload, pixelsPerModule: 10);
            KhqrImageSource = LoadBitmapImageFromBytes(pngBytes);
            KhqrAmountDisplay = KhqrCurrency == "KHR" 
                ? $"{effectiveAmount:N0} KHR (៛)" 
                : $"${TotalAmount:F2} USD";
        }
        catch (Exception ex)
        {
            SetStatus($"Error generating KHQR code: {ex.Message}", false);
        }
    }

    [RelayCommand]
    public void SetKhqrUsd()
    {
        KhqrCurrency = "USD";
        GenerateCurrentKhqr();
    }

    [RelayCommand]
    public void SetKhqrKhr()
    {
        KhqrCurrency = "KHR";
        GenerateCurrentKhqr();
    }

    partial void OnKhqrBakongIdChanged(string value)
    {
        if (IsKhqrModalOpen) GenerateCurrentKhqr();
    }

    [RelayCommand]
    public void CloseKhqrModal()
    {
        IsKhqrModalOpen = false;
    }

    [RelayCommand]
    public async Task ConfirmKhqrPaymentAsync()
    {
        IsKhqrModalOpen = false;
        await CompleteSaleAsync();
    }

    private static BitmapImage LoadBitmapImageFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
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
    public void SetExactCash()
    {
        CashReceived = TotalAmount;
        CalculateChange();
    }

    [RelayCommand]
    public void RedeemMaxPoints()
    {
        if (CurrentCustomer == null || CurrentCustomer.Points <= 0 || SubtotalAmount <= 0) return;
        int maxPointsForSubtotal = (int)(SubtotalAmount * 100);
        PointsToRedeem = Math.Min(CurrentCustomer.Points, maxPointsForSubtotal);
    }

    [RelayCommand]
    public void ClearPoints()
    {
        PointsToRedeem = 0;
    }

    [RelayCommand]
    public async Task CompleteSaleAsync()
    {
        if (CartItems.Count == 0)
        {
            SetStatus("Cart is empty.", false);
            return;
        }

        // Enforce active shift requirement
        if (!HasActiveShift || CurrentActiveShift == null)
        {
            IsShiftGateOpen = true;
            _notificationService.ShowWarning(
                "An active cashier shift is required to balance drawer cash and complete checkout.", 
                "Shift Required");
            SetStatus("Please start your cashier shift before checkout.", false);
            return;
        }

        int shiftId = CurrentActiveShift.Id;

        var saleRequests = CartItems.Select(i => (i.ProductId, i.Quantity)).ToList();
        var result = await _salesService.ProcessSaleAsync(
            saleRequests, 
            CashReceived, 
            shiftId: shiftId, 
            customerId: CurrentCustomer?.Id, 
            discountAmount: DiscountAmount, 
            paymentMethod: SelectedPayment,
            pointsRedeemed: PointsToRedeem);

        if (!result.IsSuccess)
        {
            _notificationService.ShowError(result.Error ?? "Sale failed to process.");
            SetStatus(result.Error ?? "Sale failed.", false);
            return;
        }

        var sale = result.Value!;
        ChangeGiven = sale.ChangeGiven;

        // Populate Payment Success Modal Data
        LastReceiptNumber = sale.ReceiptNumber;
        LastPaymentMethod = SelectedPayment == PaymentMethod.KHQR ? "Bakong KHQR (ACLEDA)" : "Cash Payment";
        LastTotalPaid = sale.TotalAmount;
        LastTotalPaidDisplay = SelectedPayment == PaymentMethod.KHQR && KhqrCurrency == "KHR"
            ? $"{Math.Round(sale.TotalAmount * 4100m, 0):N0} ៛ (KHR)"
            : $"${sale.TotalAmount:F2} USD";
        LastCashReceived = SelectedPayment == PaymentMethod.Cash ? CashReceived : sale.TotalAmount;
        LastChangeGiven = sale.ChangeGiven;
        LastCustomerSummary = CurrentCustomer != null ? $"{CurrentCustomer.FullName} ({CurrentCustomer.PhoneNumber})" : "Walk-in Guest";
        LastCashierName = _currentUser?.FullName ?? "Cashier";
        LastSaleTimeDisplay = sale.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        LastLoyaltyPointsEarned = CurrentCustomer != null ? $"+{(int)Math.Floor(sale.TotalAmount)} pts" : "0 pts";
        LastItemsCountDisplay = $"{saleRequests.Sum(r => r.Quantity)} items";

        // Open Payment Success Alert Modal & Trigger Toast
        IsKhqrModalOpen = false;
        IsPaymentSuccessModalOpen = true;

        _notificationService.ShowSuccess(
            $"Payment of {LastTotalPaidDisplay} received! Receipt: {sale.ReceiptNumber}", 
            "Payment Successful! 🎉");

        SetStatus($"Sale completed! Receipt: {sale.ReceiptNumber}. Change: ${sale.ChangeGiven:F2}", true);
        _lastPrintItems = CartItems.Select(i => (i.Name, i.Quantity, i.UnitPrice, i.Subtotal)).ToList();
        CartItems.Clear();
        CurrentCustomer = null;
        CustomerPhoneInput = string.Empty;
        PointsToRedeem = 0;
        DiscountAmount = 0;
        PointRedemptionFeedback = string.Empty;
        OnPropertyChanged(nameof(SubtotalAmount));
        OnPropertyChanged(nameof(TotalAmount));
    }

    [RelayCommand]
    public void ClosePaymentSuccessModal()
    {
        IsPaymentSuccessModalOpen = false;
    }

    [RelayCommand]
    public async Task PrintLastReceiptAsync()
    {
        if (string.IsNullOrWhiteSpace(LastReceiptNumber)) return;
        await _receiptPrinter.PrintReceiptAsync(
            "Smart Mart Supermarket",
            LastReceiptNumber,
            _lastPrintItems,
            LastTotalPaid,
            LastCashReceived,
            LastChangeGiven);
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
