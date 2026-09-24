namespace SmallMartApp.Core.Hardware;

public interface IBarcodeScanner
{
    event EventHandler<string>? BarcodeScanned;
    bool IsConnected { get; }
    Task ConnectAsync();
    Task DisconnectAsync();
}
