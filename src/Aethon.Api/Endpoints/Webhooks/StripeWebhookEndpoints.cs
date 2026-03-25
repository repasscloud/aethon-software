using Aethon.Api.Infrastructure.Stripe;
using Aethon.Application.Abstractions.Logging;
using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Aethon.Api.Endpoints.Webhooks;

public static class StripeWebhookEndpoints
{
    public static void MapStripeWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/webhooks")
            .WithTags("Webhooks");

        // POST /api/v1/webhooks/stripe
        // Receives, verifies, deduplicates and processes Stripe webhook events.
        group.MapPost("/stripe", async (
            HttpRequest request,
            IConfiguration configuration,
            ISystemSettingsService settings,
            ISystemLogService systemLog,
            AethonDbContext db,
            StripeWebhookProcessor processor,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("StripeWebhook");

            // Read raw body — must be done before any body parsing middleware
            string payload;
            using (var reader = new StreamReader(request.Body))
                payload = await reader.ReadToEndAsync(ct);

            if (string.IsNullOrWhiteSpace(payload))
                return Results.BadRequest(new { code = "webhook.empty_payload", message = "Empty payload." });

            // ── Signature verification ──────────────────────────────────────
            // Webhook secret is stored in SystemSettings (managed via admin UI).
            // Falls back to appsettings if not yet configured in the DB.
            var webhookSecret = await settings.GetStringAsync(SystemSettingKeys.StripeWebhookSecret, ct)
                ?? configuration["Stripe:WebhookSecret"];

            Event stripeEvent;

            if (!string.IsNullOrEmpty(webhookSecret))
            {
                var signature = request.Headers["Stripe-Signature"].ToString();

                try
                {
                    stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret,
                        throwOnApiVersionMismatch: false);
                }
                catch (StripeException ex)
                {
                    logger.LogWarning("Stripe webhook signature validation failed: {Message}", ex.Message);
                    return Results.BadRequest(new { code = "webhook.invalid_signature", message = "Signature validation failed." });
                }
            }
            else
            {
                // No webhook secret configured — parse without verification.
                logger.LogError("Stripe webhook secret not configured in DB or environment — processing without signature verification. Set Stripe.WebhookSecret in Admin → Stripe Products.");
                await systemLog.LogAsync(
                    SystemLogLevel.Error,
                    "StripeWebhook",
                    "Webhook secret not configured — signature verification skipped.",
                    "Set Stripe.WebhookSecret via Admin → Stripe Products or the Stripe__WebhookSecret environment variable.",
                    requestPath: request.Path,
                    ct: ct);
                try
                {
                    stripeEvent = EventUtility.ParseEvent(payload, throwOnApiVersionMismatch: false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to parse Stripe event payload.");
                    return Results.BadRequest(new { code = "webhook.parse_failed", message = "Could not parse event payload." });
                }
            }

            // ── Deduplication ───────────────────────────────────────────────
            if (await db.StripePaymentEvents.AnyAsync(e => e.StripeEventId == stripeEvent.Id, ct))
                return Results.Ok(new { received = true, duplicate = true });

            // ── Persist the raw event ───────────────────────────────────────
            long? amountTotal = null;
            string? currency = null;
            string? customerEmail = null;

            try
            {
                if (stripeEvent.Data.Object is Stripe.Checkout.Session session)
                {
                    amountTotal = session.AmountTotal;
                    currency = session.Currency;
                    customerEmail = session.CustomerEmail
                        ?? session.CustomerDetails?.Email;
                }
            }
            catch
            {
                // Non-fatal — raw payload is always stored
            }

            var dbEvent = new StripePaymentEvent
            {
                Id = Guid.NewGuid(),
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                AmountTotal = amountTotal,
                Currency = currency,
                CustomerEmail = customerEmail,
                PayloadJson = payload,
                Status = StripeEventStatus.Pending,
                CreatedUtc = DateTime.UtcNow
            };

            db.StripePaymentEvents.Add(dbEvent);
            await db.SaveChangesAsync(ct);

            // ── Process the event ───────────────────────────────────────────
            try
            {
                await processor.ProcessAsync(stripeEvent, dbEvent, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Stripe event {EventId} ({EventType})", stripeEvent.Id, stripeEvent.Type);
                dbEvent.Status = StripeEventStatus.Failed;
                dbEvent.InternalNotes = $"Processing exception: {ex.Message}";
                await db.SaveChangesAsync(ct);
            }

            return Results.Ok(new { received = true });
        });
    }
}
