using System.Text.Json;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.AtsMatch;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aethon.Application.Applications.Commands.SubmitJobApplication;

public sealed class SubmitJobApplicationHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAppCache _cache;
    private readonly IEmailService _emailService;
    private readonly AtsPayloadBuilderService _atsPayloadBuilder;
    private readonly ILogger<SubmitJobApplicationHandler> _logger;

    public SubmitJobApplicationHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        IAppCache cache,
        IEmailService emailService,
        AtsPayloadBuilderService atsPayloadBuilder,
        ILogger<SubmitJobApplicationHandler> logger)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _emailService = emailService;
        _atsPayloadBuilder = atsPayloadBuilder;
        _logger = logger;
    }

    public async Task<Result<SubmitJobApplicationResult>> HandleAsync(
        SubmitJobApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == command.JobId &&
                     x.Status == JobStatus.Published,
                cancellationToken);

        if (job is null)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "jobs.not_found",
                "The requested job was not found or is not open for applications.");
        }

        if (job.ApplyByUtc.HasValue && job.ApplyByUtc.Value < _dateTimeProvider.UtcNow)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.job_closed",
                "Applications are closed for this job.");
        }

        var profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Select(p => new { p.UserId, p.AgeGroup })
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        if (profile is null)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.profile_required",
                "A job seeker profile is required before applying.");
        }

        // Age group must be confirmed before applying
        if (profile.AgeGroup == ApplicantAgeGroup.NotSpecified)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.age_group_required",
                "Please confirm your age group in your profile before applying for jobs.");
        }

        // School-leaver-targeted jobs: only school leavers may apply
        if (job.IsSchoolLeaverTargeted && profile.AgeGroup != ApplicantAgeGroup.SchoolLeaver)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.age_group_ineligible",
                "This role is targeted at school leavers (16–18). You are not eligible to apply.");
        }

        // Standard (adult) jobs: school leavers may only apply if the job includes their age range
        if (!job.IsSuitableForSchoolLeavers && !job.IsSchoolLeaverTargeted
            && profile.AgeGroup == ApplicantAgeGroup.SchoolLeaver)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.age_group_ineligible",
                "This role does not accept applications from school leavers (16–18). "
                + "Please look for jobs marked as suitable for school leavers.");
        }

        var existingApplicationExists = await _dbContext.JobApplications
            .AsNoTracking()
            .AnyAsync(x => x.JobId == command.JobId && x.UserId == currentUserId, cancellationToken);

        if (existingApplicationExists)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.duplicate",
                "The current user has already applied for this job.");
        }

        if (command.ResumeFileId.HasValue)
        {
            var resumeExists = await _dbContext.StoredFiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.ResumeFileId.Value, cancellationToken);

            if (!resumeExists)
            {
                return Result<SubmitJobApplicationResult>.Failure(
                    "applications.resume_not_found",
                    "The selected resume file was not found.");
            }
        }

        var utcNow = _dateTimeProvider.UtcNow;
        var applicationId = Guid.NewGuid();

        // ── Evaluate screening questions ──────────────────────────────────────
        var isNotSuitable = false;
        var notSuitableReasons = new List<string>();

        if (!string.IsNullOrWhiteSpace(job.ScreeningQuestionsJson) &&
            !string.IsNullOrWhiteSpace(command.ScreeningAnswersJson))
        {
            try
            {
                var config = JsonSerializer.Deserialize<ScreeningConfig>(
                    job.ScreeningQuestionsJson, _jsonOptions);
                var answers = JsonSerializer.Deserialize<ScreeningAnswers>(
                    command.ScreeningAnswersJson, _jsonOptions);

                if (config is not null && answers is not null && config.AutoTagNotSuitable)
                {
                    EvaluateQuestion(config.WorkRights, answers.WorkRights,
                        "Did not meet required work rights", notSuitableReasons);
                    EvaluateQuestion(config.YearsExperience, answers.YearsExperience,
                        "Did not meet required years of experience", notSuitableReasons);
                    EvaluateQuestion(config.NoticePeriod, answers.NoticePeriod,
                        "Notice period outside acceptable range", notSuitableReasons);
                    EvaluateQuestion(config.PoliceCheck, answers.PoliceCheck,
                        "Does not hold required police check", notSuitableReasons);
                    EvaluateQuestion(config.WorkingWithChildren, answers.WorkingWithChildren,
                        "Does not hold required Working with Children Check", notSuitableReasons);
                    EvaluateQuestion(config.MedicalCheck, answers.MedicalCheck,
                        "Not willing to undertake pre-employment medical check", notSuitableReasons);
                    EvaluateQuestion(config.DriversLicence, answers.DriversLicence,
                        "Does not hold required driver's licence", notSuitableReasons);
                    EvaluateQuestion(config.CarAccess, answers.CarAccess,
                        "Does not have required access to a vehicle", notSuitableReasons);
                    EvaluateQuestion(config.PublicHolidays, answers.PublicHolidays,
                        "Not available to work on public holidays", notSuitableReasons);

                    // Qualification (multi-select: at least one answer must be acceptable)
                    if (config.Qualification.Enabled && config.Qualification.IsMustHave &&
                        config.Qualification.AcceptableAnswers.Count > 0 &&
                        answers.Qualification.Count > 0 &&
                        !answers.Qualification.Any(a => config.Qualification.AcceptableAnswers.Contains(a)))
                    {
                        notSuitableReasons.Add("Does not hold required qualification");
                    }

                    // Salary (compare applicant's min expectation vs employer's acceptable max)
                    if (config.Salary.Enabled && config.Salary.IsMustHave &&
                        !string.IsNullOrWhiteSpace(config.Salary.AcceptableMaxSalary) &&
                        !string.IsNullOrWhiteSpace(answers.SalaryMin))
                    {
                        var empMax = ParseSalary(config.Salary.AcceptableMaxSalary);
                        var appMin = ParseSalary(answers.SalaryMin);
                        if (empMax > 0 && appMin > empMax)
                            notSuitableReasons.Add("Salary expectation outside allowed range");
                    }

                    isNotSuitable = notSuitableReasons.Count > 0;
                }
            }
            catch { /* Screening evaluation is best-effort */ }
        }

        var application = new JobApplication
        {
            Id = applicationId,
            JobId = command.JobId,
            UserId = currentUserId,
            Status = ApplicationStatus.Submitted,
            ResumeFileId = command.ResumeFileId,
            CoverLetter = Normalize(command.CoverLetter),
            Source = Normalize(command.Source) ?? "AethonJobBoard",
            SubmittedUtc = utcNow,
            LastStatusChangedUtc = utcNow,
            LastActivityUtc = utcNow,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId,
            ScreeningAnswersJson = command.ScreeningAnswersJson,
            IsNotSuitable = isNotSuitable,
            NotSuitableReasons = notSuitableReasons.Count > 0
                ? string.Join("\n", notSuitableReasons)
                : null
        };

        var historyEntry = new JobApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            FromStatus = null,
            ToStatus = ApplicationStatus.Submitted,
            ChangedByUserId = currentUserId,
            ChangedUtc = utcNow,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId,
            Notes = "Application submitted."
        };

        _dbContext.JobApplications.Add(application);
        _dbContext.JobApplicationStatusHistoryEntries.Add(historyEntry);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Enqueue ATS match — non-fatal if it fails, application is already saved
        var atsProvider = job.HasAiCandidateMatching ? AtsMatchProvider.Claude : AtsMatchProvider.Ollama;
        var atsPriority = job.HasAiCandidateMatching ? 10 : 0;
        try
        {
            var payloadJson = await _atsPayloadBuilder.BuildJsonAsync(job, currentUserId, cancellationToken);
            var queueItem = new AtsMatchQueueItem
            {
                Id               = Guid.NewGuid(),
                JobApplicationId = applicationId,
                JobId            = command.JobId,
                CandidateUserId  = currentUserId,
                Provider         = atsProvider,
                Priority         = atsPriority,
                Status           = AtsMatchStatus.Pending,
                PayloadJson      = payloadJson,
                CreatedUtc       = utcNow,
                CreatedByUserId  = currentUserId
            };
            _dbContext.AtsMatchQueue.Add(queueItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue ATS match for application {Id}.", applicationId);
        }

        await _cache.RemoveByPrefixAsync(CacheKeys.MyApplicationsPrefix(currentUserId), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.JobApplicationsPrefix(command.JobId), cancellationToken);

        var applicantEmail = await _dbContext.Users
            .Where(u => u.Id == currentUserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(applicantEmail))
        {
            await _emailService.SendAsync(new EmailMessage
            {
                ToEmail = applicantEmail,
                Subject = $"Application submitted — {job.Title}",
                TextBody = $"Your application for \"{job.Title}\" has been submitted successfully.\n\nYou can track the status of your application in My applications on Aethon.",
                HtmlBody = $"""
                    <!DOCTYPE html><html><body style="font-family:Arial,sans-serif;line-height:1.5;">
                    <h2>Application submitted</h2>
                    <p>Your application for <strong>{job.Title}</strong> has been submitted successfully.</p>
                    <p>You can track the status of your application in <em>My applications</em> on Aethon.</p>
                    </body></html>
                    """
            }, cancellationToken);
        }

        return Result<SubmitJobApplicationResult>.Success(new SubmitJobApplicationResult
        {
            Id = application.Id,
            JobId = application.JobId,
            Status = application.Status,
            SubmittedUtc = application.SubmittedUtc
        });
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Evaluates a single-answer screening question. Adds <paramref name="failReason"/>
    /// to <paramref name="reasons"/> when the question is enabled, marked as must-have,
    /// has acceptable answers configured, and the applicant's answer is not in the list.
    /// </summary>
    private static void EvaluateQuestion(
        ScreeningQuestion config,
        string? answer,
        string failReason,
        List<string> reasons)
    {
        if (!config.Enabled || !config.IsMustHave || config.AcceptableAnswers.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(answer) ||
            !config.AcceptableAnswers.Contains(answer, StringComparer.OrdinalIgnoreCase))
        {
            reasons.Add(failReason);
        }
    }

    /// <summary>
    /// Parses a salary string like "$80,000" or "80000" into a decimal.
    /// Returns 0 on failure.
    /// </summary>
    private static decimal ParseSalary(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        var cleaned = value.Replace("$", "").Replace(",", "").Replace("+", "").Trim();
        return decimal.TryParse(cleaned, out var result) ? result : 0;
    }
}
