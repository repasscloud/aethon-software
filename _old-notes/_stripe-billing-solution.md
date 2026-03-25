# Stripe & Billing Implementation Plan
## Aethon ATS — Full Commercial Model

_Reference documents: `ats-job-posting-pricing-and-launch-strategy.md`, `_implementation-plan-stripe.txt`_
_Confirmed decisions: Option A (card on file), one-time verification payments (not subscription)_

## Confirmed Answers to Open Questions

| # | Question | Answer |
|---|---|---|
| Q1 | Add-on charges with no credits | **Option A** — one combined PaymentIntent at publish time covering base post + all selected add-ons |
| Q2 | Verification bundles (A$68 / A$208) | **Separate Stripe products** — created manually in Stripe, Price IDs entered in admin UI (`/admin/stripe-products`). No product IDs needed before dev starts; sandbox IDs used during development. |
| Q3 | Add-ons on existing published jobs | **Chargeable on update** — adding a video ID (or other Standard add-on) to a published job and clicking "Update post" triggers a charge at that point |
| Q4 | Job post default expiry | **30 days Standard, 60 days Premium**. Calendar UI allows a shorter date but enforces the maximum. Server-side validation also enforces the cap. Date field is a date picker — no free-text entry. |
| Q5 | Add `Failed` to `StripeEventStatus` | **Yes** — `Failed = 3` added |

---

## 1. Current State Assessment

### What already exists

| Item | Status |
|---|---|
| `StripePaymentEvent` entity | Exists — stores raw webhook payloads |
| `StripeWebhookEndpoints.cs` | Exists — stores events but does NO processing |
| `StripeEventStatus` enum (Pending/Reviewed/Completed) | Exists |
| `SystemSetting` entity (key/value config) | Exists — will be used for Stripe product IDs |
| `Organisation.VerificationTier` | Exists |
| `Organisation.VerifiedUtc` / `VerifiedByUserId` | Exists |
| `Job.IsHighlighted`, `StickyUntilUtc`, `VideoYouTubeId/VimeoId`, `AllowAutoMatch` | Exists |
| `JobStatus` (Draft → PendingCompanyApproval → Approved → Published → Closed) | Exists — matches required workflow |
| `Stripe.net` NuGet package | **NOT installed** |

### What is missing

- `Organisation.StripeCustomerId` — needed for card-on-file
- `Organisation.VerificationExpiresAt` — needed for renewal tracking
- `Job.PostingTier` — Standard or Premium
- `Job.HighlightColour` — hex colour for highlighted listings
- Credit system (entities)
- Stripe product ID management in admin UI
- Verification checkout endpoint
- Job credit purchase checkout endpoint
- Billing portal endpoint
- Webhook signature verification + event routing
- Admin page `/admin/stripe-events` (UI only — data exists)
- Org billing page `/app/organisation/billing`
- Job create page tier + add-on selector with live cost button

---

## 2. Architecture Decisions

### 2.1 Verification — One-Time Payment (not subscription)

Verification is a one-time `payment` mode checkout. Expiry is tracked via `Organisation.VerificationExpiresAt`. Renewal reminders and expiry enforcement are handled via cron jobs (future work). This avoids Stripe managing the subscription lifecycle and keeps full control in the platform.

### 2.2 Card on File

First payment (verification or credit purchase) creates a Stripe `Customer` and stores the `StripeCustomerId` on the org. The checkout uses `setup_future_usage: "off_session"` so the card is saved. Future charges (job posting, sticky) can be made silently against the saved card without a Stripe redirect.

For job credits specifically: if the org has sufficient posting credits, no Stripe call is needed at all. Credits are consumed on publication.

### 2.3 Stripe Product IDs in Admin

Product IDs and Price IDs are stored in the `SystemSettings` table (already exists). An admin page at `/admin/stripe-products` reads and writes these keys. This means you never need a deployment to update a price — just change it in the admin UI.

### 2.4 Credit System

Credits are stored as individual ledger rows in a new `OrganisationJobCredit` table. Each row represents a "batch" of credits with its own source, type, quantity, and optional expiry date. This supports the audit requirements and the admin ability to grant credits manually. Credit consumption is tracked in a separate `CreditConsumptionLog` table.

