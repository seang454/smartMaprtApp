using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Suppliers;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly SmallMartDbContext _context;

    public SupplierService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<Supplier>> GetAllAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Include(s => s.PurchaseOrders)
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    public async Task<Result<Supplier>> AddOrUpdateAsync(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.CompanyName))
            return Result<Supplier>.Failure("Company name is required.");

        if (supplier.Id == 0)
        {
            await _context.Suppliers.AddAsync(supplier);
        }
        else
        {
            var tracked = await _context.Suppliers.FindAsync(supplier.Id);
            if (tracked == null)
                return Result<Supplier>.Failure("Supplier not found.");

            _context.Entry(tracked).CurrentValues.SetValues(supplier);
        }

        await _context.SaveChangesAsync();
        return Result<Supplier>.Success(supplier);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return Result.Failure("Supplier not found.");

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<PurchaseOrder>> CreateRestockOrderAsync(int supplierId, List<(int ProductId, int Qty, decimal UnitCost)> items)
    {
        if (items == null || items.Count == 0)
            return Result<PurchaseOrder>.Failure("Restock order has no items.");

        var supplier = await _context.Suppliers.FindAsync(supplierId);
        if (supplier == null) return Result<PurchaseOrder>.Failure("Supplier not found.");

        var orderItems = new List<PurchaseOrderItem>();
        decimal totalCost = 0m;

        foreach (var item in items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                // Increase inventory stock from supplier delivery!
                product.StockQuantity += item.Qty;

                decimal subtotal = item.Qty * item.UnitCost;
                totalCost += subtotal;

                orderItems.Add(new PurchaseOrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = item.Qty,
                    UnitCost = item.UnitCost
                });
            }
        }

        var order = new PurchaseOrder
        {
            SupplierId = supplierId,
            TotalCost = totalCost,
            Items = orderItems
        };

        await _context.PurchaseOrders.AddAsync(order);
        await _context.SaveChangesAsync();

        return Result<PurchaseOrder>.Success(order);
    }

    public async Task<List<PurchaseOrder>> GetRecentOrdersAsync(int count = 20)
    {
        return await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }
}
