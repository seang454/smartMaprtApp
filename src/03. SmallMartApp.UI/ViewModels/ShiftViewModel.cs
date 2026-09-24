using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Shifts;

namespace SmallMartApp.UI.ViewModels;

public partial class ShiftViewModel : ViewModelBase
{
    private readonly IShiftService _shiftService;

    [ObservableProperty]
    private CashierShift? _activeShift;

    [ObservableProperty]
    private bool _hasActiveShift;

    [ObservableProperty]
    private decimal _startingCashInput = 50.00m;

    [ObservableProperty]
    private decimal _actualCashInput = 0m;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<CashierShift> RecentShifts { get; } = new();

    public ShiftViewModel(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [RelayCommand]
    public async Task RefreshShiftAsync()
    {
        // Currently default to user ID 2 (cashier1)
        ActiveShift = await _shiftService.GetCurrentActiveShiftAsync(2);
        HasActiveShift = ActiveShift != null;

        RecentShifts.Clear();
        var shifts = await _shiftService.GetRecentShiftsAsync();
        foreach (var s in shifts) RecentShifts.Add(s);
    }

    [RelayCommand]
    public async Task OpenShiftAsync()
    {
        var res = await _shiftService.OpenShiftAsync(2, StartingCashInput);
        if (res.IsSuccess)
        {
            StatusMessage = "Shift opened successfully! Cash drawer ready.";
            await RefreshShiftAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Could not open shift.";
        }
    }

    [RelayCommand]
    public async Task CloseShiftAsync()
    {
        if (ActiveShift == null) return;

        var res = await _shiftService.CloseShiftAsync(ActiveShift.Id, ActualCashInput);
        if (res.IsSuccess)
        {
            decimal diff = (res.Value?.ActualCash ?? 0) - (res.Value?.ExpectedCash ?? 0);
            StatusMessage = $"Shift closed! Discrepancy: ${diff:F2}";
            await RefreshShiftAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Could not close shift.";
        }
    }
}
