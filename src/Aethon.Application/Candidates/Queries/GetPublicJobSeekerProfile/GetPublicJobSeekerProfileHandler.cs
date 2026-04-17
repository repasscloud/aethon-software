using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Candidates;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Queries.GetPublicJobSeekerProfile;

public sealed class GetPublicJobSeekerProfileHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetPublicJobSeekerProfileHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Resolves a profile by slug (must be Public) or by userId (access rules apply).
    /// </summary>
    public async Task<Result<PublicJobSeekerProfileDto>> HandleAsync(
        string identifier,
        CancellationToken ct = default)
    {
        var profile = Guid.TryParse(identifier, out var userId)
            ? await _db.JobSeekerProfiles.AsNoTracking().AsSplitQuery()
                .Include(p => p.WorkExperiences)
                .Include(p => p.Qualifications)
                .Include(p => p.Certificates)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            : await _db.JobSeekerProfiles.AsNoTracking().AsSplitQuery()
                .Include(p => p.WorkExperiences)
                .Include(p => p.Qualifications)
                .Include(p => p.Certificates)
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.Slug == identifier.ToLowerInvariant(), ct);

        if (profile is null)
            return Result<PublicJobSeekerProfileDto>.Failure("profile.not_found", "Profile not found.");

        var isStaff = _currentUser.IsStaff;
        var appType  = _currentUser.AppType ?? "";
        var isEmployerOrRecruiter = appType is "employer" or "recruiter";

        var canAccess = profile.ProfileVisibility switch
        {
            ProfileVisibility.Public   => true,
            ProfileVisibility.Unlisted => isStaff || isEmployerOrRecruiter,
            ProfileVisibility.Private  => isStaff,
            _                          => false
        };

        // Slug-based access only works for Public profiles
        if (!Guid.TryParse(identifier, out _) && profile.ProfileVisibility != ProfileVisibility.Public)
            canAccess = false;

        if (!canAccess)
            return Result<PublicJobSeekerProfileDto>.Failure("profile.not_found", "Profile not found.");

        return Result<PublicJobSeekerProfileDto>.Success(new PublicJobSeekerProfileDto
        {
            UserId               = profile.UserId,
            FirstName            = profile.FirstName,
            LastName             = profile.LastName,
            Headline             = profile.Headline,
            Summary              = profile.Summary,
            AboutMe              = profile.AboutMe,
            CurrentLocation      = profile.CurrentLocation,
            LinkedInUrl          = profile.LinkedInUrl,
            OpenToWork           = profile.OpenToWork,
            Slug                 = profile.Slug,
            ProfileVisibility    = profile.ProfileVisibility,
            IsLinkedInVerified   = profile.LinkedInVerifiedAt.HasValue,
            IsIdVerified         = profile.IsIdVerified,
            LastProfileUpdatedUtc = profile.LastProfileUpdatedUtc,
            WorkExperiences = profile.WorkExperiences
                .OrderBy(x => x.SortOrder)
                .ThenByDescending(x => x.StartYear)
                .ThenByDescending(x => x.StartMonth)
                .Select(x => new JobSeekerWorkExperienceDto
                {
                    Id = x.Id,
                    JobTitle = x.JobTitle,
                    EmployerName = x.EmployerName,
                    StartMonth = x.StartMonth,
                    StartYear = x.StartYear,
                    EndMonth = x.EndMonth,
                    EndYear = x.EndYear,
                    IsCurrent = x.IsCurrent,
                    Description = x.Description,
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Qualifications = profile.Qualifications
                .OrderBy(x => x.SortOrder)
                .Select(x => new JobSeekerQualificationDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Institution = x.Institution,
                    CompletedYear = x.CompletedYear,
                    Description = x.Description,
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Certificates = profile.Certificates
                .OrderBy(x => x.SortOrder)
                .Select(x => new JobSeekerCertificateDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IssuingOrganisation = x.IssuingOrganisation,
                    IssuedMonth = x.IssuedMonth,
                    IssuedYear = x.IssuedYear,
                    ExpiryYear = x.ExpiryYear,
                    CredentialId = x.CredentialId,
                    CredentialUrl = x.CredentialUrl,
                    SortOrder = x.SortOrder
                })
                .ToList(),
            Skills = profile.Skills
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new JobSeekerSkillDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    SkillLevel = x.SkillLevel,
                    SortOrder = x.SortOrder
                })
                .ToList()
        });
    }
}
