using System.Diagnostics;
using System.Text;
using SmallMartApp.Core.Hardware;

namespace SmallMartApp.Infrastructure.Hardware;

public class FakeReceiptPrinter : IReceiptPrinter
{
    public Task<bool> PrintReceiptAsync(
        string storeName,
        string receiptNo,
        IEnumerable<(string ItemName, int Qty, decimal Price, decimal Subtotal)> items,
        decimal totalAmount,
        decimal cashGiven,
        decimal change)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================");
        sb.AppendLine($"         {storeName.ToUpper()}");
        sb.AppendLine("================================");
        sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Receipt #: {receiptNo}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine("Item           Qty   Price Total");
        sb.AppendLine("--------------------------------");

        foreach (var item in items)
        {
            var name = item.ItemName.Length > 12 ? item.ItemName[..12] : item.ItemName.PadRight(12);
            sb.AppendLine($"{name}  {item.Qty,3}  ${item.Price,5:F2} ${item.Subtotal,5:F2}");
        }

        sb.AppendLine("--------------------------------");
        sb.AppendLine($"TOTAL:                  ${totalAmount:F2}");
        sb.AppendLine($"CASH:                   ${cashGiven:F2}");
        sb.AppendLine($"CHANGE:                 ${change:F2}");
        sb.AppendLine("================================");
        sb.AppendLine("     THANK YOU FOR SHOPPING!    ");
        sb.AppendLine("================================");

        Debug.WriteLine(sb.ToString());
        Console.WriteLine(sb.ToString());

        return Task.FromResult(true);
    }
}
