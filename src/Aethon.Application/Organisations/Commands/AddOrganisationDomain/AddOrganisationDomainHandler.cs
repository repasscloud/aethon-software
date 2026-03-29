using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.AddOrganisationDomain;

public sealed class AddOrganisationDomainHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddOrganisationDomainHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<OrganisationDomainDto>> HandleAsync(
        AddOrganisationDomainRequestDto dto,
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

        var normalizedDomain = dto.Domain.Trim().ToLowerInvariant();

        if (!normalizedDomain.Contains('.') || normalizedDomain.Contains(' '))
            return Result<OrganisationDomainDto>.Failure(
                "domains.invalid",
                "Invalid domain format.");

        var isDuplicate = await _dbContext.Set<OrganisationDomain>()
            .AnyAsync(
                d => d.OrganisationId == orgId && d.NormalizedDomain == normalizedDomain,
                ct);

        if (isDuplicate)
            return Result<OrganisationDomainDto>.Failure(
                "domains.duplicate",
                "Domain already registered.");

        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var recordName = "_aethon-verify." + normalizedDomain;
        var recordValue = "aethon-verification=" + token;

        var isFirst = !await _dbContext.Set<OrganisationDomain>()
            .AnyAsync(d => d.OrganisationId == orgId, ct);

        var utcNow = _dateTimeProvider.UtcNow;

        var domain = new OrganisationDomain
        {
            Id = Guid.NewGuid(),
            OrganisationId = orgId,
            Domain = dto.Domain.Trim(),
            NormalizedDomain = normalizedDomain,
            IsPrimary = isFirst,
            Status = DomainStatus.Pending,
            VerificationMethod = dto.VerificationMethod,
            TrustLevel = DomainTrustLevel.Low,
            VerificationToken = token,
            VerificationDnsRecordName = recordName,
            VerificationDnsRecordValue = recordValue,
            VerificationRequestedUtc = utcNow,
            CreatedUtc = utcNow,
            CreatedByUserId = userId
        };

        _dbContext.Set<OrganisationDomain>().Add(domain);
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
