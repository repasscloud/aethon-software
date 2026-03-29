using Aethon.Shared.Enums;

namespace Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;

public sealed class UpsertMyCandidateProfileCommand
{
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }

    public ApplicantAgeGroup AgeGroup { get; init; }
    public int? BirthMonth { get; init; }
    public int? BirthYear { get; init; }

    public string? PhoneNumber { get; init; }
    public string? WhatsAppNumber { get; init; }

    public string? Headline { get; init; }
    public string? Summary { get; init; }
    public string? AboutMe { get; init; }

    public string? CurrentLocation { get; init; }
    public string? PreferredLocation { get; init; }
    public string? AvailabilityText { get; init; }

    public string? LinkedInUrl { get; init; }

    public bool OpenToWork { get; init; }
    public decimal? DesiredSalaryFrom { get; init; }
    public decimal? DesiredSalaryTo { get; init; }
    public CurrencyCode? DesiredSalaryCurrency { get; init; }

    public bool? WillRelocate { get; init; }
    public bool? RequiresSponsorship { get; init; }
    public bool? HasWorkRights { get; init; }

    public bool IsPublicProfileEnabled { get; init; }
    public bool IsSearchable { get; init; }

    public string? Slug { get; init; }

    public ProfileVisibility ProfileVisibility { get; init; }
}
