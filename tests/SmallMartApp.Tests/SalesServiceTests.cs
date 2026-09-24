using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Infrastructure.Hardware;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.Infrastructure.Services;
using Xunit;

namespace SmallMartApp.Tests;

public class SalesServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SmallMartDbContext _context;
    private readonly SalesService _salesService;

    public SalesServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SmallMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SmallMartDbContext(options);
        _context.Database.EnsureCreated();

        var mockPrinter = new FakeReceiptPrinter();
        _salesService = new SalesService(_context, mockPrinter);
    }

    [Fact]
    public async Task ProcessSaleAsync_ShouldDeductStock_AndCalculateChange()
    {
        var product = await _context.Products.FirstAsync(p => p.Barcode == "885012401");
        int initialStock = product.StockQuantity;
        int qtyToBuy = 2;
        decimal cashGiven = 5.00m;

        var result = await _salesService.ProcessSaleAsync(new() { (product.Id, qtyToBuy) }, cashGiven);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(initialStock - qtyToBuy, product.StockQuantity);
        Assert.Equal(qtyToBuy * product.SellPrice, result.Value.TotalAmount);
        Assert.Equal(cashGiven - result.Value.TotalAmount, result.Value.ChangeGiven);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
