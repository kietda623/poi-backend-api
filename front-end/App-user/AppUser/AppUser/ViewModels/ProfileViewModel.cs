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

        [ObservableProperty]
        private string pageTitle = "👤 Hồ sơ của tôi";

        [ObservableProperty]
        private string editProfileText = "✏️ Chỉnh sửa hồ sơ";

        [ObservableProperty]
        private string foodieBadgeText = "🧑‍🍳 THÁM TỬ ẨM THỰC";

        [ObservableProperty]
        private string totalExploredLabel = "Điểm khám phá";

        [ObservableProperty]
        private string currentLanguageLabel = "Ngôn ngữ hiện tại";

        [ObservableProperty]
        private string settingsTitle = "⚙️ Cài đặt";

        [ObservableProperty]
        private string narrationLanguageTitle = "Ngôn ngữ thuyết minh";

        [ObservableProperty]
        private string audioPackageTitle = "Goi nghe thuyet minh";

        [ObservableProperty]
        private string historyTitle = "📖 Lịch sử khám phá";

        [ObservableProperty]
        private string emptyHistoryTitle = "Chưa có hành trình nào";

        [ObservableProperty]
        private string emptyHistoryMessage = "Hãy bắt đầu khám phá các điểm ẩm thực xung quanh bạn ngay!";

        [ObservableProperty]
        private string logoutButtonText = "🚪  Đăng xuất khỏi ứng dụng";

        [ObservableProperty]
        private string logoutTitle = "Đăng xuất";

        [ObservableProperty]
        private string logoutMessage = "Bạn có chắc chắn muốn đăng xuất không?";

        [ObservableProperty]
        private string logoutConfirmText = "Đăng xuất";

        [ObservableProperty]
        private string cancelText = "Hủy";

        public ProfileViewModel(AuthService auth, AudioService audio)
        {
            _authService = auth;
            _audioService = audio;
            _audioService.LanguageChanged += OnLanguageChanged;
            UpdateLanguageDisplay();
            UpdateLocalizedTexts();
        }

        public void Initialize()
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
                "vi" => "🇻🇳 Tiếng Việt",
                "en" => "🇬🇧 English",
                "zh" => "🇨🇳 中文",
                _ => "🇻🇳 Tiếng Việt"
            };
        }

        private void UpdateLocalizedTexts()
        {
            switch (_audioService.CurrentLanguage)
            {
                case "en":
                    PageTitle = "👤 My Profile";
                    EditProfileText = "✏️ Edit profile";
                    FoodieBadgeText = "🧑‍🍳 FOOD EXPLORER";
                    TotalExploredLabel = "Places explored";
                    CurrentLanguageLabel = "Current language";
                    SettingsTitle = "⚙️ Settings";
                    NarrationLanguageTitle = "Narration language";
                    AudioPackageTitle = "Audio packages";
                    HistoryTitle = "📖 Listening history";
                    EmptyHistoryTitle = "No journeys yet";
                    EmptyHistoryMessage = "Start exploring food spots around you now!";
                    LogoutButtonText = "🚪  Log out of the app";
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
                    PageTitle = "👤 Hồ sơ của tôi";
                    EditProfileText = "✏️ Chỉnh sửa hồ sơ";
                    FoodieBadgeText = "🧑‍🍳 THÁM TỬ ẨM THỰC";
                    TotalExploredLabel = "Điểm khám phá";
                    CurrentLanguageLabel = "Ngôn ngữ hiện tại";
                    SettingsTitle = "⚙️ Cài đặt";
                    NarrationLanguageTitle = "Ngôn ngữ thuyết minh";
                    AudioPackageTitle = "Goi nghe thuyet minh";
                    HistoryTitle = "📖 Lịch sử khám phá";
                    EmptyHistoryTitle = "Chưa có hành trình nào";
                    EmptyHistoryMessage = "Hãy bắt đầu khám phá các điểm ẩm thực xung quanh bạn ngay!";
                    LogoutButtonText = "🚪  Đăng xuất khỏi ứng dụng";
                    LogoutTitle = "Đăng xuất";
                    LogoutMessage = "Bạn có chắc chắn muốn đăng xuất không?";
                    LogoutConfirmText = "Đăng xuất";
                    CancelText = "Hủy";
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
