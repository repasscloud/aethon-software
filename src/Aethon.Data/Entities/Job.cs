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

    /// <summary>Structured city name from Google Places.</summary>
    public string? LocationCity { get; set; }

    /// <summary>Structured state/region from Google Places.</summary>
    public string? LocationState { get; set; }

    /// <summary>Structured country name from Google Places.</summary>
    public string? LocationCountry { get; set; }

    /// <summary>ISO 3166-1 alpha-2 country code from Google Places.</summary>
    public string? LocationCountryCode { get; set; }

    /// <summary>GPS latitude for radius-based job search.</summary>
    public double? LocationLatitude { get; set; }

    /// <summary>GPS longitude for radius-based job search.</summary>
    public double? LocationLongitude { get; set; }

    /// <summary>Google Places place_id for the job location.</summary>
    public string? LocationPlaceId { get; set; }

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

    // ─── Extended classification ────────────────────────────────────────────

    /// <summary>Industry / department category.</summary>
    public JobCategory? Category { get; set; }

    /// <summary>JSON-serialised list of JobRegion values for this role.</summary>
    public string? Regions { get; set; }

    /// <summary>
    /// JSON-serialised list of country names associated with this job.
    /// </summary>
    public string? Countries { get; set; }

    /// <summary>
    /// When this job posting expires and becomes invisible on the public board.
    /// After this date no new applications are accepted and the listing is hidden.
    /// </summary>
    public DateTime? PostingExpiresUtc { get; set; }

    // ─── Display & promotion ─────────────────────────────────────────────────

    /// <summary>Whether to show the owning organisation logo on the public listing.</summary>
    public bool IncludeCompanyLogo { get; set; }

    /// <summary>Whether this job is highlighted in search results.</summary>
    public bool IsHighlighted { get; set; }

    /// <summary>When the sticky-to-top promotion expires. Null means not sticky.</summary>
    public DateTime? StickyUntilUtc { get; set; }

    // ─── Application options ─────────────────────────────────────────────────

    /// <summary>
    /// JSON-serialised list of selected benefit tag strings.
    /// </summary>
    public string? BenefitsTags { get; set; }

    /// <summary>Free-text special requirements for applicants.</summary>
    public string? ApplicationSpecialRequirements { get; set; }

    // ─── Pay & commission ─────────────────────────────────────────────────────

    /// <summary>Whether the role includes a commission component.</summary>
    public bool HasCommission { get; set; }

    /// <summary>Lower bound of the OTE (On Target Earnings = base + commission).</summary>
    public decimal? OteFrom { get; set; }

    /// <summary>Upper bound of the OTE.</summary>
    public decimal? OteTo { get; set; }

    // ─── Job flags ────────────────────────────────────────────────────────────

    /// <summary>Whether the role is available for an immediate start.</summary>
    public bool IsImmediateStart { get; set; }

    // ─── Media ────────────────────────────────────────────────────────────────

    /// <summary>YouTube video ID for an optional job video embed (mutually exclusive with VimeoVideoId).</summary>
    public string? VideoYouTubeId { get; set; }

    /// <summary>Vimeo video ID for an optional job video embed (mutually exclusive with VideoYouTubeId).</summary>
    public string? VideoVimeoId { get; set; }

    // ─── Screening questions ─────────────────────────────────────────────────

    /// <summary>
    /// JSON-serialised ScreeningConfig — employer-configured screening questions
    /// and per-question acceptable answers.  Null means no screening questions.
    /// </summary>
    public string? ScreeningQuestionsJson { get; set; }

    // ─── ATS / search ────────────────────────────────────────────────────────

    /// <summary>Comma-separated or space-separated keywords used for ATS matching.</summary>
    public string? Keywords { get; set; }

    /// <summary>Internal purchase-order / posting-reference number.</summary>
    public string? PoNumber { get; set; }

    // ─── Auto-match & short URL ───────────────────────────────────────────────

    /// <summary>Whether the job should be auto-matched to candidate profiles.</summary>
    public bool AllowAutoMatch { get; set; }

    /// <summary>Short URL slug generated for this job posting.</summary>
    public string? ShortUrlCode { get; set; }

    // ─── Relationships ────────────────────────────────────────────────────────

    /// <summary>
    /// Applications submitted against this job.
    /// </summary>
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}