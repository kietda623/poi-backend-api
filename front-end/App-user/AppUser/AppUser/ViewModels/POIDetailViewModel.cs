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

        public POIDetailViewModel(AudioService audio, POIService poiService)
        {
            _audioService = audio;
            _poiService = poiService;
            CurrentLanguage = _audioService.CurrentLanguage;
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
            }
        }

        [RelayCommand]
        private async Task PlayAudioAsync()
        {
            if (POI == null || CurrentAudioGuide == null) return;

            _audioService.LoadGuide(CurrentAudioGuide, POI);

            await Shell.Current.GoToAsync("audioPlayer",
                new Dictionary<string, object>
                {
                    ["AudioGuide"] = CurrentAudioGuide,
                    ["POI"] = POI
                });
        }

        [RelayCommand]
        private async Task ToggleLanguageAsync()
        {
            CurrentLanguage = CurrentLanguage switch
            {
                "vi" => "en",
                "en" => "zh",
                _ => "vi"
            };
            UpdateLocalization();
            
            if (POI != null)
            {
                // Refresh data from API for the new language
                var updatedPOI = await _poiService.GetPOIByIdAsync(POI.Id, CurrentLanguage);
                if (updatedPOI != null)
                {
                    POI = updatedPOI;
                }

                CurrentAudioGuide = _audioService.GetGuideForPOI(POI!);
                HasAudio = CurrentAudioGuide != null;
            }
            
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(DisplayDescription));
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
