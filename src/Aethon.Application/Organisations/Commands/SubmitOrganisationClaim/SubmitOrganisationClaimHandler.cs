using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Email;
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
    private readonly IEmailService _emailService;

    public SubmitOrganisationClaimHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _emailService = emailService;
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

        await _emailService.SendAsync(new EmailMessage
        {
            ToEmail = userEmail,
            Subject = $"Your claim request for {org.Name} — verification token",
            TextBody = $"Your claim request for {org.Name} has been submitted.\n\nYour verification token is:\n{verificationToken}\n\nSend this token to Aethon support to complete your claim verification.",
            HtmlBody = $"""
                <!DOCTYPE html><html><body style="font-family:Arial,sans-serif;line-height:1.5;">
                <h2>Claim request submitted — {org.Name}</h2>
                <p>Your claim request for <strong>{org.Name}</strong> has been submitted successfully.</p>
                <p>Your verification token is:</p>
                <p style="background:#f4f4f4;padding:12px;border-radius:6px;font-family:monospace;font-size:1.1em;">{verificationToken}</p>
                <p>Send this token to Aethon support to complete verification. You can also find it in <em>My claim requests</em>.</p>
                </body></html>
                """
        }, ct);

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
