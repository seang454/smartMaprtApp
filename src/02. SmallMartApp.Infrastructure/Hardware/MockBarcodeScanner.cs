using SmallMartApp.Core.Hardware;

namespace SmallMartApp.Infrastructure.Hardware;

public class MockBarcodeScanner : IBarcodeScanner
{
    public event EventHandler<string>? BarcodeScanned;
    public bool IsConnected { get; private set; } = true;

    public Task ConnectAsync()
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public void SimulateScan(string barcode)
    {
        if (IsConnected && !string.IsNullOrWhiteSpace(barcode))
        {
            BarcodeScanned?.Invoke(this, barcode.Trim());
        }
    }
}
