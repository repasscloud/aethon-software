namespace Aethon.Application.Abstractions.Settings;

public static class SystemSettingKeys
{
    public const string GoogleIndexingEnabled         = "GoogleIndexing.Enabled";
    public const string GoogleIndexingServiceAccount  = "GoogleIndexing.ServiceAccountJson";

    // ─── Stripe ───────────────────────────────────────────────────────────────
    // Secret key lives in appsettings.json / environment vars — NOT here.
    // Webhook signing secret and all price IDs are managed via the admin UI.

    public const string StripeWebhookSecret                     = "Stripe.WebhookSecret";

    // Verification
    public const string StripePriceVerificationStandard         = "Stripe.Price.VerificationStandard";
    public const string StripePriceVerificationEnhanced         = "Stripe.Price.VerificationEnhanced";

    // Verification + first post bundles
    public const string StripePriceBundleStandardVerificationPost  = "Stripe.Price.Bundle.StandardVerificationPost";
    public const string StripePriceBundleEnhancedVerificationPost  = "Stripe.Price.Bundle.EnhancedVerificationPost";

    // Job posting credit packs — Standard
    public const string StripePriceJobStandard1x                = "Stripe.Price.Job.Standard.1x";
    public const string StripePriceJobStandard5x                = "Stripe.Price.Job.Standard.5x";
    public const string StripePriceJobStandard10x               = "Stripe.Price.Job.Standard.10x";
    public const string StripePriceJobStandard20x               = "Stripe.Price.Job.Standard.20x";

    // Job posting credit packs — Premium
    public const string StripePriceJobPremium1x                 = "Stripe.Price.Job.Premium.1x";
    public const string StripePriceJobPremium5x                 = "Stripe.Price.Job.Premium.5x";
    public const string StripePriceJobPremium10x                = "Stripe.Price.Job.Premium.10x";
    public const string StripePriceJobPremium20x                = "Stripe.Price.Job.Premium.20x";

    // Sticky — verified org pricing
    public const string StripePriceStickyVerified24h            = "Stripe.Price.Sticky.Verified.24h";
    public const string StripePriceStickyVerified7d             = "Stripe.Price.Sticky.Verified.7d";
    public const string StripePriceStickyVerified30d            = "Stripe.Price.Sticky.Verified.30d";

    // Sticky — unverified org pricing
    public const string StripePriceStickyUnverified24h          = "Stripe.Price.Sticky.Unverified.24h";
    public const string StripePriceStickyUnverified7d           = "Stripe.Price.Sticky.Unverified.7d";
    public const string StripePriceStickyUnverified30d          = "Stripe.Price.Sticky.Unverified.30d";

    // Standard job post add-ons (included in Premium)
    public const string StripePriceAddonHighlight               = "Stripe.Price.Addon.Highlight";
    public const string StripePriceAddonVideo                   = "Stripe.Price.Addon.Video";
    public const string StripePriceAddonAiMatching              = "Stripe.Price.Addon.AiMatching";
}
