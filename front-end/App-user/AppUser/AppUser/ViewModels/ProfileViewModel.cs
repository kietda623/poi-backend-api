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
        private string currentLanguageDisplay = "🇬🇧 English";

        [ObservableProperty]
        private string pageTitle = "My Profile";

        [ObservableProperty]
        private string editProfileText = "Edit profile";

        [ObservableProperty]
        private string foodieBadgeText = "FOOD EXPLORER";

        [ObservableProperty]
        private string totalExploredLabel = "Places explored";

        [ObservableProperty]
        private string currentLanguageLabel = "Current language";

        [ObservableProperty]
        private string settingsTitle = "Settings";

        [ObservableProperty]
        private string narrationLanguageTitle = "Narration language";

        [ObservableProperty]
        private string audioPackageTitle = "Goi nghe thuyet minh";

        [ObservableProperty]
        private string historyTitle = "Listening history";

        [ObservableProperty]
        private string emptyHistoryTitle = "No journeys yet";

        [ObservableProperty]
        private string emptyHistoryMessage = "Start exploring food spots around you now.";

        [ObservableProperty]
        private string logoutButtonText = "Log out";

        [ObservableProperty]
        private string logoutTitle = "Log out";

        [ObservableProperty]
        private string logoutMessage = "Are you sure you want to log out?";

        [ObservableProperty]
        private string logoutConfirmText = "Log out";

        [ObservableProperty]
        private string cancelText = "Cancel";

        // Guest mode: hiện nút đăng nhập thay vì thông tin profile
        [ObservableProperty]
        private bool isGuest = false;

        [ObservableProperty]
        private string loginButtonText = "Sign In / Sign Up";

        [ObservableProperty]
        private string guestMessage = "You are using the app as guest. You can use chatbot and packages now, or sign in to sync data across devices.";

        public ProfileViewModel(AuthService auth, AudioService audio)
        {
            _authService = auth;
            _audioService = audio;
            _audioService.LanguageChanged += OnLanguageChanged;
            UpdateLanguageDisplay();
            UpdateLocalizedTexts();
        }

        public async Task InitializeAsync()
        {
            await _authService.EnsureSessionLoadedAsync();
            if (!_authService.IsLoggedIn)
            {
                await _authService.InitGuestSessionAsync();
            }

            IsGuest = _authService.IsGuest;

            if (_authService.IsLoggedIn)
            {
                var user = _authService.GetCurrentUser();
                if (user != null)
                {
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
            }
            else
            {
                CurrentUser = null;
            }

            LoadHistory();
            UpdateLanguageDisplay();
            UpdateLocalizedTexts();
        }

        private void LoadHistory()
        {
            var history = _audioService.GetRecentHistory();
            ListenHistory.Clear();
            foreach (var item in history)
            {
                ListenHistory.Add(item);
            }
            TotalListened = history.Count;
        }

        private void UpdateLanguageDisplay()
        {
            CurrentLanguageDisplay = _audioService.CurrentLanguage switch
            {
                "vi" => "🇻🇳 Vietnamese",
                "en" => "🇬🇧 English",
                "zh" => "🇨🇳 中文",
                _ => "🇬🇧 English"
            };
        }

        private void UpdateLocalizedTexts()
        {
            switch (_audioService.CurrentLanguage)
            {
                case "en":
                    PageTitle = "My Profile";
                    EditProfileText = "Edit profile";
                    FoodieBadgeText = "FOOD EXPLORER";
                    TotalExploredLabel = "Places explored";
                    CurrentLanguageLabel = "Current language";
                    SettingsTitle = "Settings";
                    NarrationLanguageTitle = "Narration language";
                    AudioPackageTitle = "Audio packages";
                    HistoryTitle = "Listening history";
                    EmptyHistoryTitle = "No journeys yet";
                    EmptyHistoryMessage = "Start exploring food spots around you now!";
                    LogoutButtonText = "Log out";
                    LogoutTitle = "Log out";
                    LogoutMessage = "Are you sure you want to log out?";
                    LogoutConfirmText = "Log out";
                    CancelText = "Cancel";
                    break;
                case "zh":
                    PageTitle = "👤 我的资料";
                    EditProfileText = "✏️ 编辑资料";
                    FoodieBadgeText = "🧑‍🍳 美食探索者";
                    TotalExploredLabel = "探索地点";
                    CurrentLanguageLabel = "当前语言";
                    SettingsTitle = "⚙️ 设置";
                    NarrationLanguageTitle = "讲解语言";
                    AudioPackageTitle = "语音套餐";
                    HistoryTitle = "📖 探索历史";
                    EmptyHistoryTitle = "还没有探索记录";
                    EmptyHistoryMessage = "现在就开始探索你身边的美食地点吧！";
                    LogoutButtonText = "🚪  退出应用";
                    LogoutTitle = "退出登录";
                    LogoutMessage = "你确定要退出登录吗？";
                    LogoutConfirmText = "退出";
                    CancelText = "取消";
                    break;
                default:
                    PageTitle = "My Profile";
                    EditProfileText = "Edit profile";
                    FoodieBadgeText = "FOOD EXPLORER";
                    TotalExploredLabel = "Places explored";
                    CurrentLanguageLabel = "Current language";
                    SettingsTitle = "Settings";
                    NarrationLanguageTitle = "Narration language";
                    AudioPackageTitle = "Goi nghe thuyet minh";
                    HistoryTitle = "Listening history";
                    EmptyHistoryTitle = "No journeys yet";
                    EmptyHistoryMessage = "Start exploring food spots around you now.";
                    LogoutButtonText = "Log out";
                    LogoutTitle = "Log out";
                    LogoutMessage = "Are you sure you want to log out?";
                    LogoutConfirmText = "Log out";
                    CancelText = "Cancel";
                    break;
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                LogoutTitle,
                LogoutMessage,
                LogoutConfirmText,
                CancelText);

            if (!confirm) return;

            await _authService.LogoutAsync();
            // Quay về Home với Guest mode (không redirect về Login nữa)
            IsGuest = true;
            CurrentUser = null;
            await Shell.Current.GoToAsync("//home");
        }

        // Navigate đến trang Login (cho Guest muốn đăng nhập)
        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("login");
        }

        [RelayCommand]
        private async Task GoToEditProfileAsync()
        {
            await Shell.Current.GoToAsync("editProfile");
        }

        [RelayCommand]
        private async Task ChangeLanguageAsync()
        {
            // Show ActionSheet (Dropdown-like menu from bottom)
            string[] languages = { "🇻🇳 Vietnamese", "🇬🇧 English", "🇨🇳 中文" };
            string title = _audioService.CurrentLanguage switch
            {
                "en" => "Select Language",
                "zh" => "选择语言",
                _ => "Select Language"
            };
            string cancel = _audioService.CurrentLanguage switch
            {
                "en" => "Cancel",
                "zh" => "取消",
                _ => "Cancel"
            };

            var action = await Shell.Current.DisplayActionSheet(title, cancel, null, languages);

            if (action == "🇻🇳 Vietnamese")
            {
                _audioService.SetLanguage("vi");
            }
            else if (action == "🇬🇧 English")
            {
                _audioService.SetLanguage("en");
            }
            else if (action == "🇨🇳 中文")
            {
                _audioService.SetLanguage("zh");
            }

            UpdateLanguageDisplay();
            UpdateLocalizedTexts();
        }

        [RelayCommand]
        private async Task GoToSubscriptionPackagesAsync()
        {
            await Shell.Current.GoToAsync("subscriptionPackages");
        }

        [RelayCommand]
        private async Task NavigateToPOIAsync(POIDto poi)
        {
            await Shell.Current.GoToAsync("poiDetail",
                new Dictionary<string, object> { ["POI"] = poi });
        }

        private void OnLanguageChanged(object? sender, string language)
        {
            UpdateLanguageDisplay();
            UpdateLocalizedTexts();
        }
    }
}
