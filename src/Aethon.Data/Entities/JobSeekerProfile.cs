using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// Candidate profile data for a job seeker account.
/// 
/// This stores public-facing profile, career preferences, and candidate details.
/// Account-level verification concerns should live on ApplicationUser.
/// </summary>
public class JobSeekerProfile : EntityBase
{
    /// <summary>
    /// The user account this profile belongs to.
    /// One job seeker user should have one profile.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation to the owning user account.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Candidate first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Candidate middle name.
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Candidate last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Age classification for this job seeker.
    /// School leavers (16–18) also supply BirthMonth + BirthYear.
    /// Adults confirm 18+ — no date is stored.
    /// NotSpecified means the job seeker has not yet confirmed their age group.
    /// </summary>
    public ApplicantAgeGroup AgeGroup { get; set; } = ApplicantAgeGroup.NotSpecified;

    /// <summary>
    /// Birth month (1–12). Only populated for SchoolLeaver age group.
    /// Used for account lifecycle management (30-day inactivity purge).
    /// </summary>
    public int? BirthMonth { get; set; }

    /// <summary>
    /// Birth year (e.g. 2009). Only populated for SchoolLeaver age group.
    /// Used alongside BirthMonth for account lifecycle management.
    /// </summary>
    public int? BirthYear { get; set; }

    /// <summary>
    /// When the job seeker confirmed their age group.
    /// </summary>
    public DateTime? AgeConfirmedUtc { get; set; }

    /// <summary>
    /// Candidate contact phone number.
    /// Verification of the number should be tracked on the user account.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Optional WhatsApp number.
    /// </summary>
    public string? WhatsAppNumber { get; set; }

    /// <summary>
    /// Headline shown on the candidate profile.
    /// Example: Senior .NET Developer | Azure | Blazor
    /// </summary>
    public string? Headline { get; set; }

    /// <summary>
    /// Candidate summary / bio / introduction.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Candidate current location in free text form.
    /// </summary>
    public string? CurrentLocation { get; set; }

    /// <summary>
    /// Candidate preferred location in free text form.
    /// </summary>
    public string? PreferredLocation { get; set; }

    /// <summary>
    /// Candidate LinkedIn profile URL.
    /// </summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// Indicates whether the candidate is currently open to work.
    /// </summary>
    public bool OpenToWork { get; set; }

    /// <summary>
    /// Desired minimum salary.
    /// </summary>
    public decimal? DesiredSalaryFrom { get; set; }

    /// <summary>
    /// Desired maximum salary.
    /// </summary>
    public decimal? DesiredSalaryTo { get; set; }

    /// <summary>
    /// Currency for the desired salary range.
    /// </summary>
    public CurrencyCode? DesiredSalaryCurrency { get; set; }

    /// <summary>
    /// Indicates whether the candidate is willing to relocate.
    /// </summary>
    public bool? WillRelocate { get; set; }

    /// <summary>
    /// Indicates whether the candidate requires sponsorship / visa support.
    /// </summary>
    public bool? RequiresSponsorship { get; set; }

    /// <summary>
    /// Indicates whether the candidate currently has work rights.
    /// This is candidate-supplied profile data, not a formal verification outcome.
    /// </summary>
    public bool? HasWorkRights { get; set; }

    /// <summary>
    /// Candidate availability / notice period in free text form.
    /// Example: Immediate / 2 weeks / 1 month
    /// </summary>
    public string? AvailabilityText { get; set; }

    /// <summary>
    /// Whether the candidate's profile can be viewed publicly.
    /// </summary>
    public bool IsPublicProfileEnabled { get; set; }

    /// <summary>
    /// Whether the candidate can appear in recruiter/company search results.
    /// This allows a candidate to have a profile without being discoverable.
    /// </summary>
    public bool IsSearchable { get; set; }

    /// <summary>
    /// Public URL slug for the candidate profile.
    /// Example: "jane-smith" or "jane-smith-dotnet"
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Optional short public "about me" text used for profile previews/cards.
    /// This is separate from the fuller summary if you want a shorter recruiter-facing intro.
    /// </summary>
    public string? AboutMe { get; set; }

    /// <summary>
    /// Controls who can view the candidate's profile.
    /// Public = accessible at /job-seeker/{slug} (requires slug).
    /// Unlisted = accessible at /job-seeker/{userId} by employers/recruiters/admins.
    /// Private = accessible only by admin/support.
    /// </summary>
    public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Private;

    /// <summary>
    /// LinkedIn account ID, populated after the user connects via LinkedIn OAuth.
    /// </summary>
    public string? LinkedInId { get; set; }

    /// <summary>
    /// When the candidate last verified their identity via LinkedIn OAuth.
    /// </summary>
    public DateTime? LinkedInVerifiedAt { get; set; }

    /// <summary>
    /// FK to a StoredFile record for the candidate's profile picture.
    /// </summary>
    public Guid? ProfilePictureStoredFileId { get; set; }

    /// <summary>
    /// Whether the candidate has been ID-verified (e.g. via document check).
    /// When true, name fields are locked for the candidate — only staff can change them.
    /// </summary>
    public bool IsIdVerified { get; set; }

    /// <summary>
    /// When set, prevents the candidate from changing FirstName/LastName/MiddleName.
    /// Set automatically when IsIdVerified becomes true.
    /// Only staff can set this back to false.
    /// </summary>
    public bool IsNameLocked { get; set; }

    /// <summary>
    /// When the candidate profile was last meaningfully updated.
    /// Useful for profile freshness and search ranking later.
    /// </summary>
    public DateTime? LastProfileUpdatedUtc { get; set; }

    /// <summary>
    /// Resume/CV files uploaded by the candidate.
    /// One of these can optionally be marked as the default resume.
    /// </summary>
    public ICollection<JobSeekerResume> Resumes { get; set; } = new List<JobSeekerResume>();

    /// <summary>
    /// Candidate nationality records.
    /// Verification should be tracked per item.
    /// </summary>
    public ICollection<JobSeekerNationality> Nationalities { get; set; } = new List<JobSeekerNationality>();

    /// <summary>
    /// Candidate language records.
    /// Spoken/written and verification should be tracked per item.
    /// </summary>
    public ICollection<JobSeekerLanguage> Languages { get; set; } = new List<JobSeekerLanguage>();

    public ICollection<JobSeekerWorkExperience> WorkExperiences { get; set; } = new List<JobSeekerWorkExperience>();
    public ICollection<JobSeekerQualification> Qualifications { get; set; } = new List<JobSeekerQualification>();
    public ICollection<JobSeekerCertificate> Certificates { get; set; } = new List<JobSeekerCertificate>();
    public ICollection<JobSeekerSkill> Skills { get; set; } = new List<JobSeekerSkill>();
}