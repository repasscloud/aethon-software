using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetOrganisationMembers;

public sealed class GetOrganisationMembersHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetOrganisationMembersHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganisationMembersResponseDto>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationMembersResponseDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<OrganisationMembersResponseDto>.Failure("organisations.not_found", "No active organisation membership found.");

        var orgId = myMembership.OrganisationId;
        var org = myMembership.Organisation;

        var members = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.OrganisationId == orgId && m.Status == MembershipStatus.Active)
            .ToListAsync(ct);

        var invites = await _db.OrganisationInvitations
            .AsNoTracking()
            .Where(i => i.OrganisationId == orgId && i.Status == InvitationStatus.Pending)
            .ToListAsync(ct);

        var canInvite = myMembership.IsOwner ||
                        myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin ||
                        myMembership.RecruiterRole is RecruiterRole.Owner or RecruiterRole.Admin;

        return Result<OrganisationMembersResponseDto>.Success(new OrganisationMembersResponseDto
        {
            OrganisationId = orgId,
            OrganisationName = org.Name,
            OrganisationType = org.Type.ToString().ToLowerInvariant(),
            IsOwner = myMembership.IsOwner,
            CanInvite = canInvite,
            Members = members.Select(m => new OrganisationMemberDto
            {
                UserId = m.UserId.ToString(),
                DisplayName = m.User.DisplayName,
                Email = m.User.Email ?? "",
                IsOwner = m.IsOwner,
                MembershipStatus = m.Status.ToString(),
                CompanyRole = m.CompanyRole?.ToString(),
                RecruiterRole = m.RecruiterRole?.ToString(),
                JoinedUtc = m.JoinedUtc
            }).ToList(),
            PendingInvites = invites.Select(i => new OrganisationInviteDto
            {
                InvitationId = i.Id,
                Email = i.Email,
                InvitationType = i.Type.ToString(),
                InvitationStatus = i.Status.ToString(),
                CompanyRole = i.CompanyRole?.ToString(),
                RecruiterRole = i.RecruiterRole?.ToString(),
                AllowClaimAsOwner = i.AllowClaimAsOwner,
                ExpiresUtc = i.ExpiresUtc,
                Token = i.Token
            }).ToList()
        });
    }
}
