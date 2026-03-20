using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetOrganisationDomains;

public sealed class GetOrganisationDomainsHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetOrganisationDomainsHandler(AethonDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<List<OrganisationDomainDto>>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUserAccessor.IsAuthenticated)
            return Result<List<OrganisationDomainDto>>.Failure(
                "auth.unauthenticated",
                "Not authenticated.");

        var userId = _currentUserAccessor.UserId;

        var membership = await _dbContext.Set<OrganisationMembership>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.Status == MembershipStatus.Active,
                ct);

        if (membership is null)
            return Result<List<OrganisationDomainDto>>.Failure(
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
            return Result<List<OrganisationDomainDto>>.Failure(
                "organisations.forbidden",
                "Insufficient permissions.");

        var domains = await _dbContext.Set<OrganisationDomain>()
            .AsNoTracking()
            .Where(d => d.OrganisationId == orgId)
            .OrderBy(d => d.IsPrimary ? 0 : 1)
            .ThenBy(d => d.Domain)
            .ToListAsync(ct);

        var result = domains.Select(d => new OrganisationDomainDto
        {
            Id = d.Id,
            OrganisationId = d.OrganisationId,
            Domain = d.Domain,
            IsPrimary = d.IsPrimary,
            Status = d.Status,
            VerificationMethod = d.VerificationMethod,
            TrustLevel = d.TrustLevel,
            VerificationToken = d.VerificationToken,
            VerificationDnsRecordName = d.VerificationDnsRecordName,
            VerificationDnsRecordValue = d.VerificationDnsRecordValue,
            VerificationEmailAddress = d.VerificationEmailAddress,
            VerifiedUtc = d.VerifiedUtc
        }).ToList();

        return Result<List<OrganisationDomainDto>>.Success(result);
    }
}
