using Aethon.Application.Abstractions.Authentication;
using Aethon.Shared.Enums;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Candidates;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;

public sealed class UpsertMyCandidateProfileHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAppCache _cache;

    public UpsertMyCandidateProfileHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        IAppCache cache)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
    }

    public async Task<Result<CandidateProfileDto>> HandleAsync(
        UpsertMyCandidateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<CandidateProfileDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        if (command.DesiredSalaryFrom.HasValue && command.DesiredSalaryFrom.Value < 0)
        {
            return Result<CandidateProfileDto>.Failure(
                "candidates.profile.salary_from_invalid",
                "Desired salary from must be zero or greater.");
        }

        if (command.DesiredSalaryTo.HasValue && command.DesiredSalaryTo.Value < 0)
        {
            return Result<CandidateProfileDto>.Failure(
                "candidates.profile.salary_to_invalid",
                "Desired salary to must be zero or greater.");
        }

        if (command.DesiredSalaryFrom.HasValue &&
            command.DesiredSalaryTo.HasValue &&
            command.DesiredSalaryTo.Value < command.DesiredSalaryFrom.Value)
        {
            return Result<CandidateProfileDto>.Failure(
                "candidates.profile.salary_range_invalid",
                "Desired salary to must be greater than or equal to desired salary from.");
        }

        if ((command.DesiredSalaryFrom.HasValue || command.DesiredSalaryTo.HasValue) &&
            command.DesiredSalaryCurrency is null)
        {
            return Result<CandidateProfileDto>.Failure(
                "candidates.profile.salary_currency_required",
                "Desired salary currency is required when salary is provided.");
        }

        if (command.ProfileVisibility == ProfileVisibility.Public && string.IsNullOrWhiteSpace(command.Slug))
        {
            return Result<CandidateProfileDto>.Failure(
                "candidates.profile.slug_required_for_public",
                "A profile URL slug is required before setting your profile to Public.");
        }

        var currentUserId = _currentUserAccessor.UserId;
        var utcNow = _dateTimeProvider.UtcNow;

        // Slug uniqueness check (exclude current user's own profile)
        if (!string.IsNullOrWhiteSpace(command.Slug))
        {
            var normalised = command.Slug.Trim().ToLowerInvariant();
            var slugTaken = await _dbContext.JobSeekerProfiles
                .AnyAsync(p => p.Slug == normalised && p.UserId != currentUserId, cancellationToken);
            if (slugTaken)
                return Result<CandidateProfileDto>.Failure(
                    "candidates.profile.slug_taken",
                    "This profile URL is already in use. Please choose a different one.");
        }

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
                .ThenInclude(x => x.StoredFile)
            .Include(x => x.Nationalities)
            .Include(x => x.Languages)
            .SingleOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        if (profile is null)
        {
            profile = new JobSeekerProfile
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                CreatedUtc = utcNow,
                CreatedByUserId = currentUserId
            };

            _dbContext.JobSeekerProfiles.Add(profile);
        }

        profile.FirstName = Normalize(command.FirstName);
        profile.MiddleName = Normalize(command.MiddleName);
        profile.LastName = Normalize(command.LastName);

        // Age group — only update when explicitly being changed from NotSpecified,
        // or when School Leaver provides birth month/year.
        if (command.AgeGroup != ApplicantAgeGroup.NotSpecified)
        {
            profile.AgeGroup = command.AgeGroup;

            if (command.AgeGroup == ApplicantAgeGroup.SchoolLeaver)
            {
                profile.BirthMonth = command.BirthMonth;
                profile.BirthYear = command.BirthYear;
            }
            else
            {
                // Adult: clear any birth date fields that may have been set previously
                profile.BirthMonth = null;
                profile.BirthYear = null;
            }

            if (profile.AgeConfirmedUtc is null)
                profile.AgeConfirmedUtc = utcNow;
        }

        profile.PhoneNumber = Normalize(command.PhoneNumber);
        profile.WhatsAppNumber = Normalize(command.WhatsAppNumber);
        profile.Headline = Normalize(command.Headline);
        profile.Summary = Normalize(command.Summary);
        profile.AboutMe = Normalize(command.AboutMe);
        profile.CurrentLocation = Normalize(command.CurrentLocation);
        profile.PreferredLocation = Normalize(command.PreferredLocation);
        profile.AvailabilityText = Normalize(command.AvailabilityText);
        profile.LinkedInUrl = Normalize(command.LinkedInUrl);
        profile.OpenToWork = command.OpenToWork;
        profile.DesiredSalaryFrom = command.DesiredSalaryFrom;
        profile.DesiredSalaryTo = command.DesiredSalaryTo;
        profile.DesiredSalaryCurrency = command.DesiredSalaryCurrency;
        profile.WillRelocate = command.WillRelocate;
        profile.RequiresSponsorship = command.RequiresSponsorship;
        profile.HasWorkRights = command.HasWorkRights;
        profile.ProfileVisibility = command.ProfileVisibility;
        profile.IsPublicProfileEnabled = command.ProfileVisibility == ProfileVisibility.Public;
        profile.IsSearchable = command.IsSearchable;
        profile.Slug = Normalize(command.Slug);
        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.CandidateProfile(currentUserId), cancellationToken);

        return Result<CandidateProfileDto>.Success(Map(profile));
    }

    private static CandidateProfileDto Map(JobSeekerProfile profile)
    {
        return new CandidateProfileDto
        {
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            MiddleName = profile.MiddleName,
            LastName = profile.LastName,
            AgeGroup = profile.AgeGroup,
            BirthMonth = profile.BirthMonth,
            BirthYear = profile.BirthYear,
            AgeConfirmedUtc = profile.AgeConfirmedUtc,
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
        };
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
