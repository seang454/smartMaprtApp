using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SmallMartDbContext _context;

    public AuthService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<Result<User>> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Result<User>.Failure("Username and password are required.");

        var input = username.Trim().ToLower();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => (u.Username.ToLower() == input || u.FullName.ToLower() == input) && u.IsActive);

        if (user == null)
            return Result<User>.Failure("User not found or inactive.");

        if (user.PasswordHash != password)
            return Result<User>.Failure("Invalid password.");

        return Result<User>.Success(user);
    }
}
