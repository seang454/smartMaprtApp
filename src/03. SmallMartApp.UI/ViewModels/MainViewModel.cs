using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Auth;

namespace SmallMartApp.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly DashboardViewModel _dashboardVm;
    private readonly PosCheckoutViewModel _posVm;
    private readonly ProductListViewModel _productVm;
    private readonly CategoryViewModel _categoryVm;
    private readonly CustomerViewModel _customerVm;
    private readonly SupplierViewModel _supplierVm;
    private readonly ShiftViewModel _shiftVm;
    private readonly UserViewModel _userVm;
    private readonly CustomerPurchaseHistoryViewModel _customerPurchasesVm;

    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _activeTab = "Dashboard";

    [ObservableProperty]
    private string _currentUserName = "Admin (Manager)";

    [ObservableProperty]
    private bool _isDatabaseConnected = true;

    [ObservableProperty]
    private bool _isAdmin = true;

    public bool IsCashier => !IsAdmin;

    [ObservableProperty]
    private User? _currentUser;

    public event Action? RequestLogout;

    // Toast Alert Notification Overlay Properties
    private readonly SmallMartApp.UI.Services.INotificationService _notificationService;
    private System.Windows.Threading.DispatcherTimer? _toastTimer;

    [ObservableProperty]
    private bool _isToastVisible;

    [ObservableProperty]
    private string _toastTitle = "Success";

    [ObservableProperty]
    private string _toastMessage = string.Empty;

    [ObservableProperty]
    private string _toastIcon = "✅";

    [ObservableProperty]
    private string _toastBorderBrush = "#10B981";

    [ObservableProperty]
    private string _toastBackground = "#ECFDF5";

    [ObservableProperty]
    private string _toastForeground = "#065F46";

    public MainViewModel(
        DashboardViewModel dashboardVm,
        PosCheckoutViewModel posVm,
        ProductListViewModel productVm,
        CategoryViewModel categoryVm,
        CustomerViewModel customerVm,
        SupplierViewModel supplierVm,
        ShiftViewModel shiftVm,
        UserViewModel userVm,
        CustomerPurchaseHistoryViewModel customerPurchasesVm,
        SmallMartApp.UI.Services.INotificationService notificationService)
    {
        _dashboardVm = dashboardVm;
        _posVm = posVm;
        _productVm = productVm;
        _categoryVm = categoryVm;
        _customerVm = customerVm;
        _supplierVm = supplierVm;
        _shiftVm = shiftVm;
        _userVm = userVm;
        _customerPurchasesVm = customerPurchasesVm;
        _notificationService = notificationService;

        _currentView = _dashboardVm;

        _notificationService.NotificationReceived += OnNotificationReceived;
        _posVm.RequestNavigateToShifts += () => _ = NavigateToShiftsAsync();
    }

    private void OnNotificationReceived(SmallMartApp.UI.Services.NotificationItem item)
    {
        ToastTitle = item.Title;
        ToastMessage = item.Message;

        switch (item.Type)
        {
            case SmallMartApp.UI.Services.NotificationType.Success:
                ToastIcon = "✅";
                ToastBackground = "#ECFDF5";
                ToastBorderBrush = "#10B981";
                ToastForeground = "#065F46";
                break;
            case SmallMartApp.UI.Services.NotificationType.Error:
                ToastIcon = "❌";
                ToastBackground = "#FEF2F2";
                ToastBorderBrush = "#EF4444";
                ToastForeground = "#991B1B";
                break;
            case SmallMartApp.UI.Services.NotificationType.Warning:
                ToastIcon = "⚠️";
                ToastBackground = "#FFFBEB";
                ToastBorderBrush = "#F59E0B";
                ToastForeground = "#92400E";
                break;
            case SmallMartApp.UI.Services.NotificationType.Info:
            default:
                ToastIcon = "ℹ️";
                ToastBackground = "#EFF6FF";
                ToastBorderBrush = "#3B82F6";
                ToastForeground = "#1E40AF";
                break;
        }

        IsToastVisible = true;

        _toastTimer?.Stop();
        _toastTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(item.DurationMs)
        };
        _toastTimer.Tick += (s, e) =>
        {
            IsToastVisible = false;
            _toastTimer.Stop();
        };
        _toastTimer.Start();
    }

    [RelayCommand]
    public void DismissToast()
    {
        IsToastVisible = false;
        _toastTimer?.Stop();
    }

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
        IsAdmin = user.Role == UserRole.Admin;
        OnPropertyChanged(nameof(IsCashier));
        CurrentUserName = $"{user.FullName} ({user.Role})";
        _shiftVm.SetCurrentUser(user);
        _posVm.SetCurrentUser(user);
        _productVm.SetCurrentUser(user);

        if (user.Role == UserRole.Cashier)
        {
            CurrentView = _posVm;
            ActiveTab = "POS";
        }
        else
        {
            CurrentView = _dashboardVm;
            ActiveTab = "Dashboard";
            _ = _dashboardVm.LoadDashboardCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    public void Logout()
    {
        RequestLogout?.Invoke();
    }

    [RelayCommand]
    public async Task NavigateToDashboardAsync()
    {
        if (!IsAdmin) return;
        CurrentView = _dashboardVm;
        ActiveTab = "Dashboard";
        await _dashboardVm.LoadDashboardCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToPosAsync()
    {
        CurrentView = _posVm;
        ActiveTab = "POS";
        await _posVm.RefreshActiveShiftCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToProductsAsync()
    {
        CurrentView = _productVm;
        ActiveTab = "Products";
        await _productVm.LoadProductsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToCategoriesAsync()
    {
        if (!IsAdmin) return;
        CurrentView = _categoryVm;
        ActiveTab = "Categories";
        await _categoryVm.LoadCategoriesCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToCustomersAsync()
    {
        CurrentView = _customerVm;
        ActiveTab = "Customers";
        await _customerVm.LoadCustomersCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToCustomerPurchasesAsync()
    {
        CurrentView = _customerPurchasesVm;
        ActiveTab = "Purchases";
        await _customerPurchasesVm.LoadSalesHistoryCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToSuppliersAsync()
    {
        if (!IsAdmin) return;
        CurrentView = _supplierVm;
        ActiveTab = "Suppliers";
        await _supplierVm.LoadSuppliersCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToShiftsAsync()
    {
        CurrentView = _shiftVm;
        ActiveTab = "Shifts";
        await _shiftVm.RefreshShiftCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToUsersAsync()
    {
        if (!IsAdmin) return;
        CurrentView = _userVm;
        ActiveTab = "Users";
        await _userVm.LoadUsersCommand.ExecuteAsync(null);
    }
}