### 2.5 Sticky Top — Credits Only, No Auto-Charge

Sticky Top is never included in posting credits. It is separately purchasable via Stripe checkout OR grantable by admin. Once an org has sticky credits, they can apply them during job creation without going back to Stripe. The sticky credit types are `StickyTop24h`, `StickyTop7d`, `StickyTop30d`.

### 2.6 Discount Codes

Managed entirely in Stripe (coupon "once per customer" restriction). The platform's only responsibility is to always pass the `StripeCustomerId` when a returning customer checks out, so Stripe correctly enforces "already used" state. No platform-side discount tracking is needed.

---

## 3. Database Changes

### 3.1 New Columns on `Organisations`

```csharp
public string? StripeCustomerId { get; set; }          // Stripe Customer ID (created on first payment)
public DateTime? VerificationExpiresAt { get; set; }    // When current verification expires (1 year from payment)
public DateTime? VerificationPaidAt { get; set; }       // When the verification payment was received
public string? VerificationStripeEventId { get; set; }  // FK to StripePaymentEvent.StripeEventId for audit
```

> **Note:** `VerifiedUtc` already exists and records when admin set verification. `VerificationPaidAt` separately records when payment was received — useful when auto-verify runs and both happen simultaneously, vs manual review where there's a delay.

### 3.2 New Columns on `Jobs`

```csharp
public JobPostingTier PostingTier { get; set; } = JobPostingTier.Standard;
public string? HighlightColour { get; set; }            // Hex e.g. "#FFD700", null = none
public bool HasAiCandidateMatching { get; set; }        // True if purchased or Premium
// VideoYouTubeId / VideoVimeoId already exist
// IsHighlighted already exists
// StickyUntilUtc already exists
```

### 3.3 New Enum: `JobPostingTier`

```csharp
// Aethon.Shared/Enums/JobPostingTier.cs
public enum JobPostingTier
{
    Standard = 1,
    Premium = 2
}
```

### 3.4 New Enum: `CreditType`

```csharp
// Aethon.Shared/Enums/CreditType.cs
public enum CreditType
{
    JobPostingStandard = 1,
    JobPostingPremium = 2,
    StickyTop24h = 3,
    StickyTop7d = 4,
    StickyTop30d = 5
}
```

### 3.5 New Enum: `CreditSource`

```csharp
// Aethon.Shared/Enums/CreditSource.cs
public enum CreditSource
{
    LaunchPromotion = 1,
    StripePurchase = 2,
    AdminGrant = 3,
    ConvertedFromStandard = 4    // When unused Standard promo credits → Premium on verification
}
```

### 3.6 New Entity: `OrganisationJobCredit`

```csharp
// Aethon.Data/Entities/OrganisationJobCredit.cs
public class OrganisationJobCredit : EntityBase
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public CreditType CreditType { get; set; }
    public CreditSource Source { get; set; }

    public int QuantityOriginal { get; set; }
    public int QuantityRemaining { get; set; }

    public DateTime? ExpiresAt { get; set; }        // Null = no expiry (admin grants)
    public DateTime? ConvertedAt { get; set; }      // Set when Standard promo → Premium

    // For Stripe purchases
    public Guid? StripePaymentEventId { get; set; }
    public StripePaymentEvent? StripePaymentEvent { get; set; }

    // For admin grants
    public Guid? GrantedByUserId { get; set; }
    public string? GrantNote { get; set; }

    public ICollection<CreditConsumptionLog> ConsumptionLogs { get; set; } = [];
}
```

### 3.7 New Entity: `CreditConsumptionLog`

```csharp
// Aethon.Data/Entities/CreditConsumptionLog.cs
public class CreditConsumptionLog : EntityBase
{
    public Guid OrganisationJobCreditId { get; set; }
    public OrganisationJobCredit Credit { get; set; } = null!;

    public Guid OrganisationId { get; set; }
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    public Guid ConsumedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }     // For recruiter-managed jobs approved by company

    public int QuantityConsumed { get; set; } = 1;
    public DateTime ConsumedAt { get; set; }
}
```

