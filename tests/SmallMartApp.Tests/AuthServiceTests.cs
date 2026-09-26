using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Infrastructure.Persistence;
using SmallMartApp.Infrastructure.Services;
using Xunit;

namespace SmallMartApp.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SmallMartDbContext _context;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SmallMartDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SmallMartDbContext(options);
        _context.Database.EnsureCreated();

        _authService = new AuthService(_context);
    }

    [Fact]
    public async Task LoginAsync_WithValidAdminCredentials_ReturnsSuccessAndAdminRole()
    {
        var result = await _authService.LoginAsync("admin", "admin123");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("admin", result.Value.Username);
        Assert.Equal(UserRole.Admin, result.Value.Role);
    }

    [Fact]
    public async Task LoginAsync_WithValidCashierCredentials_ReturnsSuccessAndCashierRole()
    {
        var result = await _authService.LoginAsync("cashier1", "123456");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("cashier1", result.Value.Username);
        Assert.Equal(UserRole.Cashier, result.Value.Role);
    }

    [Fact]
    public async Task LoginAsync_WithValidFullName_ReturnsSuccess()
    {
        var result = await _authService.LoginAsync("Sreymao Tep", "123456");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("cashier1", result.Value.Username);
        Assert.Equal("Sreymao Tep", result.Value.FullName);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailure()
    {
        var result = await _authService.LoginAsync("admin", "wrongpassword");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Invalid password.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
    {
        var result = await _authService.LoginAsync("doesnotexist", "password");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("User not found or inactive.", result.Error);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("admin", "")]
    [InlineData("", "admin123")]
    public async Task LoginAsync_WithEmptyCredentials_ReturnsFailure(string username, string password)
    {
        var result = await _authService.LoginAsync(username, password);

        Assert.False(result.IsSuccess);
        Assert.Equal("Username and password are required.", result.Error);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
