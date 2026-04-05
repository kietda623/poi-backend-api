using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly AudioService _audioService;

        [ObservableProperty]
        private UserDto? currentUser;

        [ObservableProperty]
        private ObservableCollection<(POIDto POI, DateTime ListenedAt)> listenHistory = new();

        [ObservableProperty]
        private int totalListened = 0;

        [ObservableProperty]
        private string currentLanguageDisplay = "🇻🇳 Tiếng Việt";

        public ProfileViewModel(AuthService auth, AudioService audio)
        {
            _authService = auth;
            _audioService = audio;
        }

        public void Initialize()
        {
            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                // Re-assign to force PropertyChanged
                CurrentUser = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };
            }
            LoadHistory();
            UpdateLanguageDisplay();
        }

        private void LoadHistory()
        {
            var history = _audioService.GetRecentHistory();
            ListenHistory.Clear();
            foreach (var item in history)
                ListenHistory.Add(item);
            TotalListened = history.Count;
        }

        private void UpdateLanguageDisplay()
        {
            CurrentLanguageDisplay = _audioService.CurrentLanguage switch
            {
                "vi" => "🇻🇳 Tiếng Việt",
                "en" => "🇬🇧 English",
                "zh" => "🇨🇳 中文",
                _ => "🇻🇳 Tiếng Việt"
            };
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Đăng xuất",
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Đăng xuất",
                "Hủy");

            if (!confirm) return;

            await _authService.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }

        [RelayCommand]
        private async Task GoToEditProfileAsync()
        {
            await Shell.Current.GoToAsync("editProfile");
        }

        [RelayCommand]
        private void ToggleLanguage()
        {
            var newLang = _audioService.CurrentLanguage switch
            {
                "vi" => "en",
                "en" => "zh",
                _ => "vi"
            };
            _audioService.SetLanguage(newLang);
            UpdateLanguageDisplay();
        }

        [RelayCommand]
        private async Task NavigateToPOIAsync(POIDto poi)
        {
            await Shell.Current.GoToAsync("poiDetail",
                new Dictionary<string, object> { ["POI"] = poi });
        }
    }
}
