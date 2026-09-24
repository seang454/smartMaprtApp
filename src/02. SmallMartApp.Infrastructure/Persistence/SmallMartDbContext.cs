using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;

namespace SmallMartApp.Infrastructure.Persistence;

public class SmallMartDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    public SmallMartDbContext(DbContextOptions<SmallMartDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.Barcode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.SellPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.CashReceived).HasPrecision(18, 2);
            entity.Property(e => e.ChangeGiven).HasPrecision(18, 2);
            entity.HasMany(e => e.Items).WithOne().HasForeignKey(e => e.SaleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
        });

        // Seed initial sample inventory for testing
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Barcode = "885012401", Name = "Coca Cola Can 330ml", CostPrice = 0.40m, SellPrice = 0.65m, StockQuantity = 50 },
            new Product { Id = 2, Barcode = "885012402", Name = "Mineral Water 500ml", CostPrice = 0.15m, SellPrice = 0.35m, StockQuantity = 100 },
            new Product { Id = 3, Barcode = "885012403", Name = "Potato Chips Original", CostPrice = 0.80m, SellPrice = 1.25m, StockQuantity = 30 },
            new Product { Id = 4, Barcode = "885012404", Name = "Instant Noodles Chicken", CostPrice = 0.25m, SellPrice = 0.50m, StockQuantity = 80 }
        );
    }
}
