using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Suppliers;

public class PurchaseOrder : BaseEntity
{
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public List<PurchaseOrderItem> Items { get; set; } = new();
}

public class PurchaseOrderItem : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Subtotal => Quantity * UnitCost;
}
