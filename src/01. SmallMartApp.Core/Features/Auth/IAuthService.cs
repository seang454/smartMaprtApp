using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Auth;

public interface IAuthService
{
    Task<Result<User>> LoginAsync(string username, string password);
}