### 3.8 Changes to `StripePaymentEvent`

Add fields to link back to the organisation and describe what was purchased:

```csharp
public Guid? OrganisationId { get; set; }           // Populated from metadata.organisation_id
public Organisation? Organisation { get; set; }

public string? PurchaseType { get; set; }           // "verification", "job_credits", "sticky"
public string? ProductId { get; set; }              // Stripe product ID from metadata
public string? PriceId { get; set; }                // Stripe price ID from metadata
public string? PurchaseMetaJson { get; set; }       // Full metadata JSON for reference
```

> The existing `StripePaymentEvent` has `StripeEventId`, `EventType`, `AmountTotal`, `Currency`, `CustomerEmail`, `PayloadJson`, `Status`, `InternalNotes`, `CompletedByUserId`, `CompletedUtc`. These remain unchanged.

### 3.9 Migration

One migration covers all of the above:
```
dotnet ef migrations add AddStripeBillingAndCreditSystem \
  --project src/Aethon.Data \
  --startup-project src/Aethon.Api
```

---

## 4. Stripe Product ID Management (Admin UI)

### 4.1 SystemSettings Keys

The following keys are stored in `SystemSettings` and managed via an admin page. Prefix `stripe:` to namespace them.

| Key | Description | Example value |
|---|---|---|
| `stripe:price:verification_standard` | Standard Employer Verification — Stripe Price ID | `price_xxx` |
| `stripe:price:verification_enhanced` | Enhanced Trusted Employer — Stripe Price ID | `price_xxx` |
| `stripe:price:job_standard_bundle_1x` | 1x Standard Job Post credit | `price_xxx` |
| `stripe:price:job_standard_bundle_5x` | 5x Standard Job Post credits | `price_xxx` |
| `stripe:price:job_standard_bundle_10x` | 10x Standard Job Post credits | `price_xxx` |
| `stripe:price:job_standard_bundle_20x` | 20x Standard Job Post credits | `price_xxx` |
| `stripe:price:job_premium_bundle_1x` | 1x Premium Job Post credit | `price_xxx` |
| `stripe:price:job_premium_bundle_5x` | 5x Premium Job Post credits | `price_xxx` |
| `stripe:price:job_premium_bundle_10x` | 10x Premium Job Post credits | `price_xxx` |
| `stripe:price:job_premium_bundle_20x` | 20x Premium Job Post credits | `price_xxx` |
| `stripe:price:sticky_24h_verified` | Sticky 24h — verified org price | `price_xxx` |
| `stripe:price:sticky_7d_verified` | Sticky 7d — verified org price | `price_xxx` |
| `stripe:price:sticky_30d_verified` | Sticky 30d — verified org price | `price_xxx` |
| `stripe:price:sticky_24h_unverified` | Sticky 24h — unverified org price | `price_xxx` |
| `stripe:price:sticky_7d_unverified` | Sticky 7d — unverified org price | `price_xxx` |
| `stripe:price:sticky_30d_unverified` | Sticky 30d — unverified org price | `price_xxx` |
| `stripe:price:addon_highlight` | Standard add-on: highlight colour | `price_xxx` |
| `stripe:price:addon_video` | Standard add-on: video embed | `price_xxx` |
| `stripe:price:addon_ai_matching` | Standard add-on: AI candidate matching | `price_xxx` |
| `stripe:price:bundle_standard_verification_post` | Bundle: Standard Verification + First Standard Post (A$68) | `price_xxx` |
| `stripe:price:bundle_enhanced_verification_post` | Bundle: Enhanced Verification + First Premium Post (A$208) | `price_xxx` |
| `stripe:webhook_secret` | Webhook signing secret (sensitive — mask in UI) | `whsec_xxx` |
| `stripe:publishable_key` | Publishable key (shown in frontend if needed) | `pk_live_xxx` |

> **Secret key** (`sk_live_xxx`) should NEVER go in `SystemSettings`. It should be in `appsettings.json` / environment variable / secrets manager only, and loaded via `IConfiguration`.

### 4.2 Admin Page: `/admin/stripe-products`

