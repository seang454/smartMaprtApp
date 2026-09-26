using SmallMartApp.Infrastructure.Services;
using Xunit;

namespace SmallMartApp.Tests;

public class KhqrServiceTests
{
    private readonly KhqrService _khqrService = new();

    [Fact]
    public void GenerateKhqrString_ShouldProduceValidAcledaEmvCoStructure()
    {
        // Arrange
        string bakongId = "016901360@aclb";
        string merchantName = "SIM PENGSEANG";
        string city = "Phnom Penh";
        decimal amount = 26.09m;

        // Act
        string khqr = _khqrService.GenerateKhqrString(
            bakongId, merchantName, city, amount, "USD", "REC-001",
            mobileNumber: "016901360", merchantRoutingId: "85521566549", acquiringBank: "ACLEDA");

        // Assert
        Assert.NotNull(khqr);
        Assert.StartsWith("000201", khqr); // Tag 00 length 02 value 01
        Assert.Contains("010212", khqr); // Tag 01 length 02 value 12 (Dynamic QR)
        Assert.Contains("khqr@aclb", khqr); // Tag 29 subtag 00
        Assert.Contains("85521566549", khqr); // Tag 29 subtag 01
        Assert.Contains("ACLEDA", khqr); // Tag 29 subtag 02
        Assert.Contains("2CCY", khqr); // Tag 39 dual currency
        Assert.Contains("52045999", khqr); // Tag 52 length 04 value 5999 (General Merchandise/Retail)
        Assert.Contains("5303840", khqr); // Tag 53 length 03 value 840 (USD)
        Assert.Contains("540526.09", khqr); // Tag 54 length 05 value 26.09
        Assert.Contains("5802KH", khqr); // Tag 58 length 02 value KH
        Assert.Contains("SIM PENGSEANG", khqr);
        Assert.Contains("Phnom Penh", khqr);
        Assert.Contains("016901360", khqr); // Tag 62 phone reference
        Assert.Contains("6304", khqr); // Tag 63 CRC
        Assert.Equal(4, khqr[^4..].Length); // 4 hex digits for CRC
    }

    [Fact]
    public void GenerateKhqrString_MatchesNativeAcledaStaticStringExactly()
    {
        // Exactly matches the screenshot taken from the user's ACLEDA mobile app
        string khqr = _khqrService.GenerateKhqrString(
            "016901360@aclb", "SIM PENGSEANG", "Phnom Penh", 0m, "KHR",
            mobileNumber: "016901360", merchantRoutingId: "85521566549", acquiringBank: "ACLEDA");

        string expected = "00020101021129380009khqr@aclb0111855215665490206ACLEDA391300042CCY01014520459995802KH53031165913SIM PENGSEANG6010Phnom Penh6213020901690136063040321";
        Assert.Equal(expected, khqr);
    }

    [Fact]
    public void GenerateKhqrString_ShouldProduceGeneralBakongSolo()
    {
        // Act
        string khqr = _khqrService.GenerateKhqrString("sothea@aba", "Sothea Shop", "Phnom Penh", 10.00m, "USD");

        // Assert
        Assert.Contains("0010sothea@aba", khqr);
        Assert.Contains("Sothea Shop", khqr);
        Assert.EndsWith(KhqrService.CalculateCrc16Ccitt(khqr[..^4]), khqr);
    }

    [Fact]
    public void GenerateQrCodePng_ShouldReturnNonEmptyBytes()
    {
        // Arrange
        string payload = _khqrService.GenerateKhqrString("016901360@aclb", "Smart Mart", "Phnom Penh", 15.50m);

        // Act
        byte[] pngBytes = _khqrService.GenerateQrCodePng(payload, pixelsPerModule: 5);

        // Assert
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 100);
        // PNG magic header: 0x89, 'P', 'N', 'G'
        Assert.Equal(0x89, pngBytes[0]);
        Assert.Equal((byte)'P', pngBytes[1]);
        Assert.Equal((byte)'N', pngBytes[2]);
        Assert.Equal((byte)'G', pngBytes[3]);
    }
}
