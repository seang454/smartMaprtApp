using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMartApp.Core.Features.Customers;
using SmallMartApp.Core.Features.Dashboard;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Features.Shifts;
using SmallMartApp.Core.Features.Suppliers;
using SmallMartApp.Core.Hardware;
using SmallMartApp.Infrastructure.Hardware;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.Infrastructure.Services;

namespace SmallMartApp.Infrastructure;

public static class DependencyInjection
{
    public const string DefaultSqlServerConnection = 
        "Server=localhost\\SQLEXPRESS;Database=SmallMartDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString = null)
    {
        connectionString ??= DefaultSqlServerConnection;

        // Use Transient lifetime for DbContext so each service/operation gets an isolated, thread-safe instance
        services.AddDbContext<SmallMartDbContext>(
            options => options.UseSqlServer(connectionString), 
            ServiceLifetime.Transient, 
            ServiceLifetime.Transient);

        // Hardware Services (Singletons)
        var mockScanner = new MockBarcodeScanner();
        services.AddSingleton<IBarcodeScanner>(mockScanner);
        services.AddSingleton(mockScanner);
        services.AddSingleton<IReceiptPrinter, FakeReceiptPrinter>();

        // Domain Business Services (Transient to prevent DbContext sharing concurrency issues in WPF)
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IProductService, ProductService>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<ISupplierService, SupplierService>();
        services.AddTransient<IShiftService, ShiftService>();
        services.AddTransient<ISalesService, SalesService>();
        services.AddTransient<IDashboardService, DashboardService>();

        return services;
    }
}
