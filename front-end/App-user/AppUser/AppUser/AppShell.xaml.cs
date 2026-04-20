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
            // Routes mới cho Guest Access & QR scanning
            Routing.RegisterRoute("qrScanner", typeof(QrScannerPage));
            Routing.RegisterRoute("login", typeof(LoginPage));
            // Chat vẫn accessible nhưng không còn là bottom tab
            Routing.RegisterRoute("chat", typeof(ChatPage));
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
                    ScanQrTab.Title = "Scan QR";
                    TourPlanTab.Title = "Tour Plan";
                    ProfileTab.Title = "Profile";
                    break;
                case "zh":
                    HomeTab.Title = "首页";
                    ExploreTab.Title = "探索";
                    ScanQrTab.Title = "扫描";
                    TourPlanTab.Title = "行程";
                    ProfileTab.Title = "资料";
                    break;
                default:
                    HomeTab.Title = "Trang chủ";
                    ExploreTab.Title = "Khám phá";
                    ScanQrTab.Title = "Quét QR";
                    TourPlanTab.Title = "Hành trình";
                    ProfileTab.Title = "Hồ sơ";
                    break;
            }
        }
    }
}
