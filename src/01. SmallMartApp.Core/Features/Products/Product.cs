using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Products;

public class Product : BaseEntity
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}
