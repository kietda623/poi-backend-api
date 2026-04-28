using AppUser.Models;

namespace AppUser.Services
{
    /// <summary>
    /// AudioService - Manages audio playback state for POI audio guides.
    /// MAUI MediaElement handles actual streaming from URL.
    /// </summary>
    public class AudioService
    {
        public event EventHandler<string>? LanguageChanged;
        // ─── Playback State ───────────────────────────────────────────────────
        public AudioGuideDto? CurrentGuide { get; private set; }
        public bool IsPlaying { get; private set; }
        public TimeSpan Position { get; set; } = TimeSpan.Zero;
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        // ─── History ──────────────────────────────────────────────────────────
        private readonly List<(POIDto POI, DateTime ListenedAt)> _history = new();
        public IReadOnlyList<(POIDto POI, DateTime ListenedAt)> History => _history;

        // ─── Language Preference ─────────────────────────────────────────────
        public string CurrentLanguage { get; private set; } = "en";

        /// <summary>Load an audio guide for playback</summary>
        public void LoadGuide(AudioGuideDto guide, POIDto poi)
        {
            if (CurrentGuide?.Id != guide.Id)
            {
                CurrentGuide = guide;
                IsPlaying = false;
                Position = TimeSpan.Zero;

                // Add to history if not already the most recent
                if (_history.Count == 0 || _history.Last().POI.Id != poi.Id)
                    _history.Add((poi, DateTime.Now));
            }
        }

        /// <summary>Set play state</summary>
        public void SetPlayState(bool isPlaying) => IsPlaying = isPlaying;

        /// <summary>Switch language for audio guides</summary>
        public void SetLanguage(string langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode))
            {
                langCode = "en";
            }

            if (CurrentLanguage == langCode)
            {
                return;
            }

            CurrentLanguage = langCode;
            LanguageChanged?.Invoke(this, CurrentLanguage);
        }

        /// <summary>Get audio guide for a POI in current language</summary>
        public AudioGuideDto? GetGuideForPOI(POIDto poi)
        {
            // First try current language, fallback to English.
            return poi.AudioGuides.FirstOrDefault(g => g.LanguageCode == CurrentLanguage)
                ?? poi.AudioGuides.FirstOrDefault(g => g.LanguageCode == "en")
                ?? poi.AudioGuides.FirstOrDefault();
        }

        /// <summary>Get recent listen history (last 20)</summary>
        public List<(POIDto POI, DateTime ListenedAt)> GetRecentHistory()
            => _history.TakeLast(20).Reverse().ToList();

        /// <summary>Get progress (0.0 - 1.0)</summary>
        public double Progress
        {
            get
            {
                if (Duration == TimeSpan.Zero) return 0;
                return Position.TotalSeconds / Duration.TotalSeconds;
            }
        }
    }
}
