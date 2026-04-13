using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PoiApi.Services;

/// <summary>
/// Service to call Groq Cloud API for AI-powered features
/// (Tour Plan generation, Chatbot "Tho Dia").
/// Uses OpenAI-compatible chat completions endpoint.
/// </summary>
public class GroqService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GroqService> _logger;

    public GroqService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<GroqService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = config["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey is not configured.");
        
        // Using llama-3.3-70b-versatile as default high-performance model
        _model = config["Groq:Model"] ?? "llama-3.3-70b-versatile";
            
        _logger = logger;
    }

    /// <summary>
    /// Send a prompt to Groq and get text response.
    /// </summary>
    public async Task<string?> GenerateContentAsync(
        string systemInstruction,
        string userMessage,
        List<GroqChatTurn>? history = null)
    {
        try
        {
            var messages = new List<object>();

            // Add system instruction
            messages.Add(new { role = "system", content = systemInstruction });

            // Add history if any
            if (history != null)
            {
                foreach (var turn in history)
                {
                    messages.Add(new
                    {
                        role = turn.Role, // "user" or "assistant"
                        content = turn.Message
                    });
                }
            }

            // Add current user message
            messages.Add(new
            {
                role = "user",
                content = userMessage
            });

            var requestBody = new
            {
                model = _model,
                messages,
                temperature = 0.7,
                max_tokens = 4096,
                top_p = 1,
                stream = false,
                stop = (string[]?)null
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var url = "https://api.groq.com/openai/v1/chat/completions";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Groq API error, status {StatusCode}: {Body}", response.StatusCode, responseBody);
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq response parse failed");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GroqService.GenerateContentAsync failed");
            return null;
        }
    }
}

/// <summary>Represents a single turn in chatbot conversation history</summary>
public class GroqChatTurn
{
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public string Message { get; set; } = string.Empty;
}
