using AppUser.ViewModels;

namespace AppUser.Pages;

public partial class SubscriptionPage : ContentPage
{
    private readonly SubscriptionViewModel _viewModel;
    private bool _isInitializing;
    private bool _isResumeHooked;
    private Window? _hookedWindow;

    public SubscriptionPage(SubscriptionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        HookAppResumed();

        if (_isInitializing) return;

        try
        {
            _isInitializing = true;
            _viewModel.UpdateDialogLayout(Width, Height);
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubscriptionPage.OnAppearing error: {ex}");
        }
        finally
        {
            _isInitializing = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnhookAppResumed();
        _viewModel.OnPageDisappearing();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        _viewModel.UpdateDialogLayout(width, height);
    }

    private void HookAppResumed()
    {
        if (_isResumeHooked || Window == null)
        {
            return;
        }

        Window.Resumed += OnWindowResumed;
        _hookedWindow = Window;
        _isResumeHooked = true;
    }

    private void UnhookAppResumed()
    {
        if (!_isResumeHooked || _hookedWindow == null)
        {
            return;
        }

        _hookedWindow.Resumed -= OnWindowResumed;
        _hookedWindow = null;
        _isResumeHooked = false;
    }

    private async void OnWindowResumed(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.HandleAppResumedAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubscriptionPage.OnWindowResumed error: {ex}");
        }
    }
}
