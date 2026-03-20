using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterJobs.Commands.CreateRecruiterJobDraft;

public sealed class CreateRecruiterJobDraftHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateRecruiterJobDraftHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> HandleAsync(
        RecruiterCreateJobDraftDto dto,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<Guid>.Failure("auth.unauthenticated", "Not authenticated.");

        var currentUserId = _currentUser.UserId;

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == currentUserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<Guid>.Failure("organisations.not_found", "No active recruiter membership found.");

        var recruiterOrgId = myMembership.OrganisationId;

        var companyOrgExists = await _db.Organisations
            .AsNoTracking()
            .AnyAsync(o => o.Id == dto.CompanyOrganisationId, ct);

        if (!companyOrgExists)
            return Result<Guid>.Failure("organisations.not_found", "The company organisation was not found.");

        var partnership = await _db.OrganisationRecruitmentPartnerships
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.RecruiterOrganisationId == recruiterOrgId &&
                p.CompanyOrganisationId == dto.CompanyOrganisationId &&
                p.Status == OrganisationRecruitmentPartnershipStatus.Active, ct);

        if (partnership is null)
            return Result<Guid>.Failure("partnerships.not_found", "No active partnership exists between your organisation and the specified company.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<Guid>.Failure("jobs.title_required", "Job title is required.");

        var utcNow = _dateTimeProvider.UtcNow;

        var job = new Job
        {
            Id = Guid.NewGuid(),
            OwnedByOrganisationId = dto.CompanyOrganisationId,
            ManagedByOrganisationId = recruiterOrgId,
            OrganisationRecruitmentPartnershipId = partnership.Id,
            Status = JobStatus.Draft,
            Visibility = JobVisibility.Private,
            CreatedByType = JobCreatedByType.RecruiterUser,
            Title = dto.Title.Trim(),
            Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            LocationText = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location.Trim(),
            SalaryFrom = dto.SalaryMin,
            SalaryTo = dto.SalaryMax,
            CreatedByIdentityUserId = currentUserId,
            CreatedByUser = null!,
            CreatedByUserId = currentUserId,
            CreatedUtc = utcNow
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(ct);

        return Result<Guid>.Success(job.Id);
    }
}
