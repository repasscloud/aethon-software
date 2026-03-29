using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

/// <summary>
/// Public-facing profile for an organisation team member.
/// Separate from ApplicationUser (identity/account) and JobSeekerProfile (candidate-specific).
/// Only surfaced publicly when both the organisation and the member have enabled their public profile.
/// </summary>
public class OrganisationMemberProfile : EntityBase
{
    /// <summary>The user this profile belongs to.</summary>
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    /// <summary>The organisation this profile is scoped to.</summary>
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    /// <summary>
    /// URL slug for this member's profile page within the organisation.
    /// Unique per organisation. Used at /organisations/{org-slug}/team/{slug}.
    /// Lowercase alphanumeric + hyphens only, 3–60 characters.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>Job title displayed on the team page.</summary>
    public string? JobTitle { get; set; }

    /// <summary>Short bio displayed on the member's public profile.</summary>
    public string? Bio { get; set; }

    /// <summary>URL to the member's profile picture (stored file or external URL).</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>Publicly displayed email address (may differ from account email).</summary>
    public string? PublicEmail { get; set; }

    /// <summary>Publicly displayed phone number.</summary>
    public string? PublicPhone { get; set; }

    /// <summary>LinkedIn profile URL.</summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// When true, this member's profile is shown on the organisation's public team page.
    /// Only has effect if the organisation itself also has IsPublicProfileEnabled = true.
    /// </summary>
    public bool IsPublicProfileEnabled { get; set; }
}
