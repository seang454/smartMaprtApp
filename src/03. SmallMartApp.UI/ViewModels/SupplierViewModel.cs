using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Suppliers;

namespace SmallMartApp.UI.ViewModels;

public partial class SupplierViewModel : ViewModelBase
{
    private readonly ISupplierService _supplierService;

    public ObservableCollection<Supplier> Suppliers { get; } = new();
    public ObservableCollection<PurchaseOrder> RecentOrders { get; } = new();

    [ObservableProperty]
    private string _newCompanyName = string.Empty;

    [ObservableProperty]
    private string _newContactPerson = string.Empty;

    [ObservableProperty]
    private string _newPhoneNumber = string.Empty;

    [ObservableProperty]
    private string _newAddress = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SupplierViewModel(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [RelayCommand]
    public async Task LoadSuppliersAsync()
    {
        Suppliers.Clear();
        var items = await _supplierService.GetAllAsync();
        foreach (var item in items) Suppliers.Add(item);

        RecentOrders.Clear();
        var orders = await _supplierService.GetRecentOrdersAsync();
        foreach (var order in orders) RecentOrders.Add(order);
    }

    [RelayCommand]
    public async Task AddSupplierAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCompanyName) || string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            StatusMessage = "Company Name and Phone are required.";
            return;
        }

        var sup = new Supplier
        {
            CompanyName = NewCompanyName.Trim(),
            ContactPerson = NewContactPerson.Trim(),
            PhoneNumber = NewPhoneNumber.Trim(),
            Address = NewAddress.Trim()
        };

        var res = await _supplierService.AddOrUpdateAsync(sup);
        if (res.IsSuccess)
        {
            StatusMessage = $"Added supplier '{sup.CompanyName}' successfully!";
            NewCompanyName = string.Empty;
            NewContactPerson = string.Empty;
            NewPhoneNumber = string.Empty;
            NewAddress = string.Empty;
            await LoadSuppliersAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error adding supplier.";
        }
    }
}
