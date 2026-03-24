using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Jobs;

/// <summary>
/// Request to apply paid add-ons to an already-published job.
/// Each flag signals intent; the backend enforces billing per add-on.
/// </summary>
public sealed class JobAddonRequestDto
{
    /// <summary>Enable highlight colour on this listing.</summary>
    public bool AddHighlight { get; set; }

    /// <summary>Hex colour for the highlight (e.g. "#FFF9C4").</summary>
    [MaxLength(20)]
    public string? HighlightColour { get; set; }

    /// <summary>Enable AI candidate matching on this listing.</summary>
    public bool AddAiMatching { get; set; }

    /// <summary>Sticky-top duration in days (1, 7, or 30). 0 = no sticky.</summary>
    public int StickyDuration { get; set; }
}
