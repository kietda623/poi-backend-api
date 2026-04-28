using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Services;

namespace AppUser.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private bool isPasswordVisible = false;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Please fill in all required fields.");
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowError("Password confirmation does not match.");
                return;
            }

            if (Password.Length < 6)
            {
                ShowError("Password must be at least 6 characters.");
                return;
            }

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var (success, message) = await _authService.RegisterAsync(Email.Trim(), Password, FullName.Trim());

                if (success)
                {
                    await Shell.Current.DisplayAlert("Success", "Your account has been created. Please sign in.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ShowError(message);
                }
            }
            catch (Exception ex)
            {
                ShowError("A connection error occurred. Please try again later.");
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoBackToLoginAsync()
        {
            await Shell.Current.GoToAsync("login");
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}
