using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Candidates;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Queries.GetMyCandidateProfile;

public sealed class GetMyCandidateProfileHandler
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(3);

    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAppCache _cache;

    public GetMyCandidateProfileHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IAppCache cache)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _cache = cache;
    }

    public async Task<Result<CandidateProfileDto>> HandleAsync(
        GetMyCandidateProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<CandidateProfileDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;
        var cacheKey = CacheKeys.CandidateProfile(currentUserId);

        var profileDto = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var profile = await _dbContext.JobSeekerProfiles
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(x => x.Resumes)
                        .ThenInclude(x => x.StoredFile)
                    .Include(x => x.Nationalities)
                    .Include(x => x.Languages)
                    .Include(x => x.WorkExperiences)
                    .Include(x => x.Qualifications)
                    .Include(x => x.Certificates)
                    .Include(x => x.Skills)
                    .SingleOrDefaultAsync(x => x.UserId == currentUserId, ct);

                if (profile is null)
                {
                    return new CandidateProfileDto
                    {
                        UserId = currentUserId
                    };
                }

                return new CandidateProfileDto
                {
                    UserId = profile.UserId,
                    FirstName = profile.FirstName,
                    MiddleName = profile.MiddleName,
                    LastName = profile.LastName,
                    DateOfBirth = profile.DateOfBirth,
                    PhoneNumber = profile.PhoneNumber,
                    WhatsAppNumber = profile.WhatsAppNumber,
                    Headline = profile.Headline,
                    Summary = profile.Summary,
                    AboutMe = profile.AboutMe,
                    CurrentLocation = profile.CurrentLocation,
                    PreferredLocation = profile.PreferredLocation,
                    AvailabilityText = profile.AvailabilityText,
                    LinkedInUrl = profile.LinkedInUrl,
                    OpenToWork = profile.OpenToWork,
                    DesiredSalaryFrom = profile.DesiredSalaryFrom,
                    DesiredSalaryTo = profile.DesiredSalaryTo,
                    DesiredSalaryCurrency = profile.DesiredSalaryCurrency,
                    WillRelocate = profile.WillRelocate,
                    RequiresSponsorship = profile.RequiresSponsorship,
                    HasWorkRights = profile.HasWorkRights,
                    IsPublicProfileEnabled = profile.IsPublicProfileEnabled,
                    IsSearchable = profile.IsSearchable,
                    Slug = profile.Slug,
                    ProfileVisibility = profile.ProfileVisibility,
                    LinkedInId = profile.LinkedInId,
                    LinkedInVerifiedAt = profile.LinkedInVerifiedAt,
                    IsIdVerified = profile.IsIdVerified,
                    IsNameLocked = profile.IsNameLocked,
                    LastProfileUpdatedUtc = profile.LastProfileUpdatedUtc,
                    Resumes = profile.Resumes
                        .OrderByDescending(x => x.IsDefault)
                        .ThenBy(x => x.Name)
                        .Select(x => new CandidateResumeDto
                        {
                            Id = x.Id,
                            StoredFileId = x.StoredFileId,
                            Name = x.Name,
                            Description = x.Description,
                            IsDefault = x.IsDefault,
                            IsActive = x.IsActive,
                            OriginalFileName = x.StoredFile.OriginalFileName,
                            ContentType = x.StoredFile.ContentType,
                            LengthBytes = x.StoredFile.LengthBytes
                        })
                        .ToList(),
                    Nationalities = profile.Nationalities
                        .OrderBy(x => x.Name)
                        .Select(x => new CandidateNationalityDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            IsVerified = x.IsVerified
                        })
                        .ToList(),
                    Languages = profile.Languages
                        .OrderBy(x => x.Name)
                        .Select(x => new CandidateLanguageDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            AbilityType = x.AbilityType,
                            ProficiencyLevel = x.ProficiencyLevel,
                            IsVerified = x.IsVerified
                        })
                        .ToList(),
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
                };
            },
            CacheTtl,
            cancellationToken);

        return Result<CandidateProfileDto>.Success(profileDto);
    }
}
