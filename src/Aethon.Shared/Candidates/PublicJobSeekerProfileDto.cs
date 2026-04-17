using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class PublicJobSeekerProfileDto
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? AboutMe { get; set; }
    public string? CurrentLocation { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool OpenToWork { get; set; }
    public string? Slug { get; set; }
    public ProfileVisibility ProfileVisibility { get; set; }
    public bool IsLinkedInVerified { get; set; }
    public bool IsIdVerified { get; set; }
    public DateTime? LastProfileUpdatedUtc { get; set; }
    public IReadOnlyList<JobSeekerWorkExperienceDto> WorkExperiences { get; set; } = [];
    public IReadOnlyList<JobSeekerQualificationDto> Qualifications { get; set; } = [];
    public IReadOnlyList<JobSeekerCertificateDto> Certificates { get; set; } = [];
    public IReadOnlyList<JobSeekerSkillDto> Skills { get; set; } = [];
}
