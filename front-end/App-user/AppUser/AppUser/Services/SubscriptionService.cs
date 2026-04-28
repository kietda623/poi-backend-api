using AppUser.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace AppUser.Services;

public class SubscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public SubscriptionService(AuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.BaseApiUrl)
        };
        AppConfig.ConfigureHttpClient(_httpClient);
    }

    public async Task<List<AppServicePackageDto>> GetUserPackagesAsync()
    {
        EnsureAuthenticated();

        try
        {
            ApplyAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<AppServicePackageDto>>("app/subscriptions/packages") ?? new();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetUserPackagesAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<AppSubscriptionEnvelopeDto> GetMySubscriptionAsync()
    {
        EnsureAuthenticated();

        try
        {
            ApplyAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<AppSubscriptionEnvelopeDto>("app/subscriptions/my")
                ?? new AppSubscriptionEnvelopeDto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetMySubscriptionAsync error: {ex.Message}");
            return new AppSubscriptionEnvelopeDto();
        }
    }

    public async Task<List<AppCurrentSubscriptionDto>> GetHistoryAsync()
    {
        EnsureAuthenticated();

        try
        {
            ApplyAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<AppCurrentSubscriptionDto>>("app/subscriptions/history") ?? new();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetHistoryAsync error: {ex.Message}");
            return new();
        }
    }

    public async Task<AppCheckoutSubscriptionResultDto?> CreateCheckoutAsync(int packageId, string billingCycle)
    {
        EnsureAuthenticated();

        try
        {
            ApplyAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("app/subscriptions", new
            {
                packageId,
                billingCycle
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorMessageAsync(response);
                throw new InvalidOperationException(error);
            }

            return await response.Content.ReadFromJsonAsync<AppCheckoutSubscriptionResultDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateCheckoutAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<AppSubscriptionEnvelopeDto> SyncPaymentAsync(int id)
    {
        EnsureAuthenticated();

        try
        {
            ApplyAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync($"app/subscriptions/{id}/sync-payment", new { });
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorMessageAsync(response);
                throw new InvalidOperationException(error);
            }

            return await response.Content.ReadFromJsonAsync<AppSubscriptionEnvelopeDto>() ?? new AppSubscriptionEnvelopeDto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SyncPaymentAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<AppSubscriptionEnvelopeDto?> TrySyncPaymentAsync(int id)
    {
        try
        {
            return await SyncPaymentAsync(id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TrySyncPaymentAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CancelAsync(int id)
    {
        EnsureAuthenticated();

        try
        {
            ApplyAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"app/subscriptions/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CancelAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CanAccessAudioAsync()
    {
        if (string.IsNullOrWhiteSpace(_authService.Token))
        {
            return false;
        }

        try
        {
            var current = await GetMySubscriptionAsync();
            return current.CanAccessAudio;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CanAccessAudioAsync error: {ex.Message}");
            return false;
        }
    }

    private void EnsureAuthenticated()
    {
        if (string.IsNullOrWhiteSpace(_authService.Token))
        {
            throw new InvalidOperationException("Ban can xac thuc de su dung tinh nang nay.");
        }
    }

    private void ApplyAuthorizationHeader()
    {
        var token = _authService.Token;
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Unable to process package request.";
        }

        try
        {
            var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(raw);
            if (payload != null &&
                payload.TryGetValue("message", out var message) &&
                !string.IsNullOrWhiteSpace(message))
            {
                return message;
            }
        }
        catch
        {
        }

        return raw;
    }
}
