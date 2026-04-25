using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace AppUser.Services;

public class AppPresenceService : IAsyncDisposable
{
    private readonly ILogger<AppPresenceService> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private HubConnection? _hubConnection;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public AppPresenceService(ILogger<AppPresenceService> logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_hubConnection == null)
            {
                _hubConnection = BuildConnection();
            }

            if (_hubConnection.State == HubConnectionState.Connected ||
                _hubConnection.State == HubConnectionState.Connecting ||
                _hubConnection.State == HubConnectionState.Reconnecting)
            {
                return;
            }

            await _hubConnection.StartAsync(cancellationToken);
            _logger.LogInformation("Connected to AppPresenceHub.");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_hubConnection == null)
            {
                return;
            }

            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync(cancellationToken);
            }

            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            _logger.LogInformation("Disconnected from AppPresenceHub.");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private HubConnection BuildConnection()
    {
        var hubUrl = $"{AppConfig.BaseDomain.TrimEnd('/')}/hubs/app-presence";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.Headers["ngrok-skip-browser-warning"] = "true";
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            })
            .Build();

        connection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "AppPresenceHub reconnecting...");
            return Task.CompletedTask;
        };

        connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("AppPresenceHub reconnected. ConnectionId: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        connection.Closed += error =>
        {
            _logger.LogWarning(error, "AppPresenceHub connection closed.");
            return Task.CompletedTask;
        };

        return connection;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _connectionLock.Dispose();
    }
}
