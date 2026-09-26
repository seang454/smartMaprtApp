using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Core.Features.Customers;
using SmallMartApp.Core.Features.Products;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Features.Shifts;
using SmallMartApp.Core.Features.Suppliers;

namespace SmallMartApp.Infrastructure.Persistence;

public class SmallMartDbContext : DbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    public SmallMartDbContext(DbContextOptions<SmallMartDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250);
        });

        // 2. Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(250);
        });

        // 3. Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.Barcode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.SellPrice).HasPrecision(18, 2);

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 4. Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(120).IsRequired();
        });

        // 5. User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.WorkingShift).HasMaxLength(50).HasDefaultValue("Morning");
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>();
        });

        // 6. CashierShift
        modelBuilder.Entity<CashierShift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartingCash).HasPrecision(18, 2);
            entity.Property(e => e.ExpectedCash).HasPrecision(18, 2);
            entity.Property(e => e.ActualCash).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 7. Sale
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.CashReceived).HasPrecision(18, 2);
            entity.Property(e => e.ChangeGiven).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasConversion<string>();

            entity.HasOne(e => e.Shift)
                  .WithMany(s => s.Sales)
                  .HasForeignKey(e => e.ShiftId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey(e => e.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 8. SaleItem
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
        });

        // 9. PurchaseOrder & PurchaseOrderItem
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);

            entity.HasOne(e => e.Supplier)
                  .WithMany(s => s.PurchaseOrders)
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey(e => e.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
        });

        // Seed Initial Data
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Beverages", Description = "Soft drinks, water, and juices" },
            new Category { Id = 2, Name = "Snacks", Description = "Chips, candy, and nuts" },
            new Category { Id = 3, Name = "Groceries", Description = "Instant noodles and staples" }
        );

        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, CompanyName = "Phnom Penh Distribution Co.", ContactPerson = "Sokha", PhoneNumber = "012345678", Address = "Phnom Penh" }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", PasswordHash = "admin123", FullName = "Chea Sovann", WorkingShift = "FullTime", Role = UserRole.Admin, IsActive = true },
            new User { Id = 2, Username = "cashier1", PasswordHash = "123456", FullName = "Sreymao Tep", WorkingShift = "Morning", Role = UserRole.Cashier, IsActive = true }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, CategoryId = 1, SupplierId = 1, Barcode = "885012401", Name = "Coca Cola Can 330ml", CostPrice = 0.40m, SellPrice = 0.65m, StockQuantity = 50, LowStockAlertThreshold = 5 },
            new Product { Id = 2, CategoryId = 1, SupplierId = 1, Barcode = "885012402", Name = "Mineral Water 500ml", CostPrice = 0.15m, SellPrice = 0.35m, StockQuantity = 100, LowStockAlertThreshold = 10 },
            new Product { Id = 3, CategoryId = 2, SupplierId = 1, Barcode = "885012403", Name = "Potato Chips Original", CostPrice = 0.80m, SellPrice = 1.25m, StockQuantity = 30, LowStockAlertThreshold = 5 },
            new Product { Id = 4, CategoryId = 3, SupplierId = 1, Barcode = "885012404", Name = "Instant Noodles Chicken", CostPrice = 0.25m, SellPrice = 0.50m, StockQuantity = 80, LowStockAlertThreshold = 15 }
        );
    }
}
