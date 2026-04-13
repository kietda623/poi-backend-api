using AppUser.ViewModels;

namespace AppUser.Pages;

public partial class TinderPage : ContentPage
{
    private readonly TinderViewModel _viewModel;
    private bool _isInitializing;

    public TinderPage(TinderViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"TinderPage.OnAppearing error: {ex}");
        }
        finally
        {
            _isInitializing = false;
        }
    }
}
