namespace AppUser.Pages;

/// <summary>
/// Placeholder page for the center "Scan QR" tab in the bottom navigation.
/// When this tab is tapped, it immediately navigates to the full QrScannerPage
/// and then returns to the previous tab so the FAB tab never persists as "selected".
/// </summary>
public partial class ScanQrPlaceholderPage : ContentPage
{
    private bool _isNavigating = false;

    public ScanQrPlaceholderPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            // Navigate to QR scanner as a modal-style route
            await Shell.Current.GoToAsync("qrScanner");
        }
        finally
        {
            _isNavigating = false;
        }
    }
}
