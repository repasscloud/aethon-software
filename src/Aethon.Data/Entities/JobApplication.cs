using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// A single candidate application to a job.
/// 
/// This entity should hold the current state and key reporting fields.
/// Detailed workflow history, comments, interviews, and audit data should
/// be stored in separate child entities later.
/// </summary>
public class JobApplication : EntityBase
{
    /// <summary>
    /// The job this application belongs to.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// Navigation to the job.
    /// </summary>
    public Job Job { get; set; } = null!;

    /// <summary>
    /// The user account that submitted the application.
    /// For a normal candidate flow, this is the applying job seeker.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation to the applicant user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Current application status.
    /// This is the current snapshot status only.
    /// Full history should be stored separately later.
    /// </summary>
    public ApplicationStatus Status { get; set; }

    /// <summary>
    /// Optional reason or note for the current status.
    /// Useful for shortlist/reject/on-hold/withdrawn explanations.
    /// </summary>
    public string? StatusReason { get; set; }

    /// <summary>
    /// The resume file submitted with this application, if present.
    /// </summary>
    public Guid? ResumeFileId { get; set; }

    /// <summary>
    /// Optional navigation to the stored resume file.
    /// </summary>
    public StoredFile? ResumeFile { get; set; }

    /// <summary>
    /// Candidate cover letter / introduction text.
    /// </summary>
    public string? CoverLetter { get; set; }

    /// <summary>
    /// Optional recruiter/user assigned to handle this application.
    /// </summary>
    public Guid? AssignedRecruiterUserId { get; set; }

    /// <summary>
    /// Navigation to the assigned recruiter / handler.
    /// </summary>
    public ApplicationUser? AssignedRecruiterUser { get; set; }

    /// <summary>
    /// When the current recruiter/user was assigned.
    /// </summary>
    public DateTime? AssignedRecruiterUtc { get; set; }

    /// <summary>
    /// Optional internal owner / case manager for the application.
    /// This gives you flexibility if you later separate recruiter assignment
    /// from general internal ownership.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Navigation to the assigned internal owner.
    /// </summary>
    public ApplicationUser? AssignedToUser { get; set; }

    /// <summary>
    /// When the application was submitted.
    /// </summary>
    public DateTime SubmittedUtc { get; set; }

    /// <summary>
    /// Last time the status changed.
    /// </summary>
    public DateTime? LastStatusChangedUtc { get; set; }

    /// <summary>
    /// Last time any meaningful application data changed.
    /// Useful for queues, sorting, dashboards, and reporting.
    /// </summary>
    public DateTime? LastActivityUtc { get; set; }

    /// <summary>
    /// Free-text source, kept for flexibility.
    /// Example: LinkedIn, Seek, Referral, Careers Page.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// More specific source detail if needed.
    /// Example: campaign name, recruiter desk, referral note, ad variant.
    /// </summary>
    public string? SourceDetail { get; set; }

    /// <summary>
    /// Optional external source/reference ID from a third-party system.
    /// </summary>
    public string? SourceReference { get; set; }

    /// <summary>
    /// Internal general notes stored directly on the record.
    /// Keep this as a summary field only.
    /// Full notes/comments should become child entities later.
    /// </summary>
    public string? InternalSummaryNotes { get; set; }

    /// <summary>
    /// Optional screening summary written by the internal team.
    /// </summary>
    public string? ScreeningSummary { get; set; }

    /// <summary>
    /// Optional reviewer score/rating for quick ranking.
    /// Keep broad for now; can be refined later.
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Optional internal recommendation summary.
    /// Example: Strong Yes / Yes / Hold / No.
    /// Kept as text so you do not need another enum immediately.
    /// </summary>
    public string? Recommendation { get; set; }

    /// <summary>
    /// Optional free-text tags to support search/filtering before a proper tag model exists.
    /// Example: ".NET;Azure;Sydney;Immediate"
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Candidate phone number captured at time of application if needed
    /// for historical accuracy, even if their profile later changes.
    /// </summary>
    public string? CandidatePhoneNumber { get; set; }

    /// <summary>
    /// Candidate location captured at time of application.
    /// </summary>
    public string? CandidateLocationText { get; set; }

    /// <summary>
    /// Candidate notice period / availability captured at time of application.
    /// </summary>
    public string? AvailabilityText { get; set; }

    /// <summary>
    /// Candidate salary expectation captured at application time.
    /// </summary>
    public decimal? SalaryExpectation { get; set; }

    /// <summary>
    /// Currency for the candidate salary expectation.
    /// </summary>
    public CurrencyCode? SalaryExpectationCurrency { get; set; }

