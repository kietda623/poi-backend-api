using System.Net.Http;
using System.Net.Http.Json;
using AppUser.Models;

namespace AppUser.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public AiService(AuthService authService)
    {
        _authService = authService;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.BaseApiUrl)
        };
    }

    private void ApplyAuth()
    {
        var token = _authService.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // === AI Subscription Info ===
    public async Task<AiSubscriptionInfoDto?> GetSubscriptionInfoAsync()
    {
        try
        {
            ApplyAuth();
            var response = await _httpClient.GetAsync("ai/subscription-info");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AiSubscriptionInfoDto>(new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting AI subscription info: {ex.Message}");
            return null;
        }
    }

    // === Tour Plan ===
    public async Task<AiTourPlanResponseDto?> GenerateTourPlanAsync(AiTourPlanRequestDto request)
    {
        try
        {
            ApplyAuth();
            var response = await _httpClient.PostAsJsonAsync("ai/tour-plan", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AiTourPlanResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating tour plan: {ex.Message}");
            return null;
        }
    }

    // === Chatbot ===
    public async Task<AiChatResponseDto?> ChatWithThoDiaAsync(AiChatbotRequestDto request)
    {
        try
        {
            ApplyAuth();
            var response = await _httpClient.PostAsJsonAsync("ai/chatbot", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AiChatResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in chatbot: {ex.Message}");
            return null;
        }
    }

    // === Tinder API ===
    public async Task<TinderCardsResponseDto?> GetTinderCardsAsync(int count = 10)
    {
        try
        {
            ApplyAuth();
            var response = await _httpClient.GetAsync($"app/tinder/cards?count={count}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TinderCardsResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tinder cards: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SwipeAsync(int shopId, bool isLiked)
    {
        try
        {
            ApplyAuth();
            var response = await _httpClient.PostAsJsonAsync("app/tinder/swipe", new { shopId, isLiked });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error swiping: {ex.Message}");
            return false;
        }
    }

    public async Task<LikedShopsResponseDto?> GetLikedShopsAsync()
    {
        try
        {
            ApplyAuth();
            var response = await _httpClient.GetAsync("app/tinder/liked");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LikedShopsResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting liked shops: {ex.Message}");
            return null;
        }
    }
}
