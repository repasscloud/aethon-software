using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterJobs.Commands.UpdateRecruiterJobDraft;

public sealed class UpdateRecruiterJobDraftHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateRecruiterJobDraftHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(
        Guid jobId,
        RecruiterUpdateJobDraftDto dto,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var currentUserId = _currentUser.UserId;

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == currentUserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result.Failure("organisations.not_found", "No active recruiter membership found.");

        var recruiterOrgId = myMembership.OrganisationId;

        var job = await _db.Jobs
            .FirstOrDefaultAsync(j =>
                j.Id == jobId &&
                j.ManagedByOrganisationId == recruiterOrgId &&
                j.Status == JobStatus.Draft, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "The job draft was not found or is not editable.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result.Failure("jobs.title_required", "Job title is required.");

        var utcNow = _dateTimeProvider.UtcNow;

        job.Title = dto.Title.Trim();
        job.Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim();
        job.Description = dto.Description?.Trim() ?? string.Empty;
        job.LocationText = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim();
        job.SalaryFrom = dto.SalaryMin;
        job.SalaryTo = dto.SalaryMax;
        job.UpdatedUtc = utcNow;
        job.UpdatedByUserId = currentUserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