A Blazor page that:
- Loads all `stripe:` prefixed settings from the DB
- Displays them in a table grouped by category (Verification, Job Credits, Sticky, Add-ons, Config)
- Allows inline edit of each value
- Masks `stripe:webhook_secret` (show as `••••••••` with a reveal button)
- Has a single Save button that writes only changed values
- Shows when each setting was last updated and by whom

---

## 5. Checkout Flow — Verification

### 5.1 Endpoint

```
POST /api/v1/billing/verification/checkout
Body: { "tier": "standard" | "enhanced" }
Response: { "checkoutUrl": "https://checkout.stripe.com/..." }
```

Requires auth. Org user only.

### 5.2 Logic

```
1. Load org for current user
2. If org is already at or above the requested tier → return 409 Conflict
3. Determine Stripe Price ID from SystemSetting:
   - "standard"  → stripe:price:verification_standard
   - "enhanced"  → stripe:price:verification_enhanced
4. Create or retrieve Stripe Customer:
   - If org.StripeCustomerId is null → StripeClient.Customers.Create(email, name, metadata: {organisation_id})
   - Save StripeCustomerId to org
5. Create Stripe Checkout Session:
   - Mode: "payment"
   - Customer: org.StripeCustomerId
   - LineItems: [{ Price: priceId, Quantity: 1 }]
   - Metadata: {
       organisation_id: org.Id,
       purchase_type: "verification",
       verification_tier: "standard" | "enhanced"
     }
   - setup_future_usage: "off_session"   ← saves card for future charges
   - AllowPromotionCodes: true           ← allows discount code entry at checkout
   - SuccessUrl: /app/verification/success?session_id={CHECKOUT_SESSION_ID}
   - CancelUrl: /app/verification/cancelled
6. Return checkout URL
```

### 5.3 Success Page

`/app/verification/success` — shows "Payment received. We are reviewing your verification." Do NOT immediately mark as verified here (wait for webhook).

---

## 6. Checkout Flow — Job Credits

### 6.1 Endpoint

```
POST /api/v1/billing/credits/checkout
Body: { "priceKey": "stripe:price:job_standard_bundle_5x" }
Response: { "checkoutUrl": "https://checkout.stripe.com/..." }
```

Requires auth. Org user only.

### 6.2 Logic

Same pattern as verification checkout. Metadata:
```json
{
  "organisation_id": "...",
  "purchase_type": "job_credits",
  "price_key": "stripe:price:job_standard_bundle_5x",
  "credit_type": "standard",
  "credit_quantity": "5"
}
```

The `credit_type` and `credit_quantity` are embedded in metadata so the webhook handler knows exactly what to grant without re-reading product metadata from Stripe.

---

## 7. Checkout Flow — Sticky Top

### 7.1 From Admin Grant (no Stripe)

Admin page for an org can add sticky credits directly:
```
POST /api/admin/organisations/{id}/credits/grant
Body: { "creditType": "StickyTop7d", "quantity": 1, "expiresAt": null, "note": "Complimentary" }
```

Creates `OrganisationJobCredit` row with `Source = AdminGrant`.

### 7.2 From User Purchase

```
POST /api/v1/billing/sticky/checkout
Body: { "duration": "24h" | "7d" | "30d" }
```

Same checkout pattern. Metadata:
```json
{
  "organisation_id": "...",
  "purchase_type": "sticky",
  "credit_type": "StickyTop7d",
  "credit_quantity": "1"
}
```

---

## 8. Webhook Upgrade

### 8.1 Install Stripe.net

```
dotnet add src/Aethon.Api/Aethon.Api.csproj package Stripe.net
```

### 8.2 Rewrite `StripeWebhookEndpoints.cs`

The current implementation stores the raw payload. Upgrade it to:

1. Read the raw body (must be done before any middleware touches it — already done)
2. Verify signature: `EventUtility.ConstructEvent(payload, stripeSignatureHeader, webhookSecret)`
3. Deduplicate by `event.Id` (already done)
4. Store the raw event as before
5. Route to a handler based on `event.Type`:

