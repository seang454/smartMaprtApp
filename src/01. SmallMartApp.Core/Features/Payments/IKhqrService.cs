namespace SmallMartApp.Core.Features.Payments;

public interface IKhqrService
{
    /// <summary>
    /// Generates an official EMVCo-compliant KHQR payload string for the NBC Bakong network.
    /// </summary>
    string GenerateKhqrString(
        string bakongAccountId,
        string merchantName,
        string merchantCity,
        decimal amount,
        string currency = "USD",
        string? billNumber = null,
        string? mobileNumber = null,
        string? merchantRoutingId = null,
        string? acquiringBank = null);

    /// <summary>
    /// Generates a high-resolution PNG image (byte array) of the QR code.
    /// </summary>
    byte[] GenerateQrCodePng(string qrData, int pixelsPerModule = 10);
}
