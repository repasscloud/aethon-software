using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Commands.UpdateJob;

public sealed class UpdateJobHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly OrganisationAccessService _orgAccess;

    public UpdateJobHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        OrganisationAccessService orgAccess)
    {
        _db = db;
        _currentUser = currentUser;
        _orgAccess = orgAccess;
    }

    public async Task<Result> HandleAsync(Guid jobId, UpdateJobRequestDto request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "Job not found.");

        var canEdit = await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.OwnedByOrganisationId, ct) ||
                      (job.ManagedByOrganisationId.HasValue &&
                       await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.ManagedByOrganisationId.Value, ct));

        if (!canEdit)
            return Result.Failure("jobs.forbidden", "Insufficient permissions to edit this job.");

        job.Title = request.Title.Trim();
        job.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
        job.LocationText = string.IsNullOrWhiteSpace(request.LocationText) ? null : request.LocationText.Trim();
        job.WorkplaceType = request.WorkplaceType!.Value;
        job.EmploymentType = request.EmploymentType!.Value;
        job.Description = request.Description.Trim();
        job.Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim();
        job.Benefits = string.IsNullOrWhiteSpace(request.Benefits) ? null : request.Benefits.Trim();
        job.SalaryFrom = request.SalaryFrom;
        job.SalaryTo = request.SalaryTo;
        job.SalaryCurrency = request.SalaryCurrency;
        job.UpdatedUtc = DateTime.UtcNow;
        job.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
