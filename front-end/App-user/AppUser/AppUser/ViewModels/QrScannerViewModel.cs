using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace AppUser.ViewModels;

/// <summary>
/// ViewModel xử lý logic quét QR Code.
/// Parse URL từ mã QR để trích xuất POI ID và navigate đến trang chi tiết.
/// </summary>
public partial class QrScannerViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isScanning = true;

    [ObservableProperty]
    private bool isProcessing = false;

    [ObservableProperty]
    private string statusMessage = "Point your camera at a QR code";

    // Flag tránh xử lý nhiều lần cùng lúc
    private bool _hasNavigated = false;

    /// <summary>
    /// Xử lý kết quả quét QR Code.
    /// Hỗ trợ nhiều format URL:
    /// - https://foodtour.vn/poi/{id}
    /// - foodtour://poi/{id}
    /// - https://foodtour.vn/shop/{id}
    /// - Chỉ số (plain number = POI ID)
    /// </summary>
    [RelayCommand]
    private async Task ProcessQrResultAsync(string? qrValue)
    {
        if (string.IsNullOrWhiteSpace(qrValue) || _hasNavigated || IsProcessing)
            return;

        IsProcessing = true;
        IsScanning = false;
        _hasNavigated = true;

        try
        {
            Debug.WriteLine($"[QrScanner] Scanned: {qrValue}");

            var poiId = ParsePoiIdFromQr(qrValue.Trim());

            if (poiId.HasValue)
            {
                StatusMessage = $"POI #{poiId} detected. Redirecting...";
                Debug.WriteLine($"[QrScanner] Navigating to POI ID: {poiId}");

                // Navigate đến trang chi tiết POI
                await Shell.Current.GoToAsync($"poiDetail?id={poiId}");
            }
            else
            {
                StatusMessage = "Invalid QR code. Please try again.";
                Debug.WriteLine($"[QrScanner] Could not parse POI ID from: {qrValue}");

                // Cho phép quét lại sau 2 giây
                await Task.Delay(2000);
                ResetScanner();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[QrScanner] QR processing error: {ex.Message}");
            StatusMessage = "An error occurred. Please try again.";
            await Task.Delay(2000);
            ResetScanner();
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>
    /// Reset scanner để quét lại.
    /// </summary>
    [RelayCommand]
    private void ResetScanner()
    {
        _hasNavigated = false;
        IsScanning = true;
        IsProcessing = false;
        StatusMessage = "Point your camera at a QR code";
    }

    /// <summary>
    /// Quay lại trang trước.
    /// </summary>
    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    /// <summary>
    /// Parse POI ID từ nhiều format QR Code khác nhau.
    /// </summary>
    private static int? ParsePoiIdFromQr(string qrValue)
    {
        // Trường hợp 1: Chỉ là số (plain POI ID)
        if (int.TryParse(qrValue, out var plainId))
        {
            return plainId;
        }

        // Trường hợp 2: URL format - https://foodtour.vn/poi/5
        if (Uri.TryCreate(qrValue, UriKind.Absolute, out var uri))
        {
            return ExtractIdFromUri(uri);
        }

        // Trường hợp 3: Custom scheme - foodtour://poi/5
        if (qrValue.StartsWith("foodtour://", StringComparison.OrdinalIgnoreCase))
        {
            var fakePath = qrValue.Replace("foodtour://", "https://foodtour.vn/");
            if (Uri.TryCreate(fakePath, UriKind.Absolute, out var fakeUri))
            {
                return ExtractIdFromUri(fakeUri);
            }
        }

        return null;
    }

    /// <summary>
    /// Trích xuất ID từ các segment trong URL path.
    /// Hỗ trợ: /poi/{id}, /shop/{id}, /p/{id}
    /// </summary>
    private static int? ExtractIdFromUri(Uri uri)
    {
        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i].ToLowerInvariant();

            // Nếu segment là poi/shop/p, lấy segment tiếp theo làm ID
            if ((segment == "poi" || segment == "shop" || segment == "p") &&
                i + 1 < segments.Length &&
                int.TryParse(segments[i + 1], out var id))
            {
                return id;
            }
        }

        // Fallback: thử segment cuối cùng
        if (segments.Length > 0 && int.TryParse(segments[^1], out var lastId))
        {
            return lastId;
        }

        return null;
    }
}
