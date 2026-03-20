using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Email;
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
    private readonly IEmailService _emailService;
    private readonly IAppSettings _appSettings;

    public CreateOrganisationInviteHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IEmailService emailService,
        IAppSettings appSettings)
    {
        _db = db;
        _currentUser = currentUser;
        _emailService = emailService;
        _appSettings = appSettings;
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

        var orgName = myMembership.Organisation.Name;
        var acceptUrl = $"{_appSettings.WebBaseUrl.TrimEnd('/')}/app/organisation/team?token={token}";

        await _emailService.SendAsync(new EmailMessage
        {
            ToEmail = invitation.Email,
            Subject = $"You've been invited to join {orgName} on Aethon",
            TextBody = $"You have been invited to join {orgName} on Aethon.\n\nAccept your invitation here:\n{acceptUrl}\n\nThis invitation expires in 7 days.",
            HtmlBody = $"""
                <!DOCTYPE html><html><body style="font-family:Arial,sans-serif;line-height:1.5;">
                <h2>You've been invited to join {orgName}</h2>
                <p>You have been invited to join <strong>{orgName}</strong> on Aethon.</p>
                <p><a href="{acceptUrl}" style="background:#000;color:#fff;padding:10px 20px;text-decoration:none;border-radius:6px;display:inline-block;">Accept invitation</a></p>
                <p style="color:#666;font-size:0.9em;">This invitation expires in 7 days. If you did not expect this email, you can safely ignore it.</p>
                </body></html>
                """
        }, ct);

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
