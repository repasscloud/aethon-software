using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class CompanyRecruiterRelationship : EntityBase
{
    public Guid CompanyOrganisationId { get; set; }
    public Organisation CompanyOrganisation { get; set; } = null!;

    public Guid RecruiterOrganisationId { get; set; }
    public Organisation RecruiterOrganisation { get; set; } = null!;

    public CompanyRecruiterRelationshipStatus Status { get; set; }
    public CompanyRecruiterRelationshipScope Scope { get; set; }

    public bool RecruiterCanCreateUnclaimedCompanyJobs { get; set; }
    public bool RecruiterCanPublishJobs { get; set; }
    public bool RecruiterCanManageCandidates { get; set; }

    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedUtc { get; set; }

    public string? Notes { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}