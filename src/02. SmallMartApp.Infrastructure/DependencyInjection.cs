using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Hardware;
using SmallMartApp.Infrastructure.Hardware;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.Infrastructure.Services;

namespace SmallMartApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? dbPath = null)
    {
        dbPath ??= "smallmart.db";

        services.AddDbContext<SmallMartDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        var mockScanner = new MockBarcodeScanner();
        services.AddSingleton<IBarcodeScanner>(mockScanner);
        services.AddSingleton(mockScanner);
        services.AddSingleton<IReceiptPrinter, FakeReceiptPrinter>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISalesService, SalesService>();

        return services;
    }
}
