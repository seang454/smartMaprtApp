using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Suppliers;

namespace SmallMartApp.Core.Features.Products;

public class Product : BaseEntity
{
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockAlertThreshold { get; set; } = 5;
    public bool IsActive { get; set; } = true;
}
