using AppUser.ViewModels;

namespace AppUser.Pages;

public partial class SubscriptionPage : ContentPage
{
    private readonly SubscriptionViewModel _viewModel;
    private bool _isInitializing;

    public SubscriptionPage(SubscriptionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_isInitializing) return;

        try
        {
            _isInitializing = true;
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
}
