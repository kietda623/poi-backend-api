using AppUser.Services;
using AppUser.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

namespace AppUser
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    
                    // Be Vietnam Pro - Premium Vietnamese Font
                    fonts.AddFont("BeVietnamPro-Regular.ttf", "BeVietnamPro");
                    fonts.AddFont("BeVietnamPro-Medium.ttf", "BeVietnamProMedium");
                    fonts.AddFont("BeVietnamPro-SemiBold.ttf", "BeVietnamProSemiBold");
                    fonts.AddFont("BeVietnamPro-Bold.ttf", "BeVietnamProBold");

                });

            // Register Services
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<POIService>();
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton<SubscriptionService>();
            builder.Services.AddSingleton<AiService>();
            builder.Services.AddSingleton<AppShell>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<POIListViewModel>();
            builder.Services.AddTransient<POIDetailViewModel>();
            builder.Services.AddTransient<AudioPlayerViewModel>();
            builder.Services.AddTransient<SubscriptionViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<EditProfileViewModel>();
            builder.Services.AddTransient<TinderViewModel>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<TourPlanViewModel>();

            // Register Pages
            builder.Services.AddTransient<Pages.LoginPage>();
            builder.Services.AddTransient<Pages.HomePage>();
            builder.Services.AddTransient<Pages.POIListPage>();
            builder.Services.AddTransient<Pages.POIDetailPage>();
            builder.Services.AddTransient<Pages.AudioPlayerPage>();
            builder.Services.AddTransient<Pages.SubscriptionPage>();
            builder.Services.AddTransient<Pages.ProfilePage>();
            builder.Services.AddTransient<Pages.RegisterPage>();
            builder.Services.AddTransient<Pages.EditProfilePage>();
            builder.Services.AddTransient<Pages.TinderPage>();
            builder.Services.AddTransient<Pages.ChatPage>();
            builder.Services.AddTransient<Pages.TourPlanPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
