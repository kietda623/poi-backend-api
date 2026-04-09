using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace PoiApi.Services;

public class PayOsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayOsOptions _options;

    public PayOsService(IHttpClientFactory httpClientFactory, IOptions<PayOsOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ClientId) &&
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !string.IsNullOrWhiteSpace(_options.ChecksumKey);

    public async Task<PayOsCreatePaymentResult> CreatePaymentLinkAsync(PayOsCreatePaymentRequest request)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("PayOS is not configured.");
        }

        request.Signature = CreatePaymentRequestSignature(request);

        var payload = new
        {
            orderCode = request.OrderCode,
            amount = request.Amount,
            description = request.Description,
            items = request.Items.Select(x => new
            {
                name = x.Name,
                quantity = x.Quantity,
                price = x.Price
            }),
            returnUrl = request.ReturnUrl,
            cancelUrl = request.CancelUrl,
            expiredAt = request.ExpiredAt,
            signature = request.Signature
        };

        var client = CreateClient();

        using var response = await client.PostAsJsonAsync("v2/payment-requests", payload);
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"PayOS request failed: {content}");
        }

        var apiResponse = JsonSerializer.Deserialize<PayOsApiResponse<PayOsCreatePaymentResult>>(content, JsonOptions());
        if (apiResponse == null)
        {
            throw new InvalidOperationException("PayOS returned an empty payment link response.");
        }

        if (!string.Equals(apiResponse.Code, "00", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"PayOS create payment failed: {apiResponse.Description ?? "Unknown error"}");
        }

        if (apiResponse.Data == null || string.IsNullOrWhiteSpace(apiResponse.Data.CheckoutUrl))
        {
            throw new InvalidOperationException($"PayOS create payment failed: {apiResponse.Description ?? "Missing checkoutUrl"}");
        }

        return apiResponse.Data;
    }

    public async Task<PayOsPaymentRequestInfoResult> GetPaymentLinkInfoAsync(string paymentReference)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("PayOS is not configured.");
        }

        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            throw new InvalidOperationException("PayOS payment reference is missing.");
        }

        var client = CreateClient();
        using var response = await client.GetAsync($"v2/payment-requests/{Uri.EscapeDataString(paymentReference)}");
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"PayOS request failed: {content}");
        }

        var apiResponse = JsonSerializer.Deserialize<PayOsApiResponse<PayOsPaymentRequestInfoResult>>(content, JsonOptions());
        if (apiResponse == null)
        {
            throw new InvalidOperationException("PayOS returned an empty payment info response.");
        }

        if (!string.Equals(apiResponse.Code, "00", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"PayOS get payment failed: {apiResponse.Description ?? "Unknown error"}");
        }

        if (apiResponse.Data == null)
        {
            throw new InvalidOperationException($"PayOS get payment failed: {apiResponse.Description ?? "Missing payment data"}");
        }

        return apiResponse.Data;
    }

    public bool VerifyWebhookSignature(JsonElement dataElement, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(_options.ChecksumKey))
        {
            return false;
        }

        var payload = BuildWebhookSignaturePayload(dataElement);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var computed = ComputeSignature(payload);
        return string.Equals(computed, signature.Trim().ToLowerInvariant(), StringComparison.Ordinal);
    }

    public string BuildReturnUrl(string audience, int? subscriptionId = null)
    {
        var baseUrl = _options.ReturnUrl;
        var separator = baseUrl.Contains('?') ? "&" : "?";
        var url = $"{baseUrl}{separator}audience={Uri.EscapeDataString(audience)}";
        if (subscriptionId.HasValue)
        {
            url += $"&subscriptionId={subscriptionId.Value}";
        }

        return url;
    }

    public string BuildCancelUrl(string audience, int? subscriptionId = null)
    {
        var baseUrl = _options.CancelUrl;
        var separator = baseUrl.Contains('?') ? "&" : "?";
        var url = $"{baseUrl}{separator}audience={Uri.EscapeDataString(audience)}";
        if (subscriptionId.HasValue)
        {
            url += $"&subscriptionId={subscriptionId.Value}";
        }

        return url;
    }

    public string BuildSellerPackageCallbackUrl(int? subscriptionId = null)
    {
        var baseUrl = ResolveSellerPackageBaseUrl();
        var separator = baseUrl.Contains('?') ? "&" : "?";
        var url = baseUrl;
        if (subscriptionId.HasValue)
        {
            url = $"{baseUrl}{separator}subscriptionId={subscriptionId.Value}";
        }

        return url;
    }

    private string ResolveSellerPackageBaseUrl()
    {
        if (Uri.TryCreate(_options.ReturnUrl, UriKind.Absolute, out var absoluteUri))
        {
            return $"{absoluteUri.Scheme}://{absoluteUri.Authority}/seller/service-packages";
        }

        if (Uri.TryCreate(_options.CancelUrl, UriKind.Absolute, out absoluteUri))
        {
            return $"{absoluteUri.Scheme}://{absoluteUri.Authority}/seller/service-packages";
        }

        return "http://localhost:5279/seller/service-packages";
    }

    private string CreatePaymentRequestSignature(PayOsCreatePaymentRequest request)
    {
        var payload =
            $"amount={request.Amount}" +
            $"&cancelUrl={request.CancelUrl ?? string.Empty}" +
            $"&description={request.Description ?? string.Empty}" +
            $"&orderCode={request.OrderCode}" +
            $"&returnUrl={request.ReturnUrl ?? string.Empty}";

        return ComputeSignature(payload);
    }

    private string ComputeSignature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private string BuildWebhookSignaturePayload(JsonElement dataElement)
    {
        if (dataElement.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return string.Join("&", dataElement.EnumerateObject()
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => $"{x.Name}={ConvertWebhookValue(x.Value)}"));
    }

    private static string ConvertWebhookValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            JsonValueKind.Object => JsonSerializer.Serialize(ToSortedObject(value), JsonOptions()),
            JsonValueKind.Array => JsonSerializer.Serialize(ToSortedArray(value), JsonOptions()),
            _ => value.ToString()
        };
    }

    private static SortedDictionary<string, object?> ToSortedObject(JsonElement value)
    {
        var result = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            result[property.Name] = ConvertWebhookNode(property.Value);
        }

        return result;
    }

    private static List<object?> ToSortedArray(JsonElement value)
    {
        return value.EnumerateArray()
            .Select(ConvertWebhookNode)
            .ToList();
    }

    private static object? ConvertWebhookNode(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Object => ToSortedObject(value),
            JsonValueKind.Array => ToSortedArray(value),
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => ConvertNumber(value),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => value.ToString()
        };
    }

    private static object ConvertNumber(JsonElement value)
    {
        if (value.TryGetInt64(out var longValue))
        {
            return longValue;
        }

        if (value.TryGetDecimal(out var decimalValue))
        {
            return decimalValue;
        }

        return value.GetRawText();
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Add("x-client-id", _options.ClientId);
        client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        return client;
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}

public class PayOsCreatePaymentRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public long ExpiredAt { get; set; }
    public string Signature { get; set; } = string.Empty;
    public List<PayOsPaymentItem> Items { get; set; } = new();
}

public class PayOsPaymentItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
}

public class PayOsCreatePaymentResult
{
    [JsonPropertyName("paymentLinkId")]
    public string PaymentLinkId { get; set; } = string.Empty;

    [JsonPropertyName("checkoutUrl")]
    public string CheckoutUrl { get; set; } = string.Empty;

    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; }
}

public class PayOsWebhookPayload
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("data")]
    public PayOsWebhookData? Data { get; set; }
}

public class PayOsWebhookData
{
    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("paymentLinkId")]
    public string? PaymentLinkId { get; set; }
}

public class PayOsPaymentRequestInfoResult
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("amountPaid")]
    public int AmountPaid { get; set; }

    [JsonPropertyName("amountRemaining")]
    public int AmountRemaining { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class PayOsApiResponse<T>
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
