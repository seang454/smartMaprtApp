using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmallMartApp.UI.Views;

public class ReceiptItemDisplay
{
    public string ItemName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }

    public string PriceDisplay => $"${Price:F2}";
    public string SubtotalDisplay => $"${Subtotal:F2}";
}

public partial class ReceiptPreviewWindow : Window
{
    private readonly string _storeName;
    private readonly string _receiptNo;
    private readonly List<ReceiptItemDisplay> _items;
    private readonly decimal _totalAmount;
    private readonly decimal _cashGiven;
    private readonly decimal _change;
    private readonly string _cashierName;
    private readonly string _customerName;
    private readonly DateTime _date;

    public ReceiptPreviewWindow(
        string storeName,
        string receiptNo,
        IEnumerable<(string ItemName, int Qty, decimal Price, decimal Subtotal)> items,
        decimal totalAmount,
        decimal cashGiven,
        decimal change,
        string? cashierName = null,
        string? customerName = null,
        DateTime? date = null)
    {
        InitializeComponent();

        _storeName = string.IsNullOrWhiteSpace(storeName) ? "SMART MART SUPERMARKET" : storeName;
        _receiptNo = receiptNo;
        _totalAmount = totalAmount;
        _cashGiven = cashGiven;
        _change = change;
        _cashierName = string.IsNullOrWhiteSpace(cashierName) ? "Staff Cashier" : cashierName;
        _customerName = string.IsNullOrWhiteSpace(customerName) ? "Walk-in Guest" : customerName;
        _date = date ?? DateTime.Now;

        _items = items.Select(i => new ReceiptItemDisplay
        {
            ItemName = i.ItemName,
            Qty = i.Qty,
            Price = i.Price,
            Subtotal = i.Subtotal
        }).ToList();

        PopulateData();
    }

    private void PopulateData()
    {
        TxtStoreName.Text = _storeName.ToUpperInvariant();
        TxtReceiptNo.Text = _receiptNo;
        TxtBarcodeLabel.Text = $"* {_receiptNo} *";
        TxtDate.Text = _date.ToString("yyyy-MM-dd HH:mm:ss");
        TxtCashier.Text = _cashierName;
        TxtCustomer.Text = _customerName;

        TxtSubtotal.Text = $"${_totalAmount:F2}";
        TxtTotalAmount.Text = $"${_totalAmount:F2}";
        TxtCashGiven.Text = $"${_cashGiven:F2}";
        TxtChange.Text = $"${_change:F2}";

        ItemsList.ItemsSource = _items;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(ReceiptPaperBorder, $"Receipt_{_receiptNo}");
                ShowStatus("Receipt printed / saved to PDF successfully! 🖨️");
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Print error: {ex.Message}");
        }
    }

    private void SaveTxt_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string receiptText = GenerateReceiptText();
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
            Directory.CreateDirectory(folder);

            string safeFileName = $"Receipt_{_receiptNo.Replace("/", "-").Replace("\\", "-")}.txt";
            string fullPath = Path.Combine(folder, safeFileName);
            File.WriteAllText(fullPath, receiptText, Encoding.UTF8);

            ShowStatus($"Saved to Receipts\\{safeFileName}!");

            // Open in default text viewer (Notepad)
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowStatus($"Error saving file: {ex.Message}");
        }
    }

    private void CopyText_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string receiptText = GenerateReceiptText();
            Clipboard.SetText(receiptText);
            ShowStatus("Receipt text copied to clipboard! 📋");
        }
        catch (Exception ex)
        {
            ShowStatus($"Copy error: {ex.Message}");
        }
    }

    private void ShowStatus(string message)
    {
        TxtStatus.Text = message;
        TxtStatus.Visibility = Visibility.Visible;
    }

    public string GenerateReceiptText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine($"        {_storeName.ToUpperInvariant()}");
        sb.AppendLine("      Express Retail & Grocery POS");
        sb.AppendLine("    Russian Blvd, Toul Kork, Phnom Penh");
        sb.AppendLine("  Tel: +855 23 888 999 | Tax: K001-9021882");
        sb.AppendLine("========================================");
        sb.AppendLine($"Date/Time : {_date:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Receipt # : {_receiptNo}");
        sb.AppendLine($"Cashier   : {_cashierName}");
        sb.AppendLine($"Customer  : {_customerName}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("ITEM                 QTY   PRICE   TOTAL");
        sb.AppendLine("----------------------------------------");

        foreach (var item in _items)
        {
            string name = item.ItemName.Length > 18 ? item.ItemName[..18] : item.ItemName.PadRight(18);
            sb.AppendLine($"{name} {item.Qty,3}  ${item.Price,6:F2} ${item.Subtotal,6:F2}");
        }

        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"SUBTOTAL:                       ${_totalAmount,7:F2}");
        sb.AppendLine($"VAT (0% Included):              $   0.00");
        sb.AppendLine($"GRAND TOTAL:                    ${_totalAmount,7:F2}");
        sb.AppendLine($"TENDERED / CASH:                ${_cashGiven,7:F2}");
        sb.AppendLine($"CHANGE DUE:                     ${_change,7:F2}");
        sb.AppendLine("========================================");
        sb.AppendLine($"           * {_receiptNo} *");
        sb.AppendLine("       THANK YOU FOR SHOPPING!");
        sb.AppendLine("   Please retain receipt for exchange");
        sb.AppendLine("      Powered by Smart Mart POS");
        sb.AppendLine("========================================");

        return sb.ToString();
    }
}
