using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _activeTab = "Dashboard";

    [ObservableProperty]
    private string _currentUserName = "Admin (Manager)";

    public MainViewModel(
        DashboardViewModel dashboardVm,
        PosCheckoutViewModel posVm,
        ProductListViewModel productVm,
        CategoryViewModel categoryVm,
        CustomerViewModel customerVm,
        SupplierViewModel supplierVm,
        ShiftViewModel shiftVm)
    {
        _dashboardVm = dashboardVm;
        _posVm = posVm;
        _productVm = productVm;
        _categoryVm = categoryVm;
        _customerVm = customerVm;
        _supplierVm = supplierVm;
        _shiftVm = shiftVm;

        _currentView = _dashboardVm;
        _ = _dashboardVm.LoadDashboardCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public async Task NavigateToDashboardAsync()
    {
        CurrentView = _dashboardVm;
        ActiveTab = "Dashboard";
        await _dashboardVm.LoadDashboardCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    public void NavigateToPos()
    {
        CurrentView = _posVm;
        ActiveTab = "POS";
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
    public async Task NavigateToSuppliersAsync()
    {
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
}
