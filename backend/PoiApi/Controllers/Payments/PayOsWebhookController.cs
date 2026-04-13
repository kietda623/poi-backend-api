using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Services;

namespace PoiApi.Controllers.Payments;

[ApiController]
[Route("api/payments/payos")]
public class PayOsWebhookController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PayOsService _payOsService;
    private readonly SubscriptionAccessService _subscriptionAccessService;
    private readonly ILogger<PayOsWebhookController> _logger;

    public PayOsWebhookController(
        AppDbContext context,
        PayOsService payOsService,
        SubscriptionAccessService subscriptionAccessService,
        ILogger<PayOsWebhookController> logger)
    {
        _context = context;
        _payOsService = payOsService;
        _subscriptionAccessService = subscriptionAccessService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveWebhook()
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        using var document = JsonDocument.Parse(rawBody);
        if (!document.RootElement.TryGetProperty("data", out var dataElement))
        {
            return BadRequest(new { message = "Invalid webhook payload." });
        }

        var signature = document.RootElement.TryGetProperty("signature", out var signatureElement)
            ? signatureElement.GetString()
            : null;

        if (!_payOsService.VerifyWebhookSignature(dataElement, signature))
        {
            return Unauthorized(new { message = "Invalid PayOS signature." });
        }

        var payload = JsonSerializer.Deserialize<PayOsWebhookPayload>(rawBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (payload?.Data == null)
        {
            return BadRequest(new { message = "Invalid webhook payload." });
        }

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(x => x.PaymentOrderCode == payload.Data.OrderCode);
        if (subscription == null)
        {
            return NotFound(new { message = "Subscription not found." });
        }

        if (!string.IsNullOrWhiteSpace(payload.Data.PaymentLinkId))
        {
            subscription.PaymentLinkId = payload.Data.PaymentLinkId;
        }

        var paidAmount = payload.Success ? payload.Data.Amount : 0;
        var effectiveStatus = _subscriptionAccessService.ResolveEffectivePayOsState(
            payload.Data.Status,
            payload.Data.Amount,
            paidAmount,
            payload.Success);
        _logger.LogInformation(
            "Webhook payment for subscription {SubscriptionId}: success={Success}, rawStatus={RawStatus}, amount={Amount}, amountPaid={AmountPaid}, effectiveStatus={EffectiveStatus}",
            subscription.Id,
            payload.Success,
            payload.Data.Status,
            payload.Data.Amount,
            paidAmount,
            effectiveStatus);
        await _subscriptionAccessService.ApplyPayOsPaymentStateAsync(subscription, effectiveStatus);

        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
