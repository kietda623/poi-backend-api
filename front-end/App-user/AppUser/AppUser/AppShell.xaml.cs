using AppUser.Pages;
using AppUser.Services;

namespace AppUser
{
    public partial class AppShell : Shell
    {
        private readonly AudioService _audioService;

        public AppShell(AudioService audioService)
        {
            InitializeComponent();
            _audioService = audioService;
            _audioService.LanguageChanged += OnLanguageChanged;
            UpdateTabTitles(_audioService.CurrentLanguage);

            Routing.RegisterRoute("poiDetail", typeof(POIDetailPage));
            Routing.RegisterRoute("audioPlayer", typeof(AudioPlayerPage));
            Routing.RegisterRoute("subscriptionPackages", typeof(SubscriptionPage));
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("editProfile", typeof(EditProfilePage));
        }

        private void OnLanguageChanged(object? sender, string language)
        {
            MainThread.BeginInvokeOnMainThread(() => UpdateTabTitles(language));
        }

        private void UpdateTabTitles(string language)
        {
            switch (language)
            {
                case "en":
                    HomeTab.Title = "Home";
                    ExploreTab.Title = "Explore";
                    ProfileTab.Title = "Profile";
                    break;
                case "zh":
                    HomeTab.Title = "首页";
                    ExploreTab.Title = "探索";
                    ProfileTab.Title = "资料";
                    break;
                default:
                    HomeTab.Title = "Trang chủ";
                    ExploreTab.Title = "Khám phá";
                    ProfileTab.Title = "Hồ sơ";
                    break;
            }
        }
    }
}
