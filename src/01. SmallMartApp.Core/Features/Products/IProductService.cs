using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Products;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<Result<Product>> AddOrUpdateAsync(Product product);
    Task<Result> DeleteAsync(int id);
}
