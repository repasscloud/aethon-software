using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.SubmitOrganisationClaim;

public sealed class SubmitOrganisationClaimHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitOrganisationClaimHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<OrganisationClaimRequestDto>> HandleAsync(
        SubmitClaimRequestDto dto,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationClaimRequestDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var userId = _currentUser.UserId;

        var userEmail = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);

        if (userEmail is null)
            return Result<OrganisationClaimRequestDto>.Failure("auth.user_not_found", "User account not found.");

        var slugNormalized = dto.OrganisationSlug.ToLower().Trim();

        var org = await _db.Organisations
            .FirstOrDefaultAsync(o => o.Slug != null && o.Slug.ToLower() == slugNormalized, ct);

        if (org is null)
            return Result<OrganisationClaimRequestDto>.Failure("organisations.not_found", "Organisation not found.");

        if (org.ClaimStatus != OrganisationClaimStatus.Unclaimed)
            return Result<OrganisationClaimRequestDto>.Failure("organisations.not_claimable", "This organisation is not available for claiming.");

        var hasPendingClaim = await _db.OrganisationClaimRequests
            .AnyAsync(r => r.OrganisationId == org.Id
                        && r.RequestedByUserId == userId
                        && r.Status == ClaimRequestStatus.Pending, ct);

        if (hasPendingClaim)
            return Result<OrganisationClaimRequestDto>.Failure("organisations.claim_already_pending", "You already have a pending claim for this organisation.");

        var emailDomain = userEmail.Split('@').Last().ToLowerInvariant();
        var verificationToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var utcNow = _dateTimeProvider.UtcNow;

        var claimRequest = new OrganisationClaimRequest
        {
            Id = Guid.NewGuid(),
            OrganisationId = org.Id,
            RequestedByUserId = userId,
            EmailUsed = userEmail,
            EmailDomain = emailDomain,
            Status = ClaimRequestStatus.Pending,
            VerificationMethod = DomainVerificationMethod.Email,
            VerificationToken = verificationToken,
            CreatedUtc = utcNow,
            CreatedByUserId = userId
        };

        _db.OrganisationClaimRequests.Add(claimRequest);
        await _db.SaveChangesAsync(ct);

        return Result<OrganisationClaimRequestDto>.Success(new OrganisationClaimRequestDto
        {
            Id = claimRequest.Id,
            OrganisationId = org.Id,
            OrganisationName = org.Name,
            OrganisationSlug = org.Slug ?? string.Empty,
            Status = claimRequest.Status,
            VerificationMethod = claimRequest.VerificationMethod,
            VerificationToken = claimRequest.VerificationToken,
            VerifiedUtc = claimRequest.VerifiedUtc,
            SubmittedUtc = claimRequest.CreatedUtc,
            RejectionReason = claimRequest.RejectionReason
        });
    }
}
