using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.CreateOrganisationInvite;

public sealed class CreateOrganisationInviteHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateOrganisationInviteHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganisationInviteDto>> HandleAsync(
        CreateOrganisationInviteRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationInviteDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<OrganisationInviteDto>.Failure("organisations.not_found", "No active organisation membership found.");

        var canInvite = myMembership.IsOwner ||
                        myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin ||
                        myMembership.RecruiterRole is RecruiterRole.Owner or RecruiterRole.Admin;

        if (!canInvite)
            return Result<OrganisationInviteDto>.Failure("organisations.forbidden", "Insufficient permissions to invite members.");

        CompanyRole? companyRole = null;
        RecruiterRole? recruiterRole = null;

        if (!string.IsNullOrWhiteSpace(request.CompanyRole))
        {
            if (!Enum.TryParse<CompanyRole>(request.CompanyRole, ignoreCase: true, out var parsed))
                return Result<OrganisationInviteDto>.Failure("organisations.invalid_role", "Invalid company role.");
            companyRole = parsed;
        }
        else
        {
            if (!Enum.TryParse<RecruiterRole>(request.RecruiterRole, ignoreCase: true, out var parsed))
                return Result<OrganisationInviteDto>.Failure("organisations.invalid_role", "Invalid recruiter role.");
            recruiterRole = parsed;
        }

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var expires = DateTime.UtcNow.AddDays(7);
        var emailDomain = request.Email.Contains('@') ? request.Email.Split('@')[1].Trim().ToLowerInvariant() : string.Empty;

        var invitation = new OrganisationInvitation
        {
            Id = Guid.NewGuid(),
            OrganisationId = myMembership.OrganisationId,
            Type = InvitationType.JoinOrganisation,
            Status = InvitationStatus.Pending,
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailDomain = emailDomain,
            Token = token,
            ExpiresUtc = expires,
            CompanyRole = companyRole,
            RecruiterRole = recruiterRole,
            AllowClaimAsOwner = false,
            CreatedByUserId = _currentUser.UserId,
            CreatedUtc = DateTime.UtcNow
        };

        _db.OrganisationInvitations.Add(invitation);
        await _db.SaveChangesAsync(ct);

        return Result<OrganisationInviteDto>.Success(new OrganisationInviteDto
        {
            InvitationId = invitation.Id,
            Email = invitation.Email,
            InvitationType = invitation.Type.ToString(),
            InvitationStatus = invitation.Status.ToString(),
            CompanyRole = invitation.CompanyRole?.ToString(),
            RecruiterRole = invitation.RecruiterRole?.ToString(),
            AllowClaimAsOwner = invitation.AllowClaimAsOwner,
            ExpiresUtc = invitation.ExpiresUtc,
            Token = invitation.Token
        });
    }
}
