using System.Net.Http.Json;
using System.Text.Json;

namespace foodstreet_admin.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient http, ILogger<ApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _http.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET {Endpoint} failed", endpoint);
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST {Endpoint} failed", endpoint);
            return default;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(endpoint, data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT {Endpoint} failed", endpoint);
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _http.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE {Endpoint} failed", endpoint);
            return false;
        }
    }

    public async Task<bool> PatchAsync(string endpoint)
    {
        try
        {
            var response = await _http.PatchAsync(endpoint, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PATCH {Endpoint} failed", endpoint);
            return false;
        }
    }
}
