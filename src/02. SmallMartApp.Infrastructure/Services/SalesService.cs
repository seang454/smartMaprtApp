using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Hardware;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class SalesService : ISalesService
{
    private readonly SmallMartDbContext _context;
    private readonly IReceiptPrinter _receiptPrinter;

    public SalesService(SmallMartDbContext context, IReceiptPrinter receiptPrinter)
    {
        _context = context;
        _receiptPrinter = receiptPrinter;
    }

    public async Task<Result<Sale>> ProcessSaleAsync(List<(int ProductId, int Quantity)> items, decimal cashReceived)
    {
        if (items == null || items.Count == 0)
            return Result<Sale>.Failure("Cart is empty.");

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        decimal totalAmount = 0m;
        var saleItems = new List<SaleItem>();
        var receiptPrintItems = new List<(string ItemName, int Qty, decimal Price, decimal Subtotal)>();

        foreach (var item in items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                return Result<Sale>.Failure($"Product with ID {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                return Result<Sale>.Failure($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}");

            product.StockQuantity -= item.Quantity;

            decimal subtotal = item.Quantity * product.SellPrice;
            totalAmount += subtotal;

            saleItems.Add(new SaleItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.SellPrice
            });

            receiptPrintItems.Add((product.Name, item.Quantity, product.SellPrice, subtotal));
        }

        if (cashReceived < totalAmount)
            return Result<Sale>.Failure($"Insufficient cash. Required: ${totalAmount:F2}, Received: ${cashReceived:F2}");

        decimal changeGiven = cashReceived - totalAmount;
        string receiptNumber = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var sale = new Sale
        {
            ReceiptNumber = receiptNumber,
            TotalAmount = totalAmount,
            CashReceived = cashReceived,
            ChangeGiven = changeGiven,
            Items = saleItems
        };

        await _context.Sales.AddAsync(sale);
        await _context.SaveChangesAsync();

        _ = _receiptPrinter.PrintReceiptAsync("Small Mart Kiosk", receiptNumber, receiptPrintItems, totalAmount, cashReceived, changeGiven);

        return Result<Sale>.Success(sale);
    }

    public async Task<List<Sale>> GetRecentSalesAsync(int count = 20)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
