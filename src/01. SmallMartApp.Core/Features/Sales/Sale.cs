using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Customers;
using SmallMartApp.Core.Features.Shifts;

namespace SmallMartApp.Core.Features.Sales;

public class Sale : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public int? ShiftId { get; set; }
    public CashierShift? Shift { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal CashReceived { get; set; }
    public decimal ChangeGiven { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public List<SaleItem> Items { get; set; } = new();
}