```
checkout.session.completed → HandleCheckoutCompleted(event, stripeEvent)
customer.created           → (no action needed, we create customers ourselves)
```

### 8.3 Handler: `HandleCheckoutCompleted`

```
1. Read session = event.Data.Object as Session
2. Read metadata: organisation_id, purchase_type
3. Update StripePaymentEvent: OrganisationId, PurchaseType, ProductId, PriceId, PurchaseMetaJson

4. switch purchase_type:

  case "verification":
    - Update org: StripeCustomerId (if not set), VerificationPaidAt = now
    - Set org.VerificationExpiresAt = now + 365 days
    - if verification_tier == "standard":
        → RunAutoVerificationAsync(org, stripeEvent)
    - if verification_tier == "enhanced":
        → stripeEvent.Status = Pending   (admin reviews manually)
        → Save

  case "job_credits":
    - Parse credit_type and credit_quantity from metadata
    - Create OrganisationJobCredit row:
        CreditType = parsed, Source = StripePurchase,
        QuantityOriginal = quantity, QuantityRemaining = quantity,
        ExpiresAt = null (purchased credits don't expire unless admin sets it),
        StripePaymentEventId = stripeEvent.Id
    - stripeEvent.Status = Completed
    - Save

  case "sticky":
    - Same as job_credits but for sticky credit type
    - stripeEvent.Status = Completed
    - Save
```

### 8.4 Auto-Verification Logic (`RunAutoVerificationAsync`)

This is the method you mentioned you'll write yourself. The hook:

```csharp
// Called after Standard verification payment confirmed
private async Task RunAutoVerificationAsync(Organisation org, StripePaymentEvent stripeEvent, CancellationToken ct)
{
    bool passed = await _autoVerifier.CheckAsync(org, ct);

    if (passed)
    {
        org.VerificationTier = VerificationTier.StandardEmployer;
        org.VerifiedUtc = DateTime.UtcNow;

        // Convert unused Standard promo credits to Premium
        await ConvertPromoCreditsToPremiumAsync(org.Id, ct);

        stripeEvent.Status = StripeEventStatus.Completed;
        stripeEvent.InternalNotes = "Auto-verified on payment.";
    }
    else
    {
        stripeEvent.Status = StripeEventStatus.Pending;
        stripeEvent.InternalNotes = "Auto-verification failed. Requires manual review.";
    }

    await _db.SaveChangesAsync(ct);
}
```

### 8.5 Launch Promo Credit Conversion

When an org verifies (either auto or manual admin action):

```
1. Find all OrganisationJobCredit rows where:
   - OrganisationId = org.Id
   - CreditType = JobPostingStandard
   - Source = LaunchPromotion
   - QuantityRemaining > 0
   - (ExpiresAt is null OR ExpiresAt > now)
   - ConvertedAt is null

2. For each row:
   - Set CreditType = JobPostingPremium
   - Set ConvertedAt = now
   - Source stays LaunchPromotion (for audit)

3. Save
```

---

## 9. Launch Promotion Credit Grant

When a new organisation is created (company or recruiter), automatically grant 10 Standard credits:

**In the org creation handler** (wherever `Organisation` is first saved):

```csharp
var promoCredit = new OrganisationJobCredit
{
    Id = Guid.NewGuid(),
    OrganisationId = newOrg.Id,
    CreditType = CreditType.JobPostingStandard,
    Source = CreditSource.LaunchPromotion,
    QuantityOriginal = 10,
    QuantityRemaining = 10,
    ExpiresAt = DateTime.UtcNow.AddDays(90),
    CreatedUtc = DateTime.UtcNow
};
_db.OrganisationJobCredits.Add(promoCredit);
```

---

## 10. Job Create Page Changes (`/app/jobs/new`)

### 10.1 Tier Selection

Add a tier picker at the top of the create job form:

- **Standard** (A$29 per credit / uses 1 Standard credit)
  - Platform listing, company logo, analytics
  - Add-ons available: Highlight colour (+A$9), Video embed (+A$9), AI Matching (+A$9)
- **Premium** (A$69 per credit / uses 1 Premium credit)
  - All Standard features + highlight colour + video + AI matching + enhanced visibility

