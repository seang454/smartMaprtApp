using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly SmallMartDbContext _context;

    public UserService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Result<User>> AddOrUpdateAsync(User user, string? newPassword = null)
    {
        if (string.IsNullOrWhiteSpace(user.Username))
            return Result<User>.Failure("Username is required.");

        if (string.IsNullOrWhiteSpace(user.FullName))
            return Result<User>.Failure("Full name is required.");

        var normalizedUsername = user.Username.Trim();

        // Check unique username
        var existingWithUsername = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername.ToLower() && u.Id != user.Id);

        if (existingWithUsername != null)
            return Result<User>.Failure($"Username '{normalizedUsername}' is already taken.");

        user.Username = normalizedUsername;
        user.FullName = user.FullName.Trim();

        if (user.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return Result<User>.Failure("Password is required for a new user account.");

            user.PasswordHash = newPassword.Trim();
            await _context.Users.AddAsync(user);
        }
        else
        {
            var tracked = await _context.Users.FindAsync(user.Id);
            if (tracked == null)
                return Result<User>.Failure("User not found.");

            // Update password only if a new password was provided
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordHash = newPassword.Trim();
            }
            else
            {
                user.PasswordHash = tracked.PasswordHash;
            }

            _context.Entry(tracked).CurrentValues.SetValues(user);
        }

        await _context.SaveChangesAsync();
        return Result<User>.Success(user);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return Result.Failure("User not found.");

        // Check if user is linked to shifts
        var hasShifts = await _context.CashierShifts.AnyAsync(s => s.UserId == id);
        if (hasShifts)
        {
            // Deactivate account instead of hard deleting to preserve shift audit integrity
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Result.Failure("User has shift audit records. The account has been deactivated instead of deleted to preserve history.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ToggleActiveAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return Result.Failure("User not found.");

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();
        return Result.Success();
    }
}
