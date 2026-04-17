using System.Security.Cryptography;
using System.Text;
using Aethon.Data;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Infrastructure.Workers;

public sealed class WebhookDeliveryWorker : BackgroundService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDeliveryWorker> _logger;

    public WebhookDeliveryWorker(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookDeliveryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook delivery worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingDeliveriesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in webhook delivery worker.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("Webhook delivery worker stopped.");
    }

    private async Task ProcessPendingDeliveriesAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();

        var pending = await db.WebhookDeliveries
            .Include(d => d.WebhookSubscription)
            .Where(d =>
                d.Status == "Pending" &&
                d.AttemptCount < MaxAttempts)
            .OrderBy(d => d.CreatedUtc)
            .Take(50)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Processing {Count} pending webhook deliveries.", pending.Count);

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        foreach (var delivery in pending)
        {
            var subscription = delivery.WebhookSubscription;

            if (!subscription.IsActive)
            {
                delivery.Status = "Skipped";
                delivery.LastAttemptUtc = DateTime.UtcNow;
                delivery.LastError = "Subscription is inactive.";
                continue;
            }

            delivery.AttemptCount++;
            delivery.LastAttemptUtc = DateTime.UtcNow;

            try
            {
                var payloadBytes = Encoding.UTF8.GetBytes(delivery.PayloadJson);
                var signature = ComputeHmacSignature(payloadBytes, subscription.Secret);

                using var request = new HttpRequestMessage(HttpMethod.Post, subscription.EndpointUrl);
                request.Content = new ByteArrayContent(payloadBytes);
                request.Content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                request.Headers.Add("X-Aethon-Event", delivery.EventType);
                request.Headers.Add("X-Aethon-Delivery", delivery.Id.ToString());
                request.Headers.Add("X-Aethon-Signature-256", $"sha256={signature}");

                var response = await client.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                {
                    delivery.Status = "Delivered";
                    delivery.LastError = null;

                    _logger.LogInformation(
                        "Webhook delivery {DeliveryId} succeeded (attempt {Attempt}).",
                        delivery.Id, delivery.AttemptCount);
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    delivery.LastError = $"HTTP {(int)response.StatusCode}: {Truncate(body, 500)}";

                    if (delivery.AttemptCount >= MaxAttempts)
                    {
                        delivery.Status = "Failed";
                        _logger.LogWarning(
                            "Webhook delivery {DeliveryId} permanently failed after {MaxAttempts} attempts.",
                            delivery.Id, MaxAttempts);
                    }
                    else
                    {
                        delivery.Status = "Pending";
                        _logger.LogWarning(
                            "Webhook delivery {DeliveryId} failed (attempt {Attempt}): {Error}",
                            delivery.Id, delivery.AttemptCount, delivery.LastError);
                    }
                }
            }
            catch (Exception ex)
            {
                delivery.LastError = Truncate(ex.Message, 500);

                if (delivery.AttemptCount >= MaxAttempts)
                {
                    delivery.Status = "Failed";
                    _logger.LogWarning(
                        "Webhook delivery {DeliveryId} permanently failed after {MaxAttempts} attempts: {Error}",
                        delivery.Id, MaxAttempts, ex.Message);
                }
                else
                {
                    delivery.Status = "Pending";
                    _logger.LogWarning(
                        "Webhook delivery {DeliveryId} error (attempt {Attempt}): {Error}",
                        delivery.Id, delivery.AttemptCount, ex.Message);
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static string ComputeHmacSignature(byte[] payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(keyBytes, payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength), "…");
    }
}
