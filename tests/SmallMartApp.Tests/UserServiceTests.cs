using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Core.Features.Shifts;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.Infrastructure.Services;
using Xunit;

namespace SmallMartApp.Tests;

public class UserServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SmallMartDbContext _context;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SmallMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SmallMartDbContext(options);
        _context.Database.EnsureCreated();

        _userService = new UserService(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsInitialSeededUsers()
    {
        var users = await _userService.GetAllAsync();

        Assert.NotNull(users);
        Assert.True(users.Count >= 2);
        Assert.Contains(users, u => u.Username == "admin");
        Assert.Contains(users, u => u.Username == "cashier1");
    }

    [Fact]
    public async Task AddOrUpdateAsync_WithNewValidCashier_AddsUser()
    {
        var newUser = new User
        {
            Username = "cashier2",
            FullName = "David Kim",
            Role = UserRole.Cashier,
            WorkingShift = "Afternoon",
            IsActive = true
        };

        var result = await _userService.AddOrUpdateAsync(newUser, "pass123");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Id > 0);

        var retrieved = await _userService.GetByIdAsync(result.Value.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("David Kim", retrieved.FullName);
        Assert.Equal("pass123", retrieved.PasswordHash);
        Assert.Equal(UserRole.Cashier, retrieved.Role);
    }

    [Fact]
    public async Task AddOrUpdateAsync_WithDuplicateUsername_Fails()
    {
        var duplicate = new User
        {
            Username = "admin",
            FullName = "Another Admin",
            Role = UserRole.Admin,
            WorkingShift = "Morning"
        };

        var result = await _userService.AddOrUpdateAsync(duplicate, "admin123");

        Assert.False(result.IsSuccess);
        Assert.Contains("already taken", result.Error);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpdateExistingUser_ModifiesData()
    {
        var existing = await _context.Users.FirstAsync(u => u.Username == "cashier1");
        existing.FullName = "Sreymao Updated";
        existing.WorkingShift = "Night";

        var result = await _userService.AddOrUpdateAsync(existing);

        Assert.True(result.IsSuccess);

        var refreshed = await _userService.GetByIdAsync(existing.Id);
        Assert.NotNull(refreshed);
        Assert.Equal("Sreymao Updated", refreshed.FullName);
        Assert.Equal("Night", refreshed.WorkingShift);
        Assert.Equal("123456", refreshed.PasswordHash); // Kept password
    }

    [Fact]
    public async Task ToggleActiveAsync_TogglesUserStatus()
    {
        var user = await _context.Users.FirstAsync(u => u.Username == "cashier1");
        bool originalStatus = user.IsActive;

        var result = await _userService.ToggleActiveAsync(user.Id);
        Assert.True(result.IsSuccess);

        var updated = await _userService.GetByIdAsync(user.Id);
        Assert.NotNull(updated);
        Assert.Equal(!originalStatus, updated.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_UserWithShiftAudit_DeactivatesInsteadOfHardDeleting()
    {
        // Add a shift for cashier1
        var user = await _context.Users.FirstAsync(u => u.Username == "cashier1");
        _context.CashierShifts.Add(new CashierShift
        {
            UserId = user.Id,
            StartingCash = 50m,
            StartTime = DateTime.UtcNow,
            Status = ShiftStatus.Open
        });
        await _context.SaveChangesAsync();

        var result = await _userService.DeleteAsync(user.Id);

        Assert.False(result.IsSuccess);
        Assert.Contains("audit records", result.Error);

        var trackedUser = await _userService.GetByIdAsync(user.Id);
        Assert.NotNull(trackedUser);
        Assert.False(trackedUser.IsActive); // Account deactivated to protect audit history
    }

    [Fact]
    public async Task DeleteAsync_UserWithoutShifts_RemovesFromDatabase()
    {
        var newUser = new User
        {
            Username = "tempuser",
            FullName = "Temporary Cashier",
            Role = UserRole.Cashier,
            WorkingShift = "Morning"
        };
        await _userService.AddOrUpdateAsync(newUser, "temp123");

        var result = await _userService.DeleteAsync(newUser.Id);

        Assert.True(result.IsSuccess);
        var notFound = await _userService.GetByIdAsync(newUser.Id);
        Assert.Null(notFound);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
