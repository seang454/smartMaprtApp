using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly SmallMartDbContext _context;

    public CategoryService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Result<Category>> AddOrUpdateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return Result<Category>.Failure("Category name is required.");

        if (category.Id == 0)
        {
            await _context.Categories.AddAsync(category);
        }
        else
        {
            var tracked = await _context.Categories.FindAsync(category.Id);
            if (tracked == null)
                return Result<Category>.Failure("Category not found.");

            _context.Entry(tracked).CurrentValues.SetValues(category);
        }

        await _context.SaveChangesAsync();
        return Result<Category>.Success(category);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat == null) return Result.Failure("Category not found.");

        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync();
        return Result.Success();
    }
}
