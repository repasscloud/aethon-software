# Stripe Job Publish Checkout — Implementation Plan

_Generated: 2026-03-25_

---

## Problem Summary

1. **Add-on pills missing** — `/app/jobs/new` shows no "Add-on +A$9" pricing next to
   highlight colour or video fields for Standard listings. The cost summary at the bottom
   does not include add-on charges.

2. **No Stripe redirect** — when a Standard job with chargeable add-ons is published, the
   app tries an off-session PaymentIntent (requires a saved card that may not exist yet).
   New customers hit a 402 with no path to enter a card. The correct flow is a Stripe
   Checkout hosted page so they can enter card details, use discount codes, and have their
   card saved for future off-session charges.

3. **Job status wrong** — job should move to `OnHold` while awaiting Stripe payment, not
   remain `Draft`. The webhook on `checkout.session.completed` publishes it.

4. **PO number not forwarded** — the PO/reference number entered on the job form is not
   attached to the Stripe payment, making it hard for clients to reconcile invoices.

5. **Billing portal not surfaced** — clients need a way to reach the Stripe-hosted portal
   to review invoices, update cards, and download receipts.

---

## Confirmed Architecture Decisions

| # | Decision |
|---|---|
| 1 | Use **Stripe Checkout** (hosted page) for all ad-hoc job publish charges — not off-session PaymentIntents. This captures the card for first-time customers. |
| 2 | `setup_future_usage: "off_session"` saves the card so future recurring charges (sticky top-up, bulk credits) can be done silently without a redirect. |
| 3 | `AllowPromotionCodes: true` on every checkout — discount codes are managed in Stripe, no platform code needed. |
| 4 | Job status = `OnHold` while Stripe payment is pending. Webhook `checkout.session.completed` moves it to `Published`. |
| 5 | Posting credits (free promo or purchased): consumed **at checkout creation** before opening Stripe. If the customer abandons checkout the credit is spent (acceptable trade-off for MVP). |
| 6 | PO number stored in Stripe `PaymentIntentData.Description` and `Metadata.po_number`. |
| 7 | Base post price is included as a line item **only when no credit is available**. |
| 8 | All add-ons (highlight A$9, video A$9, AI matching A$9) are chargeable per-item for Standard tier. Premium includes them free — no line item added. |

---

## Flow Diagrams

### Happy path — has credits, no add-ons (unchanged)
```
User publishes Standard job (no add-ons, has Standard credit)
  → POST /api/v1/billing/job-publish-checkout
  → backend consumes credit, sets job = Published
  → returns { published: true }
  → frontend navigates to /app/jobs/{jobId}
```

### New path — add-ons required (Stripe Checkout)
```
User publishes Standard job with highlight + video selected
  → POST /api/v1/jobs  (creates job as Draft, add-ons stripped from payload)
  → POST /api/v1/billing/job-publish-checkout
      ├─ Has Standard credit?  YES → consume it (no base-post line item)
      │                        NO  → add Standard 1x price as line item
      ├─ Highlight selected    → add Stripe.Price.Addon.Highlight line item
      ├─ Video selected        → add Stripe.Price.Addon.Video line item
      ├─ AI matching selected  → add Stripe.Price.Addon.AiMatching line item
      ├─ Sticky selected       → check sticky credit; if none, add sticky price line item
      ├─ Store pending add-on flags in Stripe metadata
      ├─ Set job.Status = OnHold
      └─ Create Stripe Checkout Session → return { checkoutUrl }
  → frontend: Nav.NavigateTo(checkoutUrl, forceLoad: true)
  → User completes Stripe Checkout
  → Stripe fires checkout.session.completed
  → StripeWebhookProcessor handles purchase_type = "job_addons"
      ├─ Apply highlight / video / AI matching / sticky to job
      └─ Set job.Status = Published
  → User lands on SuccessUrl: /app/jobs/{jobId}/checkout-success
```

---

## Files to Create

| File | Purpose |
|---|---|
| `src/Aethon.Shared/Billing/JobPublishCheckoutRequestDto.cs` | Request DTO for the new endpoint |
| `src/Aethon.Shared/Billing/JobPublishCheckoutResponseDto.cs` | Response DTO (published bool + optional checkoutUrl) |
| `src/Aethon.Web/Components/Pages/JobCheckoutSuccess.razor` | Post-checkout success page at `/app/jobs/{jobId}/checkout-success` |

---

## Files to Modify