    /// <summary>
    /// Indicates whether the candidate is willing to relocate.
    /// </summary>
    public bool? WillRelocate { get; set; }

    /// <summary>
    /// Indicates whether the candidate requires sponsorship / work rights support.
    /// </summary>
    public bool? RequiresSponsorship { get; set; }

    /// <summary>
    /// Indicates whether the candidate confirmed work rights for the relevant market.
    /// </summary>
    public bool? HasWorkRights { get; set; }

    /// <summary>
    /// Indicates whether the candidate consented to privacy / data processing terms.
    /// </summary>
    public bool AcceptedPrivacyPolicy { get; set; }

    /// <summary>
    /// When privacy / data processing consent was recorded.
    /// </summary>
    public DateTime? AcceptedPrivacyPolicyUtc { get; set; }

    /// <summary>
    /// Indicates the candidate has withdrawn their application.
    /// This may overlap with Status, but makes reporting easier.
    /// </summary>
    public bool IsWithdrawn { get; set; }

    /// <summary>
    /// When the application was withdrawn.
    /// </summary>
    public DateTime? WithdrawnUtc { get; set; }

    /// <summary>
    /// Optional reason supplied for withdrawal.
    /// </summary>
    public string? WithdrawalReason { get; set; }

    /// <summary>
    /// Optional user who recorded the withdrawal.
    /// Could be the candidate or an internal user.
    /// </summary>
    public Guid? WithdrawnByUserId { get; set; }

    /// <summary>
    /// Navigation to the user who recorded the withdrawal.
    /// </summary>
    public ApplicationUser? WithdrawnByUser { get; set; }

    /// <summary>
    /// Indicates the application has been rejected.
    /// This may overlap with Status, but makes reporting and queries easier.
    /// </summary>
    public bool IsRejected { get; set; }

    /// <summary>
    /// When the rejection occurred.
    /// </summary>
    public DateTime? RejectedUtc { get; set; }

    /// <summary>
    /// Optional rejection reason.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Optional user who rejected the application.
    /// </summary>
    public Guid? RejectedByUserId { get; set; }

    /// <summary>
    /// Navigation to the rejecting user.
    /// </summary>
    public ApplicationUser? RejectedByUser { get; set; }

    /// <summary>
    /// Indicates the candidate was hired from this application.
    /// </summary>
    public bool IsHired { get; set; }

    /// <summary>
    /// When the candidate was marked as hired.
    /// </summary>
    public DateTime? HiredUtc { get; set; }

    /// <summary>
    /// Indicates this application is marked as a duplicate.
    /// </summary>
    public bool IsDuplicate { get; set; }

    /// <summary>
    /// If duplicate, references the canonical/original application.
    /// </summary>
    public Guid? DuplicateOfApplicationId { get; set; }

    /// <summary>
    /// Navigation to the canonical/original application.
    /// </summary>
    public JobApplication? DuplicateOfApplication { get; set; }

    /// <summary>
    /// Whether the application is archived from active working views.
    /// Useful for keeping historical data without deleting it.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// When the application was archived.
    /// </summary>
    public DateTime? ArchivedUtc { get; set; }

    /// <summary>
    /// Optional external application reference for integrations/imports.
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// JSON-serialised ScreeningAnswers — the applicant's responses to the
    /// enabled screening questions on the job.
    /// </summary>
    public string? ScreeningAnswersJson { get; set; }

    /// <summary>
    /// Whether this application was automatically tagged as Not Suitable due
    /// to one or more Must-have screening question mismatches.
    /// </summary>
    public bool IsNotSuitable { get; set; }

    /// <summary>
    /// Newline-separated list of reasons why the application was tagged Not Suitable.
    /// e.g. "Did not meet required work rights\nSalary expectation outside allowed range"
    /// </summary>
    public string? NotSuitableReasons { get; set; }

    /// <summary>
    /// Historical status changes for this application.
    /// </summary>
    public ICollection<JobApplicationStatusHistory> StatusHistory { get; set; } = new List<JobApplicationStatusHistory>();

    /// <summary>
    /// Internal notes for this application.
    /// </summary>
    public ICollection<JobApplicationNote> NotesCollection { get; set; } = new List<JobApplicationNote>();

    /// <summary>
    /// Internal comments/discussion for this application.
    /// </summary>
    public ICollection<JobApplicationComment> Comments { get; set; } = new List<JobApplicationComment>();

    /// <summary>
    /// Interviews scheduled for this application.
    /// </summary>
    public ICollection<JobApplicationInterview> Interviews { get; set; } = new List<JobApplicationInterview>();
}