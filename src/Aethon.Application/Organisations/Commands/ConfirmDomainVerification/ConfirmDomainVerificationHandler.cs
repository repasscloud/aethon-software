using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.ConfirmDomainVerification;

public sealed class ConfirmDomainVerificationHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConfirmDomainVerificationHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<OrganisationDomainDto>> HandleAsync(
        Guid domainId,
        ConfirmDomainVerificationRequestDto dto,
        CancellationToken ct = default)
    {
        if (!_currentUserAccessor.IsAuthenticated)
            return Result<OrganisationDomainDto>.Failure(
                "auth.unauthenticated",
                "Not authenticated.");

        var userId = _currentUserAccessor.UserId;

        var membership = await _dbContext.Set<OrganisationMembership>()
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.Status == MembershipStatus.Active,
                ct);

        if (membership is null)
            return Result<OrganisationDomainDto>.Failure(
                "organisations.not_found",
                "No active organisation membership.");

        var orgId = membership.OrganisationId;

        var isAdminOrOwner = await _dbContext.Set<OrganisationMembership>()
            .AnyAsync(
                m => m.UserId == userId
                     && m.OrganisationId == orgId
                     && m.Status == MembershipStatus.Active
                     && (m.IsOwner
                         || m.CompanyRole == CompanyRole.Admin
                         || m.CompanyRole == CompanyRole.Owner
                         || m.RecruiterRole == RecruiterRole.Admin
                         || m.RecruiterRole == RecruiterRole.Owner),
                ct);

        if (!isAdminOrOwner)
            return Result<OrganisationDomainDto>.Failure(
                "organisations.forbidden",
                "Insufficient permissions.");

        var domain = await _dbContext.Set<OrganisationDomain>()
            .FirstOrDefaultAsync(
                d => d.Id == domainId && d.OrganisationId == orgId,
                ct);

        if (domain is null)
            return Result<OrganisationDomainDto>.Failure(
                "domains.not_found",
                "Domain not found.");

        if (domain.VerificationToken != dto.Token)
            return Result<OrganisationDomainDto>.Failure(
                "domains.token_mismatch",
                "Verification token does not match.");

        if (domain.Status == DomainStatus.Verified)
            return Result<OrganisationDomainDto>.Failure(
                "domains.already_verified",
                "Domain is already verified.");

        var utcNow = _dateTimeProvider.UtcNow;

        domain.Status = DomainStatus.Verified;
        domain.VerifiedUtc = utcNow;
        domain.VerifiedByUserId = userId;
        domain.TrustLevel = DomainTrustLevel.Medium;
        domain.UpdatedUtc = utcNow;
        domain.UpdatedByUserId = userId;

        await _dbContext.SaveChangesAsync(ct);

        return Result<OrganisationDomainDto>.Success(new OrganisationDomainDto
        {
            Id = domain.Id,
            OrganisationId = domain.OrganisationId,
            Domain = domain.Domain,
            IsPrimary = domain.IsPrimary,
            Status = domain.Status,
            VerificationMethod = domain.VerificationMethod,
            TrustLevel = domain.TrustLevel,
            VerificationToken = domain.VerificationToken,
            VerificationDnsRecordName = domain.VerificationDnsRecordName,
            VerificationDnsRecordValue = domain.VerificationDnsRecordValue,
            VerificationEmailAddress = domain.VerificationEmailAddress,
            VerifiedUtc = domain.VerifiedUtc
        });
    }
}
