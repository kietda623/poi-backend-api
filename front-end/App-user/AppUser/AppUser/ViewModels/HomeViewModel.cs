using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly POIService _poiService;
        private readonly AuthService _authService;
        private readonly AudioService _audioService;
        private readonly SubscriptionService _subscriptionService;

        [ObservableProperty]
        private ObservableCollection<POIDto> featuredPOIs = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string greetingText = "Chào buổi tối!";

        [ObservableProperty]
        private string userEmail = string.Empty;

        [ObservableProperty]
        private string currentLanguage = "en";

        [ObservableProperty]
        private string pageTitle = "Khám phá ẩm thực";

        [ObservableProperty]
        private string searchPlaceholder = "Tìm điểm ẩm thực...";

        [ObservableProperty]
        private string featuredSectionTitle = "✨ Điểm ẩm thực nổi bật";

        [ObservableProperty]
        private string seeAllText = "Xem tất cả";

        [ObservableProperty]
        private string discoverMoreTitle = "🗺️ Khám phá thêm";

        [ObservableProperty]
        private string listenNowText = "Nghe ngay";

        [ObservableProperty]
        private string noAudioTitle = "Không có audio";

        [ObservableProperty]
        private string noAudioMessage = "Điểm ẩm thực này chưa có thuyết minh audio.";

        [ObservableProperty]
        private string okText = "OK";

        public HomeViewModel(POIService poi, AuthService auth, AudioService audio, SubscriptionService subscriptionService)
        {
            _poiService = poi;
            _authService = auth;
            _audioService = audio;
            _subscriptionService = subscriptionService;
            CurrentLanguage = _audioService.CurrentLanguage;
            _audioService.LanguageChanged += OnLanguageChanged;
            UpdateGreeting();
            UpdateLocalizedTexts();
        }

        public async Task InitializeAsync()
        {
            if (_authService.IsLoggedIn)
            {
                var (success, _) = await _authService.RefreshMeAsync();
                if (!success)
                {
                    await _authService.LogoutAsync();
                    await Shell.Current.GoToAsync("//login");
                    return;
                }
            }

            CurrentLanguage = _audioService.CurrentLanguage;
            UpdateGreeting();
            UpdateLocalizedTexts();
            UserEmail = _authService.CurrentUser?.Email ?? string.Empty;
            await LoadFeaturedPOIsAsync();
        }

        [RelayCommand]
        private async Task LoadFeaturedPOIsAsync()
        {
            IsLoading = true;
            try
            {
                var pois = await _poiService.GetFeaturedPOIsAsync(5, CurrentLanguage);
                FeaturedPOIs.Clear();
                foreach (var p in pois)
                {
                    FeaturedPOIs.Add(p);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToPOIAsync(POIDto poi)
        {
            await Shell.Current.GoToAsync("poiDetail",
                new Dictionary<string, object> { ["POI"] = poi });
        }

        [RelayCommand]
        private async Task NavigateToAllPOIsAsync()
        {
            await Shell.Current.GoToAsync("//poiList");
        }

        [RelayCommand]
        private async Task PlayAudioDirect(POIDto poi)
        {
            if (poi == null) return;

            if (!_authService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Dang nhap", "Ban can dang nhap de dang ky goi nghe thuyet minh.", OkText);
                await Shell.Current.GoToAsync("//login");
                return;
            }

            if (!await _subscriptionService.CanAccessAudioAsync())
            {
                var goToPackages = await Shell.Current.DisplayAlert("Can goi audio", "Ban can goi audio dang hoat dong de nghe thuyet minh.", "Dang ky goi", "De sau");
                if (goToPackages)
                {
                    await Shell.Current.GoToAsync("subscriptionPackages");
                }
                return;
            }

            var fullPoi = await _poiService.GetPOIByIdAsync(poi.Id, CurrentLanguage);
            if (fullPoi == null || !fullPoi.AudioGuides.Any())
            {
                await Shell.Current.DisplayAlert(NoAudioTitle, NoAudioMessage, OkText);
                return;
            }

            var guide = _audioService.GetGuideForPOI(fullPoi) ?? fullPoi.AudioGuides.First();
            _audioService.LoadGuide(guide, fullPoi);

            await Shell.Current.GoToAsync("audioPlayer",
                new Dictionary<string, object>
                {
                    ["AudioGuide"] = guide,
                    ["POI"] = fullPoi
                });
        }

        [RelayCommand]
        private async Task ChangeLanguageAsync()
        {
            string currentDisplay = CurrentLanguage switch {
                "vi" => "🇻🇳 Tiếng Việt",
                "en" => "🇬🇧 English",
                "zh" => "🇨🇳 中文",
                _ => "🇬🇧 English"
            };

            string title = CurrentLanguage == "vi" ? "Chọn Ngôn Ngữ" : 
                           (CurrentLanguage == "en" ? "Select Language" : "选择语言");
            string cancel = CurrentLanguage == "vi" ? "Hủy" : 
                            (CurrentLanguage == "en" ? "Cancel" : "取消");

            var action = await Shell.Current.DisplayActionSheet(
                $"{title} (Current: {currentDisplay})", 
                cancel, null, 
                "🇻🇳 Tiếng Việt", "🇬🇧 English", "🇨🇳 中文");

            string newLang = action switch
            {
                "🇻🇳 Tiếng Việt" => "vi",
                "🇬🇧 English" => "en",
                "🇨🇳 中文" => "zh",
                _ => CurrentLanguage
            };

            if (newLang != CurrentLanguage && !string.IsNullOrEmpty(newLang) && newLang != cancel)
            {
                CurrentLanguage = newLang;
                _audioService.SetLanguage(CurrentLanguage);
                UpdateGreeting();
                UpdateLocalizedTexts();
                await LoadFeaturedPOIsAsync();
            }
        }

        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            if (CurrentLanguage == "vi")
            {
                GreetingText = hour switch
                {
                    >= 5 and < 12 => "Chào buổi sáng!",
                    >= 12 and < 18 => "Chào buổi chiều!",
                    _ => "Chào buổi tối!"
                };
            }
            else if (CurrentLanguage == "en")
            {
                GreetingText = hour switch
                {
                    >= 5 and < 12 => "Good Morning!",
                    >= 12 and < 18 => "Good Afternoon!",
                    _ => "Good Evening!"
                };
            }
            else
            {
                GreetingText = hour switch
                {
                    >= 5 and < 12 => "早上好!",
                    >= 12 and < 18 => "下午好!",
                    _ => "晚上好!"
                };
            }
        }

        private void UpdateLocalizedTexts()
        {
            switch (CurrentLanguage)
            {
                case "en":
                    PageTitle = "Explore Cuisine";
                    SearchPlaceholder = "Find food spots...";
                    FeaturedSectionTitle = "✨ Featured Food Spots";
                    SeeAllText = "See all";
                    DiscoverMoreTitle = "🗺️ Discover more";
                    ListenNowText = "Listen now";
                    NoAudioTitle = "No audio";
                    NoAudioMessage = "This food spot does not have an audio guide yet.";
                    OkText = "OK";
                    break;
                case "zh":
                    PageTitle = "探索美食";
                    SearchPlaceholder = "查找美食地点...";
                    FeaturedSectionTitle = "✨ 热门美食地点";
                    SeeAllText = "查看全部";
                    DiscoverMoreTitle = "🗺️ 发现更多";
                    ListenNowText = "立即收听";
                    NoAudioTitle = "暂无音频";
                    NoAudioMessage = "该美食地点暂时还没有语音讲解。";
                    OkText = "确定";
                    break;
                default:
                    PageTitle = "Khám phá ẩm thực";
                    SearchPlaceholder = "Tìm điểm ẩm thực...";
                    FeaturedSectionTitle = "✨ Điểm ẩm thực nổi bật";
                    SeeAllText = "Xem tất cả";
                    DiscoverMoreTitle = "🗺️ Khám phá thêm";
                    ListenNowText = "Nghe ngay";
                    NoAudioTitle = "Không có audio";
                    NoAudioMessage = "Điểm ẩm thực này chưa có thuyết minh audio.";
                    OkText = "OK";
                    break;
            }
        }

        private void OnLanguageChanged(object? sender, string language)
        {
            CurrentLanguage = language;
            UpdateGreeting();
            UpdateLocalizedTexts();
            MainThread.BeginInvokeOnMainThread(async () => await LoadFeaturedPOIsAsync());
        }
    }
}
