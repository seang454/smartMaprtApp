using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Features.Shifts;
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

    public async Task<Result<Sale>> ProcessSaleAsync(
        List<(int ProductId, int Quantity)> items, 
        decimal cashReceived, 
        int? shiftId = null, 
        int? customerId = null, 
        decimal discountAmount = 0m, 
        PaymentMethod paymentMethod = PaymentMethod.Cash,
        int pointsRedeemed = 0)
    {
        if (items == null || items.Count == 0)
            return Result<Sale>.Failure("Cart is empty.");

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        decimal subtotalAmount = 0m;
        var saleItems = new List<SaleItem>();
        var receiptPrintItems = new List<(string ItemName, int Qty, decimal Price, decimal Subtotal)>();

        foreach (var item in items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                return Result<Sale>.Failure($"Product with ID {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                return Result<Sale>.Failure($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}");

            product.StockQuantity -= item.Quantity;

            decimal lineSubtotal = item.Quantity * product.SellPrice;
            subtotalAmount += lineSubtotal;

            saleItems.Add(new SaleItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.SellPrice
            });

            receiptPrintItems.Add((product.Name, item.Quantity, product.SellPrice, lineSubtotal));
        }

        decimal finalTotal = Math.Max(0, subtotalAmount - discountAmount);

        if (paymentMethod == PaymentMethod.Cash && cashReceived < finalTotal)
            return Result<Sale>.Failure($"Insufficient cash. Required: ${finalTotal:F2}, Received: ${cashReceived:F2}");

        decimal changeGiven = paymentMethod == PaymentMethod.Cash ? Math.Max(0, cashReceived - finalTotal) : 0m;
        string receiptNumber = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var sale = new Sale
        {
            ReceiptNumber = receiptNumber,
            ShiftId = shiftId,
            CustomerId = customerId,
            TotalAmount = finalTotal,
            DiscountAmount = discountAmount,
            CashReceived = paymentMethod == PaymentMethod.Cash ? cashReceived : finalTotal,
            ChangeGiven = changeGiven,
            PaymentMethod = paymentMethod,
            Items = saleItems
        };

        await _context.Sales.AddAsync(sale);

        // Deduct redeemed points and award newly earned loyalty points
        if (customerId.HasValue && customerId.Value > 0)
        {
            var customer = await _context.Customers.FindAsync(customerId.Value);
            if (customer != null)
            {
                if (pointsRedeemed > 0)
                {
                    customer.Points = Math.Max(0, customer.Points - pointsRedeemed);
                }

                customer.Points += (int)finalTotal;
            }
        }

        // Update active shift Expected Cash in real-time for cash payments
        if (shiftId.HasValue && shiftId.Value > 0)
        {
            var shift = await _context.CashierShifts.FindAsync(shiftId.Value);
            if (shift != null && shift.Status == ShiftStatus.Open && paymentMethod == PaymentMethod.Cash)
            {
                shift.ExpectedCash += finalTotal;
            }
        }

        await _context.SaveChangesAsync();

        // Note: Receipt printing is triggered on-demand by the POS Terminal or Purchase History UI via IReceiptPrinter.
        return Result<Sale>.Success(sale);
    }

    public async Task<List<Sale>> GetRecentSalesAsync(int count = 20)
    {
        return await _context.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .Include(s => s.Shift)
                .ThenInclude(sh => sh!.User)
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetAllSalesAsync()
    {
        return await _context.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .Include(s => s.Shift)
                .ThenInclude(sh => sh!.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Sale?> GetSaleByIdAsync(int id)
    {
        return await _context.Sales
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .Include(s => s.Shift)
                .ThenInclude(sh => sh!.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
