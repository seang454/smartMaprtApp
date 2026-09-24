using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmallMartApp.Infrastructure;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.UI.ViewModels;

namespace SmallMartApp.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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

                // Register all UI ViewModels
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<PosCheckoutViewModel>();
                services.AddSingleton<ProductListViewModel>();
                services.AddSingleton<CategoryViewModel>();
                services.AddSingleton<CustomerViewModel>();
                services.AddSingleton<SupplierViewModel>();
                services.AddSingleton<ShiftViewModel>();
                services.AddSingleton<MainViewModel>();

                // Register Shell Window
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        // Ensure SQL Server database & seed data are initialized
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SmallMartDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
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
