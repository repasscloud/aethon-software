using System.ComponentModel.DataAnnotations.Schema;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class Organisation : EntityBase
{
    public OrganisationType Type { get; set; }
    public OrganisationStatus Status { get; set; }
    public OrganisationClaimStatus ClaimStatus { get; set; }

    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;
    public string? LegalName { get; set; }
    public string? WebsiteUrl { get; set; }

    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? Summary { get; set; }
    public string? PublicLocationText { get; set; }
    public string? PublicContactEmail { get; set; }
    public string? PublicContactPhone { get; set; }
    public bool IsPublicProfileEnabled { get; set; }

    public bool IsEqualOpportunityEmployer { get; set; }
    public bool IsAccessibleWorkplace { get; set; }

    public CompanySize? CompanySize { get; set; }
    public JobCategory? Industry { get; set; }
    public string? BannerImageUrl { get; set; }

    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TikTokHandle { get; set; }
    public string? InstagramHandle { get; set; }
    public string? YouTubeUrl { get; set; }

    public Guid? PrimaryDomainId { get; set; }
    public OrganisationDomain? PrimaryDomain { get; set; }

    public bool IsProvisionedByRecruiter { get; set; }
    public Guid? ClaimedByUserId { get; set; }
    public DateTime? ClaimedUtc { get; set; }

    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactPhone { get; set; }

    public VerificationTier VerificationTier { get; set; } = VerificationTier.None;
    public DateTime? VerifiedUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }

    [NotMapped]
    public bool IsVerified => VerificationTier != VerificationTier.None;

    public ICollection<OrganisationDomain> Domains { get; set; } = [];
    public ICollection<OrganisationMembership> Memberships { get; set; } = [];
    public ICollection<OrganisationInvitation> Invitations { get; set; } = [];

    public ICollection<OrganisationRecruitmentPartnership> CompanyRelationships { get; set; } = [];
    public ICollection<OrganisationRecruitmentPartnership> RecruiterRelationships { get; set; } = [];

    public ICollection<Job> OwnedJobs { get; set; } = [];
    public ICollection<Job> ManagedJobs { get; set; } = [];
}
