using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Customers;

namespace SmallMartApp.UI.ViewModels;

public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;

    public ObservableCollection<Customer> Customers { get; } = new();

    [ObservableProperty]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    private string _newPhoneNumber = string.Empty;

    [ObservableProperty]
    private string _searchPhone = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CustomerViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [RelayCommand]
    public async Task LoadCustomersAsync()
    {
        Customers.Clear();
        var items = await _customerService.GetAllAsync();
        foreach (var item in items) Customers.Add(item);
    }

    [RelayCommand]
    public async Task AddCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFullName) || string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            StatusMessage = "Name and Phone number are required.";
            return;
        }

        var customer = new Customer
        {
            FullName = NewFullName.Trim(),
            PhoneNumber = NewPhoneNumber.Trim(),
            Points = 0
        };

        var res = await _customerService.AddOrUpdateAsync(customer);
        if (res.IsSuccess)
        {
            StatusMessage = $"Added customer '{customer.FullName}' successfully!";
            NewFullName = string.Empty;
            NewPhoneNumber = string.Empty;
            await LoadCustomersAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error adding customer.";
        }
    }
}
