using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;

namespace AppUser.ViewModels
{
    [QueryProperty(nameof(POI), "POI")]
    public partial class POIDetailViewModel : ObservableObject
    {
        private readonly AudioService _audioService;
        private readonly POIService _poiService;
        private readonly AuthService _authService;
        private readonly SubscriptionService _subscriptionService;
        private bool _isResolvingAudio;
        private int? _lastTrackedViewPoiId;

        [ObservableProperty]
        private POIDto? pOI;

        [ObservableProperty]
        private AudioGuideDto? currentAudioGuide;

        [ObservableProperty]
        private string currentLanguage = "vi";

        [ObservableProperty]
        private bool hasAudio = false;

        [ObservableProperty]
        private string introLabel = "Giới thiệu";
        [ObservableProperty]
        private string menuLabel = "Thực đơn";
        [ObservableProperty]
        private string audioGuideLabel = "Thuyết minh audio";
        [ObservableProperty]
        private string playButtonLabel = "Nghe";
        [ObservableProperty]
        private string listenNowLabel = "Nghe thuyết minh ngay";
        [ObservableProperty]
        private string inStockLabel = "Còn hàng";
        [ObservableProperty]
        private string noAudioLabel = "Chưa có audio thuyết minh cho ngôn ngữ này";

        public string DisplayName => POI?.DisplayName(CurrentLanguage) ?? string.Empty;
        public string DisplayDescription => POI?.DisplayDescription(CurrentLanguage) ?? string.Empty;

        public POIDetailViewModel(AudioService audio, POIService poiService, AuthService authService, SubscriptionService subscriptionService)
        {
            _audioService = audio;
            _poiService = poiService;
            _authService = authService;
            _subscriptionService = subscriptionService;
            CurrentLanguage = "vi";
            _audioService.SetLanguage("vi");
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            if (CurrentLanguage == "vi")
            {
                IntroLabel = "Giới thiệu";
                MenuLabel = "Thực đơn";
                AudioGuideLabel = "Thuyết minh audio";
                PlayButtonLabel = "Nghe";
                ListenNowLabel = "Nghe thuyết minh ngay";
                InStockLabel = "Còn hàng";
                NoAudioLabel = "Chưa có audio thuyết minh cho ngôn ngữ này";
            }
            else if (CurrentLanguage == "en")
            {
                IntroLabel = "Introduction";
                MenuLabel = "Menu";
                AudioGuideLabel = "Audio Guide";
                PlayButtonLabel = "Play";
                ListenNowLabel = "Listen now";
                InStockLabel = "In Stock";
                NoAudioLabel = "No audio guide available for this language";
            }
            else if (CurrentLanguage == "zh")
            {
                IntroLabel = "介绍";
                MenuLabel = "菜单";
                AudioGuideLabel = "语音讲解";
                PlayButtonLabel = "播放";
                ListenNowLabel = "立即收听";
                InStockLabel = "有现货";
                NoAudioLabel = "此语言暂无语音讲解";
            }
        }

        partial void OnPOIChanged(POIDto? value)
        {
            if (value != null)
            {
                CurrentAudioGuide = _audioService.GetGuideForPOI(value);
                HasAudio = CurrentAudioGuide != null;
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DisplayDescription));

                // Track view once per POI detail open/change.
                if (_lastTrackedViewPoiId != value.Id)
                {
                    _lastTrackedViewPoiId = value.Id;
                    _ = _poiService.TrackViewAsync(value.Id);
                }

                // Resolve Vietnamese audio once, without mutating POI again (avoid recursion loop).
                _ = EnsureVietnameseAudioAsync(value.Id);
            }
        }

        public async Task InitializeAsync()
        {
            if (POI == null) return;
            await EnsureVietnameseAudioAsync(POI.Id);
        }

        [RelayCommand]
        private async Task PlayAudioAsync()
        {
            if (POI == null) return;

            if (!_authService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Dang nhap", "Ban can dang nhap de dang ky goi nghe thuyet minh.", "OK");
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

            var guide = CurrentAudioGuide;
            if (guide == null)
            {
                var latestPoi = await _poiService.GetPOIByIdAsync(POI.Id, "vi") ?? POI;
                guide = latestPoi.AudioGuides.FirstOrDefault(g => g.LanguageCode == "vi")
                    ?? latestPoi.AudioGuides.FirstOrDefault();
                if (guide != null)
                {
                    POI = latestPoi;
                    CurrentAudioGuide = guide;
                    HasAudio = true;
                }
            }

            if (guide == null) return;

            var audioUrl = AppConfig.ResolveUrl(guide.AudioUrl);
            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                await Shell.Current.DisplayAlert("Lỗi audio", "Không tìm thấy file thuyết minh tiếng Việt.", "OK");
                return;
            }

            _audioService.LoadGuide(guide, POI);

            await Shell.Current.GoToAsync("audioPlayer",
                new Dictionary<string, object>
                {
                    ["AudioGuide"] = guide,
                    ["POI"] = POI
                });
        }

        [RelayCommand]
        private async Task ToggleLanguageAsync()
        {
            await Shell.Current.DisplayAlert("Thông báo", "Hiện tại app đang ưu tiên phát thuyết minh tiếng Việt mặc định.", "OK");
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async Task EnsureVietnameseAudioAsync(int poiId)
        {
            if (_isResolvingAudio) return;
            try
            {
                _isResolvingAudio = true;
                var latestPoi = await _poiService.GetPOIByIdAsync(poiId, "vi");
                if (latestPoi == null) return;

                var viGuide = latestPoi.AudioGuides.FirstOrDefault(g => g.LanguageCode == "vi")
                    ?? latestPoi.AudioGuides.FirstOrDefault();
                if (viGuide == null) return;

                CurrentAudioGuide = viGuide;
                HasAudio = true;
            }
            catch
            {
                // Keep current state when network is unavailable.
            }
            finally
            {
                _isResolvingAudio = false;
            }
        }
    }
}
