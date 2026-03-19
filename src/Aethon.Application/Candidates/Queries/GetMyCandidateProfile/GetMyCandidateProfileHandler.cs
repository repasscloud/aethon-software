using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Candidates;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Queries.GetMyCandidateProfile;

public sealed class GetMyCandidateProfileHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetMyCandidateProfileHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
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

        var profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Include(x => x.Resumes)
                .ThenInclude(x => x.StoredFile)
            .Include(x => x.Nationalities)
            .Include(x => x.Languages)
            .SingleOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        if (profile is null)
        {
            return Result<CandidateProfileDto>.Success(new CandidateProfileDto
            {
                UserId = currentUserId
            });
        }

        return Result<CandidateProfileDto>.Success(new CandidateProfileDto
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
                .ToList()
        });
    }
}
