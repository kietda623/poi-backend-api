using AppUser.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppUser.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool isBusy;

    public WelcomeViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private Task GoToLoginAsync() => Shell.Current.GoToAsync("login");

    [RelayCommand]
    private Task GoToRegisterAsync() => Shell.Current.GoToAsync("register");

    [RelayCommand]
    private async Task ContinueAsGuestAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _authService.InitGuestSessionAsync();
            await Shell.Current.GoToAsync("//home");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
