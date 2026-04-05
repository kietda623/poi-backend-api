using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;

namespace AppUser.ViewModels
{
    [QueryProperty(nameof(AudioGuide), "AudioGuide")]
    [QueryProperty(nameof(POI), "POI")]
    public partial class AudioPlayerViewModel : ObservableObject
    {
        private readonly AudioService _audioService;

        [ObservableProperty]
        private AudioGuideDto? audioGuide;

        [ObservableProperty]
        private POIDto? pOI;

        [ObservableProperty]
        private bool isPlaying = false;

        [ObservableProperty]
        private double progress = 0;

        [ObservableProperty]
        private string positionText = "00:00";

        [ObservableProperty]
        private string durationText = "00:00";

        [ObservableProperty]
        private double playbackSpeed = 1.0;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string nowPlayingLabel = "ĐANG THƯỞNG THỨC";
        [ObservableProperty]
        private string subTitleLabel = "Thuyết minh văn hóa ẩm thực";

        [ObservableProperty]
        private string currentLangCode = "vi";

        private readonly POIService _poiService;

        public string POIName => POI?.DisplayName(CurrentLangCode) ?? string.Empty;
        public string GuideTitle => AudioGuide?.Title ?? string.Empty;
        public string ImageUrl => POI?.ImageUrl ?? string.Empty;
        public List<AppLanguageDto> AvailableLanguages => POI?.AvailableLanguages ?? new();
        public bool HasMultipleLanguages => AvailableLanguages.Count > 1;

        // Xử lý URL Audio đầy đủ
        public string AudioUrl
        {
            get
            {
                if (AudioGuide == null || string.IsNullOrWhiteSpace(AudioGuide.AudioUrl)) 
                    return string.Empty;
                
                // Sử dụng AppConfig để tự động xử lý URL đầy đủ từ backend
                return AppConfig.ResolveUrl(AudioGuide.AudioUrl);
            }
        }

        // Các sự kiện để Code-behind điều khiển MediaElement
        public event EventHandler? PlayRequested;
        public event EventHandler? PauseRequested;
        public event EventHandler<double>? SeekRequested;

        // Available speed options
        public List<double> SpeedOptions = new() { 0.75, 1.0, 1.25, 1.5, 2.0 };

        public AudioPlayerViewModel(AudioService audio, POIService poiService)
        {
            _audioService = audio;
            _poiService = poiService;
        }

        partial void OnAudioGuideChanged(AudioGuideDto? value)
        {
            if (value != null)
            {
                DurationText = value.DurationDisplay;
                OnPropertyChanged(nameof(GuideTitle));
                OnPropertyChanged(nameof(AudioUrl));
            }
        }

        partial void OnPOIChanged(POIDto? value)
        {
            OnPropertyChanged(nameof(POIName));
            OnPropertyChanged(nameof(ImageUrl));
            OnPropertyChanged(nameof(AvailableLanguages));
            OnPropertyChanged(nameof(HasMultipleLanguages));
        }

        [RelayCommand]
        private async Task SwitchLanguage(AppLanguageDto lang)
        {
            if (lang == null || lang.Code == CurrentLangCode || !lang.HasAudio) return;

            IsLoading = true;
            try
            {
                PauseRequested?.Invoke(this, EventArgs.Empty);
                
                // Tải lại thông tin POI với ngôn ngữ mới
                var updatedPoi = await _poiService.GetPOIByIdAsync(POI!.Id, lang.Code);
                if (updatedPoi != null)
                {
                    CurrentLangCode = lang.Code;
                    POI = updatedPoi;
                    
                    // Cập nhật AudioGuide mới
                    if (updatedPoi.AudioGuides.Any())
                    {
                        AudioGuide = updatedPoi.AudioGuides.First();
                        PlayRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                IsLoading = false;
                UpdateLocalization();
            }
        }

        private void UpdateLocalization()
        {
            if (CurrentLangCode == "vi")
            {
                NowPlayingLabel = "ĐANG THƯỞNG THỨC";
                SubTitleLabel = "Thuyết minh văn hóa ẩm thực";
            }
            else if (CurrentLangCode == "zh")
            {
                NowPlayingLabel = "正在播放";
                SubTitleLabel = "饮食文化语音讲解";
            }
            else // Default to English
            {
                NowPlayingLabel = "NOW PLAYING";
                SubTitleLabel = "Culinary Culture Guide";
            }
        }

        [RelayCommand]
        private void TogglePlayPause()
        {
            if (IsPlaying)
                PauseRequested?.Invoke(this, EventArgs.Empty);
            else
                PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void SeekBackward()
        {
            var newPos = Math.Max(0, Progress - 0.05); // Lùi 5%
            SeekRequested?.Invoke(this, newPos);
        }

        [RelayCommand]
        private void SeekForward()
        {
            var newPos = Math.Min(1.0, Progress + 0.05); // Tiến 5%
            SeekRequested?.Invoke(this, newPos);
        }

        [RelayCommand]
        private void CycleSpeed()
        {
            var idx = SpeedOptions.IndexOf(PlaybackSpeed);
            PlaybackSpeed = SpeedOptions[(idx + 1) % SpeedOptions.Count];
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            PauseRequested?.Invoke(this, EventArgs.Empty);
            _audioService.SetPlayState(false);
            await Shell.Current.GoToAsync("..");
        }

        /// <summary>Called from code-behind when MediaElement position changes</summary>
        public void UpdateProgress(TimeSpan position, TimeSpan duration)
        {
            _audioService.Position = position;
            _audioService.Duration = duration;
            Progress = _audioService.Progress;
            PositionText = $"{(int)position.TotalMinutes:D2}:{position.Seconds:D2}";
            if (duration != TimeSpan.Zero)
                DurationText = $"{(int)duration.TotalMinutes:D2}:{duration.Seconds:D2}";
        }
    }
}
