using Microsoft.EntityFrameworkCore;
using SmallMartApp.Infrastructure.Persistence;
using Xunit;

namespace SmallMartApp.Tests;

public class SqlServerConnectionTests
{
    [Fact]
    public async Task CanConnectToSqlServer_AndVerifyTablesCreated()
    {
        var options = new DbContextOptionsBuilder<SmallMartDbContext>()
            .UseSqlServer("Server=localhost\\SQLEXPRESS;Database=SmallMartDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        using var context = new SmallMartDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Verify that tables and seed data exist in SQL Server
        var productsCount = await context.Products.CountAsync();
        var categoriesCount = await context.Categories.CountAsync();
        var usersCount = await context.Users.CountAsync();

        Assert.True(productsCount >= 4);
        Assert.True(categoriesCount >= 3);
        Assert.True(usersCount >= 2);
    }
}
