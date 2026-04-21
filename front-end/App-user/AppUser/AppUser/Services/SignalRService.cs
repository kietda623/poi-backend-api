using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace AppUser.Services;

public class SignalRService : IAsyncDisposable
{
    private readonly ILogger<SignalRService> _logger;
    private HubConnection? _hubConnection;
    private string? _jwtToken;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService(ILogger<SignalRService> logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(string jwtToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            _logger.LogWarning("SignalR connect skipped: empty JWT.");
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            _jwtToken = jwtToken;

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
            _logger.LogInformation("Connected to UserTrackerHub.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect UserTrackerHub.");
            throw;
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
            _jwtToken = null;
            _logger.LogInformation("Disconnected from UserTrackerHub.");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private HubConnection BuildConnection()
    {
        var baseUrl = AppConfig.BaseApiUrl.TrimEnd('/');
        if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = baseUrl[..^4];
        }

        var hubUrl = $"{baseUrl}/hubs/user-tracker";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
                options.Headers["Authorization"] = $"Bearer {_jwtToken}";
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20)
            })
            .Build();

        connection.Reconnecting += error =>
        {
            _logger.LogWarning(error, "SignalR reconnecting...");
            return Task.CompletedTask;
        };

        connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR reconnected. ConnectionId: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        connection.Closed += error =>
        {
            _logger.LogWarning(error, "SignalR connection closed.");
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

