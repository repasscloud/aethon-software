using Aethon.Data.Enums;

namespace Aethon.Data.Entities;

public class CompanyRecruiterRelationship : EntityBase
{
    public string CompanyOrganisationId { get; set; } = null!;
    public Organisation CompanyOrganisation { get; set; } = null!;

    public string RecruiterOrganisationId { get; set; } = null!;
    public Organisation RecruiterOrganisation { get; set; } = null!;

    public CompanyRecruiterRelationshipStatus Status { get; set; }
    public CompanyRecruiterRelationshipScope Scope { get; set; }

    public bool RecruiterCanCreateUnclaimedCompanyJobs { get; set; }
    public bool RecruiterCanPublishJobs { get; set; }
    public bool RecruiterCanManageCandidates { get; set; }

    public string? RequestedByUserId { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedUtc { get; set; }

    public string? Notes { get; set; }

    public ICollection<Job> Jobs { get; set; } = [];
}