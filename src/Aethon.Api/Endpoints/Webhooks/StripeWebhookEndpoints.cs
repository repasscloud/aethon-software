using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Webhooks;

public static class StripeWebhookEndpoints
{
    public static void MapStripeWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/webhooks")
            .WithTags("Webhooks");

        // POST /api/v1/webhooks/stripe
        // Receives Stripe webhook events and stores them for manual review.
        group.MapPost("/stripe", async (
            HttpContext http,
            AethonDbContext db,
            CancellationToken ct) =>
        {
            string payload;
            using (var reader = new StreamReader(http.Request.Body))
                payload = await reader.ReadToEndAsync(ct);

            if (string.IsNullOrWhiteSpace(payload))
                return Results.BadRequest(new { code = "webhook.empty_payload", message = "Empty payload." });

            // Extract basic fields from the payload for quick review; full payload is stored.
            string? eventId = null;
            string? eventType = null;
            long? amountTotal = null;
            string? currency = null;
            string? customerEmail = null;

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(payload);
                var root = doc.RootElement;
                eventId = root.TryGetProperty("id", out var id) ? id.GetString() : null;
                eventType = root.TryGetProperty("type", out var t) ? t.GetString() : null;

                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("object", out var obj))
                {
                    if (obj.TryGetProperty("amount_total", out var amt))
                        amountTotal = amt.TryGetInt64(out var a) ? a : null;
                    if (obj.TryGetProperty("currency", out var cur))
                        currency = cur.GetString();
                    if (obj.TryGetProperty("customer_email", out var email))
                        customerEmail = email.GetString();
                    // Also check customer_details.email
                    if (customerEmail == null &&
                        obj.TryGetProperty("customer_details", out var details) &&
                        details.TryGetProperty("email", out var detailEmail))
                        customerEmail = detailEmail.GetString();
                }
            }
            catch
            {
                // If parsing fails, still store the raw payload for manual review
            }

            // Deduplicate by StripeEventId
            if (!string.IsNullOrEmpty(eventId) &&
                await db.StripePaymentEvents.AnyAsync(e => e.StripeEventId == eventId, ct))
            {
                return Results.Ok(new { received = true, duplicate = true });
            }

            var stripeEvent = new StripePaymentEvent
            {
                Id = Guid.NewGuid(),
                StripeEventId = eventId ?? $"unknown-{Guid.NewGuid()}",
                EventType = eventType ?? "unknown",
                AmountTotal = amountTotal,
                Currency = currency,
                CustomerEmail = customerEmail,
                PayloadJson = payload,
                Status = StripeEventStatus.Pending,
                CreatedUtc = DateTime.UtcNow
            };

            db.StripePaymentEvents.Add(stripeEvent);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { received = true });
        });
    }
}
