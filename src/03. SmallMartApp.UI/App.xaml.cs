using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmallMartApp.Infrastructure;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.UI.Helpers;
using SmallMartApp.UI.ViewModels;
using SmallMartApp.UI.Views;

namespace SmallMartApp.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Enable copy across all UI elements, tables, and cards
        GlobalCopyHelper.Initialize();

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                string connectionString = context.Configuration.GetConnectionString("DefaultConnection") 
                    ?? DependencyInjection.DefaultSqlServerConnection;

                // Register Infrastructure & Services
                services.AddInfrastructure(connectionString);

                // Register UI Notification & Alert System
                services.AddSingleton<SmallMartApp.UI.Services.INotificationService, SmallMartApp.UI.Services.NotificationService>();

                // Register Virtual Thermal Receipt Printer Simulator (replaces console fake printer with interactive UI)
                services.AddSingleton<SmallMartApp.Core.Hardware.IReceiptPrinter, SmallMartApp.UI.Services.WpfReceiptPrinter>();

                // Register all UI ViewModels
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<PosCheckoutViewModel>();
                services.AddSingleton<ProductListViewModel>();
                services.AddSingleton<CategoryViewModel>();
                services.AddSingleton<CustomerViewModel>();
                services.AddSingleton<SupplierViewModel>();
                services.AddSingleton<ShiftViewModel>();
                services.AddSingleton<UserViewModel>();
                services.AddSingleton<CustomerPurchaseHistoryViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<LoginViewModel>();

                // Register Windows
                services.AddSingleton<MainWindow>();
                services.AddSingleton<LoginWindow>();
            })
            .Build();

        await _host.StartAsync();

        // Ensure SQL Server database & seed data are initialized
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SmallMartDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
