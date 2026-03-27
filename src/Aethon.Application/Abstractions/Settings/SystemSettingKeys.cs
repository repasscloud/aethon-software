namespace Aethon.Application.Abstractions.Settings;

public static class SystemSettingKeys
{
    public const string GoogleIndexingEnabled         = "GoogleIndexing.Enabled";
    public const string GoogleIndexingServiceAccount  = "GoogleIndexing.ServiceAccountJson";

    // ─── Stripe ───────────────────────────────────────────────────────────────
    // Secret key lives in appsettings.json / environment vars — NOT here.
    // Webhook signing secret and all price IDs are managed via the admin UI.

    public const string StripeSecretKey                         = "Stripe.SecretKey";
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

    // ─── Display Prices ───────────────────────────────────────────────────────
    // Human-readable prices shown in the UI (e.g. "19" → displayed as "A$19").
    // Store as a plain number string without currency symbol.
    // Update via seed script or Admin → Stripe Products.

    // Job posting packs — Standard
    public const string DisplayPriceJobStandard1x                = "Display.Price.Job.Standard.1x";
    public const string DisplayPriceJobStandard5x                = "Display.Price.Job.Standard.5x";
    public const string DisplayPriceJobStandard10x               = "Display.Price.Job.Standard.10x";
    public const string DisplayPriceJobStandard20x               = "Display.Price.Job.Standard.20x";

    // Job posting packs — Premium
    public const string DisplayPriceJobPremium1x                 = "Display.Price.Job.Premium.1x";
    public const string DisplayPriceJobPremium5x                 = "Display.Price.Job.Premium.5x";
    public const string DisplayPriceJobPremium10x                = "Display.Price.Job.Premium.10x";
    public const string DisplayPriceJobPremium20x                = "Display.Price.Job.Premium.20x";

    // Verification
    public const string DisplayPriceVerificationStandard         = "Display.Price.Verification.Standard";
    public const string DisplayPriceVerificationEnhanced         = "Display.Price.Verification.Enhanced";

    // Bundles
    public const string DisplayPriceBundleStandardVerificationPost  = "Display.Price.Bundle.StandardVerificationPost";
    public const string DisplayPriceBundleEnhancedVerificationPost  = "Display.Price.Bundle.EnhancedVerificationPost";

    // Add-ons
    public const string DisplayPriceAddonHighlight               = "Display.Price.Addon.Highlight";
    public const string DisplayPriceAddonVideo                   = "Display.Price.Addon.Video";
    public const string DisplayPriceAddonAiMatching              = "Display.Price.Addon.AiMatching";

    // Sticky — verified
    public const string DisplayPriceStickyVerified24h            = "Display.Price.Sticky.Verified.24h";
    public const string DisplayPriceStickyVerified7d             = "Display.Price.Sticky.Verified.7d";
    public const string DisplayPriceStickyVerified30d            = "Display.Price.Sticky.Verified.30d";

    // Sticky — unverified
    public const string DisplayPriceStickyUnverified24h          = "Display.Price.Sticky.Unverified.24h";
    public const string DisplayPriceStickyUnverified7d           = "Display.Price.Sticky.Unverified.7d";
    public const string DisplayPriceStickyUnverified30d          = "Display.Price.Sticky.Unverified.30d";

    // ─── Email ────────────────────────────────────────────────────────────────
    // Resolution order: DB (here) → ENV VAR / appsettings.json → misconfigured.
    // If both DB and ENV are null/empty the admin dashboard will show a warning.

    public const string EmailMailerSendApiKey = "Email__MailerSendApiKey";
    public const string EmailFromEmail        = "Email__FromEmail";
    public const string EmailFromName         = "Email__FromName";
    public const string EmailWebBaseUrl       = "Email__WebBaseUrl";

    // ─── Email Templates ──────────────────────────────────────────────────────
    // System templates (non-deletable). Each has a Subject and Html key.
    // Custom templates follow the same pattern with an admin-chosen name slug.
    // Variable substitution uses {{VarName}} tokens.

    public const string EmailTemplateVerificationSubject         = "EmailTemplate__Verification__Subject";
    public const string EmailTemplateVerificationHtml            = "EmailTemplate__Verification__Html";

    public const string EmailTemplatePasswordResetSubject        = "EmailTemplate__PasswordReset__Subject";
    public const string EmailTemplatePasswordResetHtml           = "EmailTemplate__PasswordReset__Html";

    public const string EmailTemplatePasswordResetConfirmSubject = "EmailTemplate__PasswordResetConfirm__Subject";
    public const string EmailTemplatePasswordResetConfirmHtml    = "EmailTemplate__PasswordResetConfirm__Html";

    public const string EmailTemplateStaffWelcomeSubject         = "EmailTemplate__StaffWelcome__Subject";
    public const string EmailTemplateStaffWelcomeHtml            = "EmailTemplate__StaffWelcome__Html";

    public const string EmailTemplateIdentityRejectionSubject    = "EmailTemplate__IdentityRejection__Subject";
    public const string EmailTemplateIdentityRejectionHtml       = "EmailTemplate__IdentityRejection__Html";

    // ─── Feature Flags ────────────────────────────────────────────────────────
    // These control marketing promotions and billing behaviour. All toggled via
    // /admin/settings so no deployment is required to run or stop a promotion.

    /// <summary>
    /// When true, unused Standard LaunchPromotion credits are converted to Premium
    /// credits when an organisation verifies. Default true.
    /// </summary>
    public const string FeatureVerificationUpgradesPromoCredits = "Feature.VerificationUpgradesPromoCredits";

    /// <summary>Whether the launch promotion credit grant is active for new registrations.</summary>
    public const string FeatureLaunchPromotionEnabled           = "Feature.LaunchPromotion.Enabled";

    /// <summary>Number of free Standard job post credits granted to each new org. Default 10.</summary>
    public const string FeatureLaunchPromotionFreeJobPostCount  = "Feature.LaunchPromotion.FreeJobPostCount";

    /// <summary>Number of days before launch promo credits expire. Default 90.</summary>
    public const string FeatureLaunchPromotionExpiryDays        = "Feature.LaunchPromotion.ExpiryDays";
}
