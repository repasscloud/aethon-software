namespace Aethon.Shared.Organisations;

public sealed class UpsertMyMemberProfileRequestDto
{
    /// <summary>URL slug for /organisations/{org-slug}/team/{slug}. 3-60 chars, lowercase alphanumeric + hyphens.</summary>
    public string? Slug { get; set; }

    public string? JobTitle { get; set; }
    public string? Bio { get; set; }
    public string? PublicEmail { get; set; }
    public string? PublicPhone { get; set; }
    public string? LinkedInUrl { get; set; }

    /// <summary>Only takes effect if the organisation itself has IsPublicProfileEnabled = true.</summary>
    public bool IsPublicProfileEnabled { get; set; }

    /// <summary>Profile picture URL — upload handled separately; store the resolved URL here.</summary>
    public string? ProfilePictureUrl { get; set; }
}
