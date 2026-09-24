using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly SmallMartDbContext _context;

    public ProductService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Barcode == barcode.Trim() && p.IsActive);
    }

    public async Task<Result<Product>> AddOrUpdateAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return Result<Product>.Failure("Product name cannot be empty.");

        if (string.IsNullOrWhiteSpace(product.Barcode))
            return Result<Product>.Failure("Barcode cannot be empty.");

        if (product.SellPrice <= 0)
            return Result<Product>.Failure("Sell price must be greater than zero.");

        var existing = await _context.Products
            .FirstOrDefaultAsync(p => p.Barcode == product.Barcode && p.Id != product.Id);

        if (existing != null)
            return Result<Product>.Failure($"A product with barcode '{product.Barcode}' already exists.");

        if (product.Id == 0)
        {
            await _context.Products.AddAsync(product);
        }
        else
        {
            _context.Products.Update(product);
        }

        await _context.SaveChangesAsync();
        return Result<Product>.Success(product);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return Result.Failure("Product not found.");

        product.IsActive = false;
        await _context.SaveChangesAsync();
        return Result.Success();
    }
}
