using AppUser.ViewModels;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using AppUser.Models;
using AppUser.Pages.Popups;

namespace AppUser.Pages;

public partial class TourPlanPage : ContentPage
{
    private readonly TourPlanViewModel _viewModel;
    private bool _isInitializing;

    public TourPlanPage(TourPlanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Register to receive the message to show the popup
        WeakReferenceMessenger.Default.Register<TourPlanGeneratedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.ShowPopup(new TourPlanResultPopup(m.Result));
            });
        });
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
            System.Diagnostics.Debug.WriteLine($"TourPlanPage.OnAppearing error: {ex}");
        }
        finally
        {
            _isInitializing = false;
        }
    }
}
