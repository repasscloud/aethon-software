using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.CancelClaimRequest;

public sealed class CancelClaimRequestHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CancelClaimRequestHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(Guid claimId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var userId = _currentUser.UserId;

        var request = await _db.OrganisationClaimRequests
            .FirstOrDefaultAsync(r => r.Id == claimId && r.RequestedByUserId == userId, ct);

        if (request is null)
            return Result.Failure("organisations.claim_not_found", "Claim request not found.");

        if (request.Status != ClaimRequestStatus.Pending)
            return Result.Failure("organisations.claim_not_pending", "Only pending claims can be cancelled.");

        request.Status = ClaimRequestStatus.Cancelled;
        request.UpdatedUtc = _dateTimeProvider.UtcNow;
        request.UpdatedByUserId = userId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