| File | Change |
|---|---|
| `src/Aethon.Api/Infrastructure/Stripe/StripeCheckoutService.cs` | Add `CreateJobPublishCheckoutAsync()` |
| `src/Aethon.Api/Infrastructure/Stripe/StripeWebhookProcessor.cs` | Add `HandleJobAddonsAsync()` case |
| `src/Aethon.Api/Endpoints/Billing/BillingEndpoints.cs` | Add `POST /billing/job-publish-checkout` |
| `src/Aethon.Web/Components/Pages/NewJob.razor` | Add-on pill UI + updated publish flow |

---

## Detailed Spec

### 1. JobPublishCheckoutRequestDto

```csharp
public sealed class JobPublishCheckoutRequestDto
{
    public Guid JobId { get; set; }

    // Standard add-ons — ignored for Premium jobs
    public bool AddHighlight { get; set; }
    public string? HighlightColour { get; set; }   // hex e.g. "#FFF9C4"
    public bool AddVideo { get; set; }
    public string? VideoYouTubeId { get; set; }
    public string? VideoVimeoId { get; set; }
    public bool AddAiMatching { get; set; }

    // Sticky (both tiers)
    public int StickyDuration { get; set; }        // 0 = none, 1, 7, or 30
}
```

### 2. JobPublishCheckoutResponseDto

```csharp
public sealed class JobPublishCheckoutResponseDto
{
    /// <summary>True when the job was published immediately (no payment required).</summary>
    public bool Published { get; set; }

    /// <summary>Stripe Checkout URL to redirect the user to; null when Published = true.</summary>
    public string? CheckoutUrl { get; set; }
}
```

### 3. POST /api/v1/billing/job-publish-checkout

Logic:
```
1.  Load org for current user
2.  Load job by request.JobId; verify user can publish it (owns org)
3.  Validate job is Draft or Approved (otherwise 400)
4.  Determine PostingTier from job
5.  isPremium = job.PostingTier == Premium
6.  isVerified = org.VerificationTier != None

7.  Build line items list = []
8.  Build add-on metadata dict = {}

9.  --- BASE POSTING CREDIT ---
    postingCreditType = isPremium ? Premium : Standard
    postingCredit = find oldest non-expired credit with QuantityRemaining > 0
    if postingCredit != null:
        Consume credit (decrement QuantityRemaining, log CreditConsumptionLog)
        add-on metadata["posting_credit_consumed"] = "true"
        add-on metadata["posting_credit_id"] = postingCredit.Id.ToString()
    else:
        priceKey = isPremium ? StripePriceJobPremium1x : StripePriceJobStandard1x
        priceId = await _settings.GetStringAsync(priceKey)
        add lineItem { Price = priceId, Quantity = 1 }
        add-on metadata["posting_credit_consumed"] = "false"

10. --- STANDARD ADD-ONS (skip if Premium) ---
    if !isPremium:
        if AddHighlight && HighlightColour set:
            priceId = await _settings.GetStringAsync(StripePriceAddonHighlight)
            add lineItem for highlight
            metadata["add_highlight"] = "true"
            metadata["highlight_colour"] = HighlightColour
        if AddVideo && (YouTubeId or VimeoId set):
            priceId = await _settings.GetStringAsync(StripePriceAddonVideo)
            add lineItem for video
            metadata["add_video"] = "true"
            metadata["video_youtube_id"] = YouTubeId ?? ""
            metadata["video_vimeo_id"] = VimeoId ?? ""
        if AddAiMatching:
            priceId = await _settings.GetStringAsync(StripePriceAddonAiMatching)
            add lineItem for AI matching
            metadata["add_ai_matching"] = "true"

11. --- STICKY ---
    if StickyDuration > 0:
        stickyType = resolve from duration
        stickyCredit = find sticky credit
        if stickyCredit != null:
            Consume sticky credit
            metadata["sticky_consumed_credit"] = "true"
            metadata["sticky_duration"] = StickyDuration.ToString()
        else:
            priceKey = resolve sticky price key (verified vs unverified)
            priceId = await _settings.GetStringAsync(priceKey)
            add lineItem for sticky
            metadata["sticky_duration"] = StickyDuration.ToString()

12. --- EARLY EXIT: nothing to charge ---
    if lineItems.Count == 0:
        Apply add-ons directly to job (highlight, video, AI, sticky from credits)
        Set job.Status = Published; job.PublishedUtc = now
        Save → return { Published = true }

13. --- CREATE CHECKOUT ---
    if any priceId is null/empty → return 400 "product not configured"
    customerId = await EnsureStripeCustomerAsync(org)
    metadata["organisation_id"] = org.Id
    metadata["purchase_type"]   = "job_addons"
    metadata["job_id"]          = job.Id
    if !string.IsNullOrEmpty(job.PoNumber):
        metadata["po_number"] = job.PoNumber
    Set job.Status = OnHold
    Save
    Create SessionCreateOptions:
        Customer = customerId
        Mode = "payment"
        LineItems = lineItems
        Metadata = metadata
        PaymentIntentData.SetupFutureUsage = "off_session"
        PaymentIntentData.Description = po_number ? $"Job: {job.Title} | PO: {job.PoNumber}" : $"Job: {job.Title}"
        AllowPromotionCodes = true
        SuccessUrl = {webBase}/app/jobs/{job.Id}/checkout-success?session_id={CHECKOUT_SESSION_ID}
        CancelUrl  = {webBase}/app/jobs/{job.Id}
    return { Published = false, CheckoutUrl = session.Url }
```

