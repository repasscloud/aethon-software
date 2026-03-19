using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
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

    public UpsertMyCandidateProfileHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
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

        var currentUserId = _currentUserAccessor.UserId;
        var utcNow = _dateTimeProvider.UtcNow;

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
        profile.DateOfBirth = command.DateOfBirth;
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
        profile.IsPublicProfileEnabled = command.IsPublicProfileEnabled;
        profile.IsSearchable = command.IsSearchable;
        profile.Slug = Normalize(command.Slug);
        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

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
        };
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
