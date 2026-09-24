using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Products;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Result<Category>> AddOrUpdateAsync(Category category);
    Task<Result> DeleteAsync(int id);
}