### 10.2 Add-On Checkboxes (Standard only)

If tier = Standard, show:
- `[ ] Add highlight colour (+A$9)` — shows colour picker when checked
- `[ ] Add video embed (+A$9)` — shows YouTube/Vimeo URL field when checked
- `[ ] Enable AI candidate matching (+A$9)`

If tier = Premium, show these as "Included ✓" (no charge).

### 10.3 Post-Publish Add-On Charges

When editing an already-published Standard job:
- If the user adds a video embed URL where previously there was none → charge A$9 on "Update post"
- Same for highlight colour or AI matching if not previously purchased
- The "Update post" button shows the additional charge: `"Update post (+ A$9 video)"`
- A silent PaymentIntent is raised against the saved card; the job is only updated after payment confirmation
- If payment fails, the update is rejected and the user is shown the failure reason
- Premium jobs: these add-ons are already included, no charge on edit

### 10.4 Job Post Expiry Enforcement

- **Standard:** default `PostingExpiresUtc = PublishedUtc + 30 days`, maximum = 30 days from published
- **Premium:** default `PostingExpiresUtc = PublishedUtc + 60 days`, maximum = 60 days from published
- The date picker on the create/edit form:
  - Uses a `<input type="date">` constrained by `max` attribute (computed from today + tier limit)
  - The default pre-filled date is the maximum (full 30 or 60 days)
  - The user can reduce it but cannot exceed it
  - The `max` attribute is set server-side and validated again on submit (cannot be bypassed via DevTools)
- Server-side validation: if `PostingExpiresUtc > PublishedUtc + tierLimitDays` → return 422 validation error

### 10.5 Sticky Top (available to all tiers)

Show a separate "Sticky to Top" section:

- `[ ] Apply Sticky to Top`
  - If checked: show duration options (24h / 7d / 30d)
  - Show price based on org verification status:
    - Verified: A$9 / A$39 / A$79
    - Unverified: A$15 / A$49 / A$99
  - If org has sticky credits of the selected type, show "You have X sticky credit(s) — will be applied at no charge"
  - If no credits, the cost adds to the total

### 10.6 Live Cost Calculation on Submit Button

The "Create Job" / "Publish" button shows the current total cost:

```
Credit situation:
  - Has sufficient credits of required type → button shows "Publish (uses 1 credit)" — no Stripe charge
  - Has insufficient credits → button shows "Publish & Pay A$29" (or A$69) — triggers Stripe

If sticky selected and has sticky credits → sticky portion shows "1 credit"
If sticky selected and no sticky credits → sticky cost added to total

Button text examples:
  "Publish (uses 1 credit)"
  "Publish & Pay A$29"
  "Publish & Pay A$78"  (standard + sticky 7d, unverified, no credits)
  "Publish (uses 1 credit) + Pay A$39"  (has posting credit, no sticky credit)
```

### 10.7 Credit Consumption on Publication

Credit is consumed at `JobStatus.Published`, not at creation. In the publish handler:

```csharp
1. Find oldest non-expired credit of the required type with QuantityRemaining > 0
   (FIFO — consume oldest credits first to naturally expire least-used)
2. Decrement QuantityRemaining by 1
3. Log CreditConsumptionLog row
4. Set job.PostingTier = selected tier
5. Set job.IsHighlighted = (Premium || HighlightColour selected)
6. Set job.HighlightColour = selected colour (if applicable)
7. Set job.HasAiCandidateMatching = (Premium || AI Matching selected)
8. Set job.PublishedUtc = now
9. Set job.Status = Published
```

For sticky (if purchased separately via Stripe, not credits):
- Payment must complete before sticky is applied
- Webhook sets `job.StickyUntilUtc = now + duration`

For sticky credits:
- Applied at publish time, `StickyUntilUtc = now + duration` immediately

---

## 11. Organisation Billing Page (`/app/organisation/billing`)

### 11.1 Sections

**Credit Balance**
Table showing available credits:

