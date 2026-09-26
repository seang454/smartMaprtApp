using System;
using System.Globalization;
using System.Text;
using QRCoder;
using SmallMartApp.Core.Features.Payments;

namespace SmallMartApp.Infrastructure.Services;

public class KhqrService : IKhqrService
{
    public string GenerateKhqrString(
        string bakongAccountId,
        string merchantName,
        string merchantCity,
        decimal amount,
        string currency = "USD",
        string? billNumber = null,
        string? mobileNumber = null,
        string? merchantRoutingId = null,
        string? acquiringBank = null)
    {
        var sb = new StringBuilder();

        // 00: Payload Format Indicator (01)
        sb.Append(FormatTag("00", "01"));

        // 01: Point of Initiation Method (12 = Dynamic QR with amount, 11 = Static QR)
        sb.Append(FormatTag("01", amount > 0 ? "12" : "11"));

        string trimmedAccount = bakongAccountId.Trim();
        bool isAcleda = trimmedAccount.Contains("aclb", StringComparison.OrdinalIgnoreCase) ||
                        trimmedAccount.Contains("016901360") ||
                        string.Equals(acquiringBank, "ACLEDA", StringComparison.OrdinalIgnoreCase);

        if (isAcleda)
        {
            // ACLEDA Native KHQR Structure (EMVCo Individual / Solo Merchant)
            // Tag 29: Merchant Account Information
            //   00: Bakong Participant ID (khqr@aclb)
            //   01: ACLEDA Merchant Routing ID (85521566549)
            //   02: Acquiring Bank Name (ACLEDA)
            string routingId = merchantRoutingId ?? "85521566549";
            string bankName = acquiringBank ?? "ACLEDA";
            string sub29 = FormatTag("00", "khqr@aclb") +
                           FormatTag("01", routingId) +
                           FormatTag("02", bankName);
            sb.Append(FormatTag("29", sub29));

            // Tag 39: Dual Currency Account Indicator (2CCY)
            string sub39 = FormatTag("00", "2CCY") + FormatTag("01", "4");
            sb.Append(FormatTag("39", sub39));
        }
        else
        {
            // Standard NBC Bakong Solo Account
            // Tag 29 Sub-tag 00: Bakong Account ID (e.g. user@aba)
            string subTag00 = FormatTag("00", trimmedAccount);
            sb.Append(FormatTag("29", subTag00));
        }

        // 52: Merchant Category Code (5999 = Miscellaneous/General Retail)
        sb.Append(FormatTag("52", "5999"));

        // 58: Country Code (KH)
        sb.Append(FormatTag("58", "KH"));

        // 53: Transaction Currency (840 = USD, 116 = KHR)
        string currencyCode = currency.Trim().ToUpperInvariant() == "KHR" ? "116" : "840";
        sb.Append(FormatTag("53", currencyCode));

        // 54: Transaction Amount (dynamic only)
        if (amount > 0)
        {
            string amountStr = currencyCode == "116"
                ? ((long)Math.Round(amount)).ToString(CultureInfo.InvariantCulture)
                : amount.ToString("F2", CultureInfo.InvariantCulture);
            sb.Append(FormatTag("54", amountStr));
        }

        // 59: Merchant Name (Up to 25 chars)
        string safeMerchantName = string.IsNullOrWhiteSpace(merchantName) ? "SIM PENGSEANG" : merchantName.Trim();
        if (safeMerchantName.Length > 25) safeMerchantName = safeMerchantName[..25];
        sb.Append(FormatTag("59", safeMerchantName));

        // 60: Merchant City (Up to 15 chars)
        string safeCity = string.IsNullOrWhiteSpace(merchantCity) ? "Phnom Penh" : merchantCity.Trim();
        if (safeCity.Length > 15) safeCity = safeCity[..15];
        sb.Append(FormatTag("60", safeCity));

        // 62: Additional Data Field Template
        var sb62 = new StringBuilder();
        if (isAcleda)
        {
            string phone = mobileNumber ?? (trimmedAccount.Contains('@') ? trimmedAccount.Split('@')[0] : trimmedAccount);
            if (string.IsNullOrWhiteSpace(phone)) phone = "016901360";
            sb62.Append(FormatTag("02", phone));
        }

        if (!string.IsNullOrWhiteSpace(billNumber))
        {
            sb62.Append(FormatTag("01", billNumber.Trim()));
        }

        if (sb62.Length > 0)
        {
            sb.Append(FormatTag("62", sb62.ToString()));
        }

        // 63: CRC16 Checksum
        // Tag "63" + length "04" appended before computing the CRC
        string payloadWithoutCrc = sb.ToString() + "6304";
        string crc = CalculateCrc16Ccitt(payloadWithoutCrc);

        return payloadWithoutCrc + crc;
    }

    public byte[] GenerateQrCodePng(string qrData, int pixelsPerModule = 10)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.M);
        var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule);
    }

    private static string FormatTag(string tag, string value)
    {
        return $"{tag}{value.Length:D2}{value}";
    }

    /// <summary>
    /// Calculates CRC16-CCITT (polynomial 0x1021, init 0xFFFF, no final XOR) as specified by EMVCo / NBC KHQR.
    /// </summary>
    public static string CalculateCrc16Ccitt(string data)
    {
        ushort crc = 0xFFFF;
        const ushort polynomial = 0x1021;
        byte[] bytes = Encoding.UTF8.GetBytes(data);

        foreach (byte b in bytes)
        {
            for (int i = 0; i < 8; i++)
            {
                bool bit = ((b >> (7 - i)) & 1) == 1;
                bool c15 = ((crc >> 15) & 1) == 1;
                crc <<= 1;
                if (c15 ^ bit) crc ^= polynomial;
            }
        }

        return crc.ToString("X4");
    }
}
