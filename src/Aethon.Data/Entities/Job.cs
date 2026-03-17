using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// A job posting owned by a single organisation.
/// 
/// Rules:
/// - Every job belongs to exactly one owning organisation.
/// - A recruiter-managed company job is still owned by the company organisation.
/// - A recruiter-owned agency job remains owned by the recruiter organisation.
/// - Ownership changes should be prevented by application logic.
/// </summary>
public class Job : EntityBase
{
    /// <summary>
    /// The organisation that ultimately owns this job.
    /// This is always the source of truth for ownership.
    /// </summary>
    public Guid OwnedByOrganisationId { get; set; }

    /// <summary>
    /// Navigation to the owning organisation.
    /// </summary>
    public Organisation OwnedByOrganisation { get; set; } = null!;

    /// <summary>
    /// The organisation currently managing the job, if different from the owner.
    /// For example, a recruiter managing a company-owned job.
    /// </summary>
    public Guid? ManagedByOrganisationId { get; set; }

    /// <summary>
    /// Navigation to the managing organisation.
    /// </summary>
    public Organisation? ManagedByOrganisation { get; set; }

    /// <summary>
    /// Optional link to the company-recruiter relationship that authorised this arrangement.
    /// Useful when a recruiter is acting on behalf of a company.
    /// </summary>
    public Guid? OrganisationRecruitmentPartnershipId { get; set; }

    /// <summary>
    /// Navigation to the company-recruiter relationship.
    /// </summary>
    public OrganisationRecruitmentPartnership? OrganisationRecruitmentPartnership { get; set; }

    /// <summary>
    /// The user who originally created the job record.
    /// </summary>
    public Guid CreatedByIdentityUserId { get; set; }

    /// <summary>
    /// Navigation to the user who created the job.
    /// </summary>
    public ApplicationUser CreatedByUser { get; set; } = null!;

    /// <summary>
    /// The user currently responsible for managing this job, if assigned.
    /// </summary>
    public Guid? ManagedByUserId { get; set; }

    /// <summary>
    /// Navigation to the user currently managing the job.
    /// </summary>
    public ApplicationUser? ManagedByUser { get; set; }

    /// <summary>
    /// Indicates whether the job was created directly by the owner
    /// or by a recruiter / delegated actor.
    /// </summary>
    public JobCreatedByType CreatedByType { get; set; }

    /// <summary>
    /// Current workflow status of the job.
    /// Example: Draft, PendingApproval, Published, Closed.
    /// </summary>
    public JobStatus Status { get; set; }

    /// <summary>
    /// Optional reason associated with the current status.
    /// Useful for rejection, closure, approval notes, etc.
    /// </summary>
    public string? StatusReason { get; set; }

    /// <summary>
    /// Controls whether the job is visible publicly or restricted.
    /// </summary>
    public JobVisibility Visibility { get; set; }

    /// <summary>
    /// Public-facing title of the job.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Internal or business reference code for the job.
    /// </summary>
    public string? ReferenceCode { get; set; }

    /// <summary>
    /// Optional external/system reference if the job is synced
    /// from another platform or tracked in an external system.
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Department or business unit associated with the role.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Free-text location shown to candidates.
    /// Example: Sydney NSW, Remote, Hybrid - Melbourne.
    /// </summary>
    public string? LocationText { get; set; }

    /// <summary>
    /// Indicates whether the role is onsite, remote, hybrid, etc.
    /// </summary>
    public WorkplaceType WorkplaceType { get; set; }

    /// <summary>
    /// Indicates full-time, part-time, contract, casual, etc.
    /// </summary>
    public EmploymentType EmploymentType { get; set; }

    /// <summary>
    /// Full public job description.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Optional candidate requirements / qualifications / skills.
    /// </summary>
    public string? Requirements { get; set; }

    /// <summary>
    /// Optional benefits/perks text.
    /// </summary>
    public string? Benefits { get; set; }

    /// <summary>
    /// Short summary used for previews/cards/search results.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Lower bound of the advertised salary range.
    /// </summary>
    public decimal? SalaryFrom { get; set; }

    /// <summary>
    /// Upper bound of the advertised salary range.
    /// </summary>
    public decimal? SalaryTo { get; set; }

    /// <summary>
    /// Currency for the advertised salary range.
    /// </summary>
    public CurrencyCode? SalaryCurrency { get; set; }

    /// <summary>
    /// When the job became publicly visible / published.
    /// </summary>
    public DateTime? PublishedUtc { get; set; }

    /// <summary>
    /// Optional date after which applications should no longer be accepted.
    /// This is the intended deadline, distinct from the actual closure time.
    /// </summary>
    public DateTime? ApplyByUtc { get; set; }

    /// <summary>
    /// When the job was actually closed in the system.
    /// </summary>
    public DateTime? ClosedUtc { get; set; }

    /// <summary>
    /// When the job was submitted for approval.
    /// Mainly useful where recruiter-managed jobs require company approval.
    /// </summary>
    public DateTime? SubmittedForApprovalUtc { get; set; }

    /// <summary>
    /// The user who approved the job, if approval occurred.
    /// </summary>
    public Guid? ApprovedByUserId { get; set; }

    /// <summary>
    /// When the job was approved.
    /// </summary>
    public DateTime? ApprovedUtc { get; set; }

    /// <summary>
    /// Optional external URL for applying outside the platform.
    /// Keep null for normal internal ATS applications.
    /// </summary>
    public string? ExternalApplicationUrl { get; set; }

    /// <summary>
    /// Optional application email address if applications are accepted by email.
    /// Keep null for normal internal ATS applications.
    /// </summary>
    public string? ApplicationEmail { get; set; }

    /// <summary>
    /// Indicates the job was created before the owning company had formally claimed its profile.
    /// </summary>
    public bool CreatedForUnclaimedCompany { get; set; }

    /// <summary>
    /// Applications submitted against this job.
    /// </summary>
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}