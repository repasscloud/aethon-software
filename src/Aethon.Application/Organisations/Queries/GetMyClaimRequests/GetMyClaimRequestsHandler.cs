using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetMyClaimRequests;

public sealed class GetMyClaimRequestsHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetMyClaimRequestsHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<OrganisationClaimRequestDto>>> HandleAsync(
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<List<OrganisationClaimRequestDto>>.Failure("auth.unauthenticated", "Not authenticated.");

        var userId = _currentUser.UserId;

        var requests = await _db.OrganisationClaimRequests
            .Include(r => r.Organisation)
            .Where(r => r.RequestedByUserId == userId)
            .OrderByDescending(r => r.CreatedUtc)
            .ToListAsync(ct);

        var dtos = requests.Select(r => new OrganisationClaimRequestDto
        {
            Id = r.Id,
            OrganisationId = r.OrganisationId,
            OrganisationName = r.Organisation.Name,
            OrganisationSlug = r.Organisation.Slug ?? string.Empty,
            Status = r.Status,
            VerificationMethod = r.VerificationMethod,
            VerificationToken = r.VerificationToken,
            VerifiedUtc = r.VerifiedUtc,
            SubmittedUtc = r.CreatedUtc,
            RejectionReason = r.RejectionReason
        }).ToList();

        return Result<List<OrganisationClaimRequestDto>>.Success(dtos);
    }
}
