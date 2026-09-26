using System.IO;
using System.Text;
using System.Windows;
using SmallMartApp.Core.Hardware;
using SmallMartApp.UI.Views;

namespace SmallMartApp.UI.Services;

public class WpfReceiptPrinter : IReceiptPrinter
{
    public Task<bool> PrintReceiptAsync(
        string storeName,
        string receiptNo,
        IEnumerable<(string ItemName, int Qty, decimal Price, decimal Subtotal)> items,
        decimal totalAmount,
        decimal cashGiven,
        decimal change)
    {
        var itemsList = items?.ToList() ?? new List<(string, int, decimal, decimal)>();

        // 1. Silent Disk Audit: Save copy to Receipts/ directory
        try
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
            Directory.CreateDirectory(folder);
            string safeFileName = $"Receipt_{receiptNo.Replace("/", "-").Replace("\\", "-")}.txt";
            string fullPath = Path.Combine(folder, safeFileName);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"        {storeName.ToUpperInvariant()}");
            sb.AppendLine("========================================");
            sb.AppendLine($"Date/Time : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Receipt # : {receiptNo}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("ITEM                 QTY   PRICE   TOTAL");
            sb.AppendLine("----------------------------------------");
            foreach (var item in itemsList)
            {
                string name = item.ItemName.Length > 18 ? item.ItemName[..18] : item.ItemName.PadRight(18);
                sb.AppendLine($"{name} {item.Qty,3}  ${item.Price,6:F2} ${item.Subtotal,6:F2}");
            }
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"TOTAL:                          ${totalAmount,7:F2}");
            sb.AppendLine($"CASH:                           ${cashGiven,7:F2}");
            sb.AppendLine($"CHANGE:                         ${change,7:F2}");
            sb.AppendLine("========================================");
            File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
        }
        catch
        {
            // Ignore file logging errors in background
        }

        // 2. Interactive UI: Launch Virtual Thermal Receipt Simulator Dialog on UI thread
        try
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = new ReceiptPreviewWindow(
                        storeName,
                        receiptNo,
                        itemsList,
                        totalAmount,
                        cashGiven,
                        change);

                    if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                    {
                        window.Owner = Application.Current.MainWindow;
                    }

                    window.ShowDialog();
                });
            }
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
