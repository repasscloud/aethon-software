using Aethon.Shared.Enums;

namespace Aethon.Shared.RecruiterCompanies;

public sealed class RecruiterCompanyRelationshipDto
{
    public Guid Id { get; set; }

    public Guid RecruiterOrganisationId { get; set; }
    public Guid CompanyOrganisationId { get; set; }

    public string RecruiterOrganisationName { get; set; } = string.Empty;
    public string CompanyOrganisationName { get; set; } = string.Empty;

    public CompanyRecruiterRelationshipStatus Status { get; set; }
    public CompanyRecruiterRelationshipScope Scope { get; set; }

    public bool RecruiterCanCreateUnclaimedCompanyJobs { get; set; }
    public bool RecruiterCanPublishJobs { get; set; }
    public bool RecruiterCanManageCandidates { get; set; }

    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime? ApprovedUtc { get; set; }

    public string? Notes { get; set; }
}