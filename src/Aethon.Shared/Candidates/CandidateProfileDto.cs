using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class CandidateProfileDto
{
    public Guid UserId { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }

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

    public DateTime? LastProfileUpdatedUtc { get; set; }

    public IReadOnlyList<CandidateResumeDto> Resumes { get; set; } = [];
    public IReadOnlyList<CandidateNationalityDto> Nationalities { get; set; } = [];
    public IReadOnlyList<CandidateLanguageDto> Languages { get; set; } = [];
}
