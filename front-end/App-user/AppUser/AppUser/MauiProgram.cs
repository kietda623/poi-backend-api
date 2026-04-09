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
                    fonts.AddFont("Poppins-Regular.ttf", "Poppins");
                    fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
                    fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                });

            // Register Services
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<POIService>();
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton<SubscriptionService>();
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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
