using System.Text.Json;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.AtsMatch;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.AtsMatch;

/// <summary>
/// Builds an AtsMatchPayload snapshot from a job and candidate at application submission time.
/// The snapshot is stored in AtsMatchQueue.PayloadJson so workers need no further DB queries.
/// </summary>
public sealed class AtsPayloadBuilderService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AethonDbContext _db;

    public AtsPayloadBuilderService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<string> BuildJsonAsync(Job job, Guid candidateUserId, CancellationToken ct)
    {
        var payload = new AtsMatchPayload
        {
            Job       = BuildJobSnapshot(job),
            Candidate = await BuildCandidateSnapshotAsync(candidateUserId, ct)
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    // ── Job ────────────────────────────────────────────────────────────────────

    private static AtsJobSnapshot BuildJobSnapshot(Job job)
    {
        // Parse keywords from comma/space-separated string
        var keywords = string.IsNullOrWhiteSpace(job.Keywords)
            ? []
            : job.Keywords
                .Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        return new AtsJobSnapshot
        {
            Title          = job.Title,
            Summary        = job.Summary,
            Description    = job.Description,
            Requirements   = job.Requirements,
            Benefits       = job.Benefits,
            Keywords       = keywords,
            Category       = job.Category?.ToString(),
            EmploymentType = job.EmploymentType.ToString(),
            WorkplaceType  = job.WorkplaceType.ToString(),
            Location       = job.LocationText,
            SalaryFrom     = job.SalaryFrom,
            SalaryTo       = job.SalaryTo,
            SalaryCurrency = job.SalaryCurrency?.ToString(),
            IsImmediateStart = job.IsImmediateStart,
            ScreeningRequirements = BuildScreeningRequirements(job)
        };
    }

    private static AtsScreeningRequirements BuildScreeningRequirements(Job job)
    {
        // If no screening questions configured, return empty requirements
        if (string.IsNullOrWhiteSpace(job.ScreeningQuestionsJson))
            return new AtsScreeningRequirements();

        try
        {
            var config = JsonSerializer.Deserialize<ScreeningConfig>(
                job.ScreeningQuestionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config is null) return new AtsScreeningRequirements();

            return new AtsScreeningRequirements
            {
                WorkRightsRequired      = config.WorkRights.Enabled && config.WorkRights.IsMustHave,
                PoliceCheckRequired     = config.PoliceCheck.Enabled && config.PoliceCheck.IsMustHave,
                DriversLicenceRequired  = config.DriversLicence.Enabled && config.DriversLicence.IsMustHave,
                MinYearsExperience      = config.YearsExperience.Enabled && config.YearsExperience.IsMustHave
                                              ? TryParseMin(config.YearsExperience.AcceptableAnswers)
                                              : null,
                QualificationsRequired  = config.Qualification.Enabled && config.Qualification.IsMustHave
                                              ? config.Qualification.AcceptableAnswers
                                              : []
            };
        }
        catch
        {
            return new AtsScreeningRequirements();
        }
    }

    private static int? TryParseMin(List<string> answers)
    {
        // Acceptable answers for years exp are often strings like "3", "5", "10+"
        // Take the minimum integer found as the floor
        int? min = null;
        foreach (var a in answers)
        {
            var clean = a.Replace("+", "").Trim();
            if (int.TryParse(clean, out var v))
                min = min.HasValue ? Math.Min(min.Value, v) : v;
        }
        return min;
    }

    // ── Candidate ──────────────────────────────────────────────────────────────

    private async Task<AtsCandidateSnapshot> BuildCandidateSnapshotAsync(Guid userId, CancellationToken ct)
    {
        var profile = await _db.JobSeekerProfiles
            .AsNoTracking()
            .Include(p => p.Resumes)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
            return new AtsCandidateSnapshot();

        var skills = await _db.JobSeekerSkills
            .AsNoTracking()
            .Where(s => s.JobSeekerProfileId == profile.Id)
            .OrderBy(s => s.SortOrder)
            .Select(s => new AtsCandidateSkill(s.Name, s.SkillLevel.HasValue ? s.SkillLevel.Value.ToString() : null))
            .ToListAsync(ct);

        var experience = await _db.JobSeekerWorkExperiences
            .AsNoTracking()
            .Where(e => e.JobSeekerProfileId == profile.Id)
            .OrderBy(e => e.SortOrder)
            .Select(e => new AtsCandidateExperience(
                e.JobTitle,
                e.EmployerName,
                e.StartYear,
                e.IsCurrent ? null : e.EndYear,
                e.IsCurrent,
                e.Description))
            .ToListAsync(ct);

        var qualifications = await _db.JobSeekerQualifications
            .AsNoTracking()
            .Where(q => q.JobSeekerProfileId == profile.Id)
            .OrderBy(q => q.SortOrder)
            .Select(q => new AtsCandidateQualification(q.Title, q.Institution, q.CompletedYear))
            .ToListAsync(ct);

        // Prefer AI-extracted experience level / years from the most recent completed resume analysis
        string? experienceLevel = profile.Headline; // fallback
        int? yearsOfExperience  = null;

        var latestResume = profile.Resumes
            .OrderByDescending(r => r.CreatedUtc)
            .FirstOrDefault();

        if (latestResume is not null)
        {
            var analysis = await _db.ResumeAnalyses
                .AsNoTracking()
                .Where(a => a.JobSeekerResumeId == latestResume.Id
                         && a.Status == Aethon.Shared.Enums.ResumeAnalysisStatus.Completed)
                .OrderByDescending(a => a.AnalysedUtc)
                .FirstOrDefaultAsync(ct);

            if (analysis is not null)
            {
                if (!string.IsNullOrWhiteSpace(analysis.ExperienceLevel))
                    experienceLevel = analysis.ExperienceLevel;

                yearsOfExperience = analysis.YearsExperience;

                // Merge AI-extracted skills if profile skills are sparse
                if (skills.Count == 0 && !string.IsNullOrWhiteSpace(analysis.SkillsJson))
                {
                    try
                    {
                        var aiSkills = JsonSerializer.Deserialize<List<string>>(analysis.SkillsJson) ?? [];
                        skills = aiSkills.Select(s => new AtsCandidateSkill(s, null)).ToList();
                    }
                    catch { /* best-effort */ }
                }
            }
        }

        return new AtsCandidateSnapshot
        {
            Headline             = profile.Headline,
            Summary              = profile.Summary,
            ExperienceLevel      = experienceLevel,
            YearsOfExperience    = yearsOfExperience,
            Skills               = skills,
            WorkExperience       = experience,
            Qualifications       = qualifications,
            CurrentLocation      = profile.CurrentLocation,
            PreferredLocation    = profile.PreferredLocation,
            WillingToRelocate    = profile.WillRelocate,
            HasWorkRights        = profile.HasWorkRights,
            RequiresSponsorship  = profile.RequiresSponsorship,
            DesiredSalaryFrom    = profile.DesiredSalaryFrom,
            DesiredSalaryTo      = profile.DesiredSalaryTo,
            DesiredSalaryCurrency = profile.DesiredSalaryCurrency?.ToString(),
            Availability         = profile.AvailabilityText
        };
    }
}