| Type | Available | Source | Expires |
|---|---|---|---|
| Standard Job Post | 8 | Launch Promo | 90 days (2026-06-22) |
| Premium Job Post | 0 | — | — |
| Sticky 7-day | 1 | Admin Grant | No expiry |

With a "Buy More Credits" button linking to the credit purchase checkout.

**Buy Credits**
A simple selector showing the bundle options (1x, 5x, 10x, 20x) for Standard and Premium credits, with pricing, and a "Purchase" button per bundle.

**Billing History / Invoices**
"View invoices, manage payment methods and receipts" → button that POSTs to `/api/v1/billing/portal` and redirects to the Stripe Billing Portal URL.

**Verification Status**
Shows current tier, verified date, expiry date. If unverified, show "Verify your organisation" CTA linking to the verification purchase page.

---

## 12. Stripe Billing Portal Endpoint

```
GET /api/v1/billing/portal
Response: { "url": "https://billing.stripe.com/..." }
```

Logic:
1. Load org → get `StripeCustomerId`
2. If null → return 404 (org hasn't made any payments yet)
3. Create `BillingPortal.Session` with `Customer = StripeCustomerId`, `ReturnUrl = /app/organisation/billing`
4. Return the URL

The Stripe Billing Portal handles: view invoices, download receipts, update card, cancel future billing.

---

## 13. Admin Pages

### 13.1 `/admin/stripe-events` (upgrade existing)

The data model already supports this. The page needs to:
- List all `StripePaymentEvent` rows
- Filter by `Status` (Pending / Reviewed / Completed)
- Show: Date, Organisation name (via OrganisationId FK once added), Event Type, Purchase Type, Amount, Status, Notes
- For Pending items: "Approve" button (marks Completed, optionally sets org verified for verification events) and "Reject" button
- For Enhanced Verification pending items: "Approve → Set Enhanced" button that sets `org.VerificationTier = EnhancedTrusted`

### 13.2 `/admin/stripe-products` (new)

See Section 4.2 above.

### 13.3 `/admin/organisations/{id}` (extend)

Add a Credits section:
- Table of `OrganisationJobCredit` rows for this org
- "Grant Credits" button → modal with type, quantity, expiry (optional), note
- "Revoke Credits" on individual rows (set QuantityRemaining to 0)
- Trigger "Convert promo to premium" manually if auto-convert was missed

---

## 14. Implementation Phases

### Phase 1 — Foundation (do first, blocks everything else)

1. Install `Stripe.net` NuGet package
2. Add `StripeSecretKey` to `appsettings.json` (loaded from env/secrets)
3. Add new DB columns to `Organisation` (StripeCustomerId, VerificationExpiresAt, VerificationPaidAt, VerificationStripeEventId)
4. Add new DB columns to `Job` (PostingTier, HighlightColour, HasAiCandidateMatching)
5. Add new DB columns to `StripePaymentEvent` (OrganisationId, PurchaseType, ProductId, PriceId, PurchaseMetaJson)
6. Create new entities: `OrganisationJobCredit`, `CreditConsumptionLog`
7. Create new enums: `JobPostingTier`, `CreditType`, `CreditSource`
8. Run migration: `AddStripeBillingAndCreditSystem`
9. Register `SystemSettings`-based Stripe price ID lookup service

### Phase 2 — Webhook Upgrade

1. Upgrade `StripeWebhookEndpoints.cs` with `Stripe.net` signature verification
2. Implement `checkout.session.completed` routing
3. Implement verification handler (payment recorded, schedule auto-verify hook)
4. Implement job credits handler (grant credits to org)
5. Implement sticky handler (grant sticky credit)

### Phase 3 — Verification Purchase

1. `POST /api/v1/billing/verification/checkout` endpoint
2. Auto-create Stripe Customer on first payment
3. `/app/verification/purchase` Blazor page with Standard / Enhanced options
4. `/app/verification/success` and `/app/verification/cancelled` pages

### Phase 4 — Launch Promo Credit Grant

1. Wire credit grant into new org creation flow
2. Wire credit conversion (Standard promo → Premium) into verification confirmation path

### Phase 5 — Credit Purchase

1. `POST /api/v1/billing/credits/checkout` endpoint
2. Bundle product price configuration in `/admin/stripe-products`
3. `/app/organisation/billing` Blazor page (credits view + buy credits section)
4. `GET /api/v1/billing/portal` endpoint

### Phase 6 — Job Create Page

1. Add `PostingTier` selector to `/app/jobs/new`
2. Add Standard add-on checkboxes (highlight colour, video, AI matching)
3. Add Sticky section with pricing aware of verification status
4. Live cost calculator on submit button
5. Credit-aware publish logic (consume credit on publication)
6. Silent PaymentIntent charge if no credits (uses saved card)

### Phase 7 — Admin Pages

1. `/admin/stripe-products` — SystemSettings editor for Stripe IDs
2. `/admin/stripe-events` — upgrade with org name, approve/reject, enhanced verification approval
3. `/admin/organisations/{id}` — add credits section

---

## 15. Open Questions

All questions resolved. See confirmed answers table at the top of this document.

---

## 16. Pricing Reference (from strategy doc)

| Item | Verified Org | Unverified Org |
|---|---|---|
| Standard Employer Verification | — | A$49 |
| Enhanced Trusted Employer Verification | — | A$149 |
| Standard Job Post (credit or pay) | A$29 | A$29 |
| Premium Job Post (credit or pay) | A$69 | A$69 |
| Highlight Colour add-on (Standard) | A$9 | A$9 |
| Video Embed add-on (Standard) | A$9 | A$9 |
| AI Candidate Matching add-on (Standard) | A$9 | A$9 |
| Sticky 24h | A$9 | A$15 |
| Sticky 7d | A$39 | A$49 |
| Sticky 30d | A$79 | A$99 |
| Launch promo: 10x Standard credits | Free (90-day expiry) | Free (90-day expiry) |
| Promo credit conversion on verify | Unused Standard → Premium | — |

---

## 17. Files That Will Be Created or Modified

| File | Action |
|---|---|
| `src/Aethon.Shared/Enums/JobPostingTier.cs` | Create |
| `src/Aethon.Shared/Enums/CreditType.cs` | Create |
| `src/Aethon.Shared/Enums/CreditSource.cs` | Create |
| `src/Aethon.Shared/Enums/StripeEventStatus.cs` | Modify — add `Failed = 3` |
| `src/Aethon.Data/Entities/Organisation.cs` | Modify (add 4 fields) |
| `src/Aethon.Data/Entities/Job.cs` | Modify (add PostingTier, HighlightColour, HasAiCandidateMatching) |
| `src/Aethon.Data/Entities/StripePaymentEvent.cs` | Modify (add 5 fields) |
| `src/Aethon.Data/Entities/OrganisationJobCredit.cs` | Create |
| `src/Aethon.Data/Entities/CreditConsumptionLog.cs` | Create |
| `src/Aethon.Data/Configurations/OrganisationJobCreditConfiguration.cs` | Create |
| `src/Aethon.Data/Configurations/CreditConsumptionLogConfiguration.cs` | Create |
| `src/Aethon.Data/AethonDbContext.cs` | Modify (add DbSets) |
| `src/Aethon.Data/Migrations/*AddStripeBillingAndCreditSystem*` | Generate |
| `src/Aethon.Api/Endpoints/Webhooks/StripeWebhookEndpoints.cs` | Rewrite |
| `src/Aethon.Api/Endpoints/Billing/BillingEndpoints.cs` | Create |
| `src/Aethon.Api/Endpoints/Admin/AdminStripeEndpoints.cs` | Create/Extend |
| `src/Aethon.Web/Components/Pages/VerificationPurchase.razor` | Create |
| `src/Aethon.Web/Components/Pages/VerificationSuccess.razor` | Create |
| `src/Aethon.Web/Components/Pages/OrganisationBilling.razor` | Create |
| `src/Aethon.Web/Components/Pages/Admin/AdminStripeProducts.razor` | Create |
| `src/Aethon.Web/Components/Pages/Admin/AdminStripeEvents.razor` | Create/Upgrade |
| `src/Aethon.Web/Components/Pages/EditJob.razor` (new job page) | Modify |
