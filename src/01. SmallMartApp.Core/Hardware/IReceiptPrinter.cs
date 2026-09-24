namespace SmallMartApp.Core.Hardware;

public interface IReceiptPrinter
{
    Task<bool> PrintReceiptAsync(
        string storeName,
        string receiptNo,
        IEnumerable<(string ItemName, int Qty, decimal Price, decimal Subtotal)> items,
        decimal totalAmount,
        decimal cashGiven,
        decimal change);
}
