using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Sales;

public class Sale : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal CashReceived { get; set; }
    public decimal ChangeGiven { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
