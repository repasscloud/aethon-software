using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class CandidateProfileDto
{
    public Guid UserId { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }

    /// <summary>Age classification — SchoolLeaver (16–18) or Adult (18+). NotSpecified until confirmed.</summary>
    public ApplicantAgeGroup AgeGroup { get; set; }

    /// <summary>Birth month (1–12). Only populated for SchoolLeaver.</summary>
    public int? BirthMonth { get; set; }

    /// <summary>Birth year (e.g. 2009). Only populated for SchoolLeaver.</summary>
    public int? BirthYear { get; set; }

    /// <summary>When the age group was confirmed.</summary>
    public DateTime? AgeConfirmedUtc { get; set; }

    public string? PhoneNumber { get; set; }
    public string? WhatsAppNumber { get; set; }

    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? AboutMe { get; set; }

    public string? CurrentLocation { get; set; }
    public string? PreferredLocation { get; set; }
    public string? AvailabilityText { get; set; }

    public string? LinkedInUrl { get; set; }

    public bool OpenToWork { get; set; }
    public decimal? DesiredSalaryFrom { get; set; }
    public decimal? DesiredSalaryTo { get; set; }
    public CurrencyCode? DesiredSalaryCurrency { get; set; }

    public bool? WillRelocate { get; set; }
    public bool? RequiresSponsorship { get; set; }
    public bool? HasWorkRights { get; set; }

    public bool IsPublicProfileEnabled { get; set; }
    public bool IsSearchable { get; set; }
    public string? Slug { get; set; }
    public ProfileVisibility ProfileVisibility { get; set; }

    public string? LinkedInId { get; set; }
    public DateTime? LinkedInVerifiedAt { get; set; }
    public bool IsIdVerified { get; set; }
    public bool IsNameLocked { get; set; }

    public DateTime? LastProfileUpdatedUtc { get; set; }

    public IReadOnlyList<CandidateResumeDto> Resumes { get; set; } = [];
    public IReadOnlyList<CandidateNationalityDto> Nationalities { get; set; } = [];
    public IReadOnlyList<CandidateLanguageDto> Languages { get; set; } = [];
    public IReadOnlyList<JobSeekerWorkExperienceDto> WorkExperiences { get; set; } = [];
    public IReadOnlyList<JobSeekerQualificationDto> Qualifications { get; set; } = [];
    public IReadOnlyList<JobSeekerCertificateDto> Certificates { get; set; } = [];
    public IReadOnlyList<JobSeekerSkillDto> Skills { get; set; } = [];
}
