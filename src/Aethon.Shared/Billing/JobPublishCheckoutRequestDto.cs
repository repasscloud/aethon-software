using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Billing;

/// <summary>
/// Request to publish a job, optionally paying for Standard add-ons via
/// Stripe Checkout. If payment is needed the response carries a CheckoutUrl
/// to redirect the user to; otherwise the job is published immediately.
/// </summary>
public sealed class JobPublishCheckoutRequestDto
{
    [Required]
    public Guid JobId { get; set; }

    // ─── Standard add-ons (ignored when job tier is Premium) ─────────────────

    /// <summary>Enable the highlight colour add-on (+A$9 Standard).</summary>
    public bool AddHighlight { get; set; }

    /// <summary>Hex colour string e.g. "#FFF9C4". Required when AddHighlight = true.</summary>
    [MaxLength(20)]
    public string? HighlightColour { get; set; }

    /// <summary>Enable the video embed add-on (+A$9 Standard).</summary>
    public bool AddVideo { get; set; }

    /// <summary>YouTube video ID. Mutually exclusive with VideoVimeoId.</summary>
    [MaxLength(20)]
    public string? VideoYouTubeId { get; set; }

    /// <summary>Vimeo video ID. Mutually exclusive with VideoYouTubeId.</summary>
    [MaxLength(20)]
    public string? VideoVimeoId { get; set; }

    /// <summary>Enable the AI candidate matching add-on (+A$9 Standard).</summary>
    public bool AddAiMatching { get; set; }

    // ─── Sticky (both tiers) ──────────────────────────────────────────────────

    /// <summary>Sticky-top duration in days: 0 = none, 1 = 24h, 7 = 7d, 30 = 30d.</summary>
    public int StickyDuration { get; set; }
}
