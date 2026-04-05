using System.Text;
using System.Text.Json;

namespace PoiApi.Services;

public class AzureTranslationService
{
    private readonly string _key;
    private readonly string _region;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureTranslationService> _logger;

    public AzureTranslationService(
        IConfiguration config, 
        HttpClient httpClient, 
        ILogger<AzureTranslationService> logger)
    {
        _key = config["Azure:SpeechKey"]!; // Giả định dùng chung key Multi-service
        _region = config["Azure:SpeechRegion"]!;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> TranslateAsync(string text, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(text) || targetLang == "vi") return text;
        var results = await TranslateListAsync(new List<string> { text }, targetLang);
        return results?.FirstOrDefault();
    }

    public async Task<List<string>?> TranslateListAsync(List<string> texts, string targetLang)
    {
        if (texts == null || !texts.Any() || targetLang == "vi") return texts;

        try
        {
            var endpoint = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=vi&to={targetLang}";
            
            var requestBody = texts.Select(t => new { Text = t }).ToArray();
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = content;
            request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", _region);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            
            var results = new List<string>();
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                results.Add(item.GetProperty("translations")[0].GetProperty("text").GetString() ?? "");
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureTranslationService bulk failure for {Lang}", targetLang);
            return texts; // Fallback
        }
    }
}
