using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class Job : EntityBase
{
    public Guid OwnedByOrganisationId { get; set; }
    public Organisation OwnedByOrganisation { get; set; } = null!;

    public Guid? ManagedByOrganisationId { get; set; }
    public Organisation? ManagedByOrganisation { get; set; }

    public Guid? CompanyRecruiterRelationshipId { get; set; }
    public CompanyRecruiterRelationship? CompanyRecruiterRelationship { get; set; }

    public Guid CreatedByIdentityUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public Guid? ManagedByUserId { get; set; }
    public ApplicationUser? ManagedByUser { get; set; }
    
    public JobCreatedByType CreatedByType { get; set; }

    public JobStatus Status { get; set; }
    public string? StatusReason { get; set; }
    public JobVisibility Visibility { get; set; }

    public string Title { get; set; } = null!;
    public string? ReferenceCode { get; set; }
    public string? Department { get; set; }
    public string? LocationText { get; set; }
    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }

    public string Description { get; set; } = null!;
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }

    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }

    public DateTime? PublishedUtc { get; set; }
    public DateTime? ClosedUtc { get; set; }

    public DateTime? SubmittedForApprovalUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedUtc { get; set; }

    public string? Summary { get; set; }

    public bool CreatedForUnclaimedCompany { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}