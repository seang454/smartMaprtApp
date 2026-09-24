using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Suppliers;

public interface ISupplierService
{
    Task<List<Supplier>> GetAllAsync();
    Task<Result<Supplier>> AddOrUpdateAsync(Supplier supplier);
    Task<Result<PurchaseOrder>> CreateRestockOrderAsync(int supplierId, List<(int ProductId, int Qty, decimal UnitCost)> items);
    Task<List<PurchaseOrder>> GetRecentOrdersAsync(int count = 20);
}
