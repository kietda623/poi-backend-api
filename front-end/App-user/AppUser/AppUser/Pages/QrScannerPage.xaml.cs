using AppUser.ViewModels;
using ZXing.Net.Maui;

namespace AppUser.Pages;

/// <summary>
/// Code-behind cho trang quét QR Code.
/// Xử lý sự kiện từ camera barcode reader và forward đến ViewModel.
/// </summary>
public partial class QrScannerPage : ContentPage
{
    private readonly QrScannerViewModel _viewModel;

    public QrScannerPage(QrScannerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Cấu hình barcode reader: chỉ quét QR Code
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false // Chỉ đọc 1 mã mỗi lần
        };
    }

    /// <summary>
    /// Sự kiện khi camera phát hiện barcode/QR code.
    /// Forward kết quả đến ViewModel để xử lý.
    /// </summary>
    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        // Chạy trên MainThread vì sự kiện camera có thể chạy trên background thread
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var firstResult = e.Results?.FirstOrDefault();
            if (firstResult != null && !string.IsNullOrWhiteSpace(firstResult.Value))
            {
                await _viewModel.ProcessQrResultCommand.ExecuteAsync(firstResult.Value);
            }
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reset scanner khi trang xuất hiện lại
        _viewModel.ResetScannerCommand.Execute(null);
    }
}