### 4. StripeWebhookProcessor — HandleJobAddonsAsync

```
purchase_type = "job_addons"
→ Read job_id from metadata
→ Load job
→ Read flags from metadata:
    add_highlight, highlight_colour
    add_video, video_youtube_id, video_vimeo_id
    add_ai_matching
    sticky_duration
    posting_credit_consumed (already handled at checkout creation)
→ Apply to job:
    if add_highlight: job.IsHighlighted = true; job.HighlightColour = highlight_colour
    if add_video: job.VideoYouTubeId = ...; job.VideoVimeoId = ...
    if add_ai_matching: job.HasAiCandidateMatching = true
    if sticky_duration > 0: job.StickyUntilUtc = now + days
→ job.Status = Published; job.PublishedUtc ??= now
→ dbEvent.Status = Completed
→ Save
```

### 5. NewJob.razor UI Changes

**Add-on pill** (Standard tier only): Shown inline next to each add-on option:
```
Highlight colour    [colour swatches]   <span class="badge bg-warning text-dark">Add-on +A$9</span>
YouTube video ID    [input]             <span class="badge bg-warning text-dark">Add-on +A$9</span>
Vimeo video ID      [input]             (same badge, shown when YouTube not filled)
AI matching toggle  [existing badge already shows "Add-on"]   — update badge to show "+A$9"
```

**Cost summary** (green/warning box at bottom, Standard + Published):
- Currently shows: "1× Standard posting credit will be consumed" or "No credits…"
- Adds per-selected add-on:
  - `+ A$9 highlight colour`
  - `+ A$9 video embed`
  - `+ A$9 AI candidate matching`
  - `+ A$[n] sticky [duration]`

**Publish flow** (replaces old two-step create → publish → addons):
```
1. Strip Standard add-on fields from create payload (same as previous fix)
2. POST /api/v1/jobs → jobId
3. POST /api/v1/billing/job-publish-checkout { jobId, add-ons... }
4. if response.Published → navigate to /app/jobs/{jobId}
5. if response.CheckoutUrl → Nav.NavigateTo(checkoutUrl, forceLoad: true)
6. if error 402/400 → show _pageError
```

### 6. JobCheckoutSuccess.razor

Page at `/app/jobs/{jobId}/checkout-success`:
- Show spinner + "Confirming your payment…"
- Poll `GET /api/v1/jobs/{jobId}` every 2s for up to 20s waiting for Status = Published
- Once Published: show success card "Your job is now live!" with link to job
- If timeout: show "Payment received. Your job will go live shortly." with dashboard link

---

## Stripe Metadata Reference

All metadata keys for `purchase_type = "job_addons"`:

| Key | Example | Notes |
|---|---|---|
| `organisation_id` | `"abc-def-..."` | Always present |
| `purchase_type` | `"job_addons"` | Always present |
| `job_id` | `"abc-def-..."` | Always present |
| `po_number` | `"PO-2024-001"` | Only if set on job |
| `posting_credit_consumed` | `"true"` | Whether a credit was consumed at checkout time |
| `posting_credit_id` | `"abc-def-..."` | ID of the consumed credit row |
| `add_highlight` | `"true"` | Whether highlight line item is in this checkout |
| `highlight_colour` | `"#FFF9C4"` | Hex colour string |
| `add_video` | `"true"` | Whether video line item is in this checkout |
| `video_youtube_id` | `"dQw4w9WgXcQ"` | May be empty if Vimeo used |
| `video_vimeo_id` | `"123456789"` | May be empty if YouTube used |
| `add_ai_matching` | `"true"` | Whether AI matching line item is in this checkout |
| `sticky_duration` | `"7"` | Days; `"0"` = no sticky in this checkout |
| `sticky_consumed_credit` | `"true"` | Whether sticky was paid by credit, not line item |

---

## Out of Scope (this iteration)

- Credit reservation/rollback on checkout abandonment — if user abandons checkout a consumed promo credit is lost. Acceptable for MVP; can add a cancel webhook handler (`checkout.session.expired`) later.
- Webhooks for `checkout.session.expired` to restore credits or notify admins.
- Recurring sticky auto-renewal.
