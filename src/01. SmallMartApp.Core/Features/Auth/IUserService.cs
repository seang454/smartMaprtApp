using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Auth;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<Result<User>> AddOrUpdateAsync(User user, string? newPassword = null);
    Task<Result> DeleteAsync(int id);
    Task<Result> ToggleActiveAsync(int id);
}
