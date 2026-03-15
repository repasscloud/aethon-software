using Aethon.Data.Enums;
using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

public class Job : EntityBase
{
    public string OwnedByOrganisationId { get; set; } = null!;
    public Organisation OwnedByOrganisation { get; set; } = null!;

    public string? ManagedByOrganisationId { get; set; }
    public Organisation? ManagedByOrganisation { get; set; }

    public string? CompanyRecruiterRelationshipId { get; set; }
    public CompanyRecruiterRelationship? CompanyRecruiterRelationship { get; set; }

    public Guid CreatedByIdentityUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public JobCreatedByType CreatedByType { get; set; }

    public JobStatus Status { get; set; }
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

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedUtc { get; set; }

    public bool CreatedForUnclaimedCompany { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = [];
}