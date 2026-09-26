using System;
using System.Windows;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.UI.ViewModels;

namespace SmallMartApp.UI.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly MainViewModel _mainViewModel;
    private readonly MainWindow _mainWindow;

    public LoginWindow(LoginViewModel viewModel, MainViewModel mainViewModel, MainWindow mainWindow)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _mainViewModel = mainViewModel;
        _mainWindow = mainWindow;
        DataContext = _viewModel;

        _viewModel.LoginSuccessful += OnLoginSuccessful;
        _mainViewModel.RequestLogout += OnLogoutRequested;
    }

    private void OnLoginSuccessful(User user)
    {
        _mainViewModel.SetCurrentUser(user);
        _mainWindow.Show();
        this.Hide();
    }

    private void OnLogoutRequested()
    {
        _mainWindow.Hide();
        _viewModel.BackToWelcome();
        this.Show();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_mainWindow.IsVisible)
        {
            Application.Current.Shutdown();
        }
    }
}
