using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Auth;

namespace SmallMartApp.UI.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoginFormVisible;

    public event Action<User>? LoginSuccessful;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public void ShowLoginForm()
    {
        ErrorMessage = string.Empty;
        HasError = false;
        IsLoginFormVisible = true;
    }

    [RelayCommand]
    public void BackToWelcome()
    {
        ErrorMessage = string.Empty;
        HasError = false;
        IsLoginFormVisible = false;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = string.Empty;
        HasError = false;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);
            if (result.IsSuccess && result.Value != null)
            {
                LoginSuccessful?.Invoke(result.Value);
            }
            else
            {
                ErrorMessage = result.Error ?? "Invalid username or password.";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task QuickLoginAdminAsync()
    {
        Username = "admin";
        Password = "admin123";
        await LoginAsync();
    }

    [RelayCommand]
    public async Task QuickLoginCashierAsync()
    {
        Username = "cashier1";
        Password = "123456";
        await LoginAsync();
    }
}
