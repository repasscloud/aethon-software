using Aethon.Data.Enums;

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

    public string? PrimaryDomainId { get; set; }
    public OrganisationDomain? PrimaryDomain { get; set; }

    public bool IsProvisionedByRecruiter { get; set; }
    public string? ClaimedByUserId { get; set; }
    public DateTime? ClaimedUtc { get; set; }

    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactPhone { get; set; }

    public ICollection<OrganisationDomain> Domains { get; set; } = [];
    public ICollection<OrganisationMembership> Memberships { get; set; } = [];
    public ICollection<OrganisationInvitation> Invitations { get; set; } = [];

    public ICollection<CompanyRecruiterRelationship> CompanyRelationships { get; set; } = [];
    public ICollection<CompanyRecruiterRelationship> RecruiterRelationships { get; set; } = [];

    public ICollection<Job> OwnedJobs { get; set; } = [];
    public ICollection<Job> ManagedJobs { get; set; } = [];
}