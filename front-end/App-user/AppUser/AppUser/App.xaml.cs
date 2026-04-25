using AppUser.Services;

namespace AppUser;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthService _authService;
    private readonly AppPresenceService _appPresenceService;

    public App(
        IServiceProvider serviceProvider,
        AuthService authService,
        AppPresenceService appPresenceService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = authService;
        _appPresenceService = appPresenceService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        var window = new Window(appShell);

        window.Created += async (_, _) =>
        {
            await SafeAppSessionInitializationAsync();
            await SafePresenceActionAsync(() => _appPresenceService.ConnectAsync());
        };
        window.Activated += async (_, _) => await SafePresenceActionAsync(() => _appPresenceService.ConnectAsync());
        window.Resumed += async (_, _) => await SafePresenceActionAsync(() => _appPresenceService.ConnectAsync());
        window.Stopped += async (_, _) => await SafePresenceActionAsync(() => _appPresenceService.DisconnectAsync());
        window.Destroying += async (_, _) => await SafePresenceActionAsync(() => _appPresenceService.DisconnectAsync());

        return window;
    }

    private async Task SafeAppSessionInitializationAsync()
    {
        try
        {
            await _authService.EnsureSessionLoadedAsync();
            if (!_authService.IsLoggedIn)
            {
                await _authService.InitGuestSessionAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App session initialization error: {ex.Message}");
        }
    }

    private static async Task SafePresenceActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App presence lifecycle error: {ex.Message}");
        }
    }
}
