using System.Security.Claims;
using Aethon.Api.Infrastructure;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Authorize]
[Route("org")]
public sealed class OrganisationController : ControllerBase
{
    private readonly AethonDbContext _dbContext;

    public OrganisationController(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("me/members")]
    public async Task<IActionResult> GetMyMembers()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organisationIdValue = User.FindFirstValue(AppClaimTypes.OrganisationId);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Guid.TryParse(organisationIdValue, out var organisationId))
        {
            return BadRequest();
        }

        var organisation = await _dbContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == organisationId);

        if (organisation is null)
        {
            return NotFound();
        }

        var currentMembership = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OrganisationId == organisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        if (currentMembership is null)
        {
            return Forbid();
        }

        var members = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .Where(x => x.OrganisationId == organisationId)
            .OrderByDescending(x => x.IsOwner)
            .ThenBy(x => x.JoinedUtc)
            .Select(x => new OrganisationMemberDto
            {
                UserId = x.UserId.ToString(),
                DisplayName = x.User.DisplayName,
                Email = x.User.Email ?? "",
                IsOwner = x.IsOwner,
                MembershipStatus = x.Status.ToString(),
                CompanyRole = x.CompanyRole != null ? x.CompanyRole.Value.ToString() : null,
                RecruiterRole = x.RecruiterRole != null ? x.RecruiterRole.Value.ToString() : null,
                JoinedUtc = x.JoinedUtc
            })
            .ToListAsync();

        var pendingInvites = await _dbContext.OrganisationInvitations
            .AsNoTracking()
            .Where(x =>
                x.OrganisationId == organisationId &&
                x.Status == InvitationStatus.Pending)
            .OrderBy(x => x.Email)
            .Select(x => new OrganisationInviteDto
            {
                InvitationId = x.Id,
                Email = x.Email,
                InvitationType = x.Type.ToString(),
                InvitationStatus = x.Status.ToString(),
                CompanyRole = x.CompanyRole != null ? x.CompanyRole.Value.ToString() : null,
                RecruiterRole = x.RecruiterRole != null ? x.RecruiterRole.Value.ToString() : null,
                AllowClaimAsOwner = x.AllowClaimAsOwner,
                ExpiresUtc = x.ExpiresUtc,
                Token = x.Token
            })
            .ToListAsync();

        return Ok(new OrganisationMembersResponseDto
        {
            OrganisationId = organisation.Id,
            OrganisationName = organisation.Name,
            OrganisationType = organisation.Type == OrganisationType.RecruiterAgency ? "recruiter" : "company",
            IsOwner = currentMembership.IsOwner,
            CanInvite = currentMembership.IsOwner,
            Members = members,
            PendingInvites = pendingInvites
        });
    }

    [HttpPost("me/invites")]
    public async Task<IActionResult> CreateInvite([FromBody] CreateOrganisationInviteRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organisationIdValue = User.FindFirstValue(AppClaimTypes.OrganisationId);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Guid.TryParse(organisationIdValue, out var organisationId))
        {
            return BadRequest();
        }

        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(validationErrors);
        }

        var membership = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .Include(x => x.Organisation)
            .FirstOrDefaultAsync(x =>
                x.OrganisationId == organisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        if (membership is null || !membership.IsOwner)
        {
            return Forbid();
        }

        var inviteEmail = request.Email.Trim();
        var normalizedEmail = inviteEmail.ToUpperInvariant();
        var emailDomain = inviteEmail[(inviteEmail.LastIndexOf('@') + 1)..].Trim().ToLowerInvariant();

        var existingPendingInvite = await _dbContext.OrganisationInvitations
            .FirstOrDefaultAsync(x =>
                x.OrganisationId == organisationId &&
                x.NormalizedEmail == normalizedEmail &&
                x.Status == InvitationStatus.Pending);

        if (existingPendingInvite is not null)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateOrganisationInviteRequestDto.Email)] = ["A pending invite already exists for this email address."]
            });
        }

        var existingMembership = await _dbContext.OrganisationMemberships
            .AnyAsync(x =>
                x.OrganisationId == organisationId &&
                x.User.Email != null &&
                x.User.NormalizedEmail == normalizedEmail &&
                x.Status == MembershipStatus.Active);

        if (existingMembership)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateOrganisationInviteRequestDto.Email)] = ["This user is already an active member of the organisation."]
            });
        }

        CompanyRole? companyRole = null;
        RecruiterRole? recruiterRole = null;

        if (membership.Organisation.Type == OrganisationType.Company)
        {
            if (!Enum.TryParse<CompanyRole>(request.CompanyRole, true, out var parsedCompanyRole))
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(CreateOrganisationInviteRequestDto.CompanyRole)] = ["A valid company role is required."]
                });
            }

            companyRole = parsedCompanyRole;
        }
        else
        {
            if (!Enum.TryParse<RecruiterRole>(request.RecruiterRole, true, out var parsedRecruiterRole))
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(CreateOrganisationInviteRequestDto.RecruiterRole)] = ["A valid recruiter role is required."]
                });
            }

            recruiterRole = parsedRecruiterRole;
        }

        var invite = new OrganisationInvitation
        {
            Id = Guid.NewGuid(),
            Type = InvitationType.JoinOrganisation,
            Status = InvitationStatus.Pending,
            OrganisationId = organisationId,
            Email = inviteEmail,
            NormalizedEmail = normalizedEmail,
            EmailDomain = emailDomain,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresUtc = DateTime.UtcNow.AddDays(7),
            CompanyRole = companyRole,
            RecruiterRole = recruiterRole,
            AllowClaimAsOwner = false,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _dbContext.OrganisationInvitations.Add(invite);
        await _dbContext.SaveChangesAsync();

        return Ok(new OrganisationInviteDto
        {
            InvitationId = invite.Id,
            Email = invite.Email,
            InvitationType = invite.Type.ToString(),
            InvitationStatus = invite.Status.ToString(),
            CompanyRole = invite.CompanyRole?.ToString(),
            RecruiterRole = invite.RecruiterRole?.ToString(),
            AllowClaimAsOwner = invite.AllowClaimAsOwner,
            ExpiresUtc = invite.ExpiresUtc,
            Token = invite.Token
        });
    }

    [HttpPost("invites/accept")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptOrganisationInviteRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(email))
        {
            return BadRequest();
        }

        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(validationErrors);
        }

        var normalizedEmail = email.ToUpperInvariant();

        var invite = await _dbContext.OrganisationInvitations
            .FirstOrDefaultAsync(x =>
                x.Token == request.Token &&
                x.Status == InvitationStatus.Pending);

        if (invite is null)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(AcceptOrganisationInviteRequestDto.Token)] = ["Invitation was not found."]
            });
        }

        if (invite.ExpiresUtc < DateTime.UtcNow)
        {
            invite.Status = InvitationStatus.Expired;
            invite.UpdatedUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(AcceptOrganisationInviteRequestDto.Token)] = ["Invitation has expired."]
            });
        }

        if (!string.Equals(invite.NormalizedEmail, normalizedEmail, StringComparison.Ordinal))
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(AcceptOrganisationInviteRequestDto.Token)] = ["This invitation does not belong to your signed-in account."]
            });
        }

        var alreadyMember = await _dbContext.OrganisationMemberships
            .AnyAsync(x =>
                x.OrganisationId == invite.OrganisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        if (!alreadyMember)
        {
            _dbContext.OrganisationMemberships.Add(new OrganisationMembership
            {
                Id = Guid.NewGuid(),
                OrganisationId = invite.OrganisationId,
                UserId = userId,
                Status = MembershipStatus.Active,
                CompanyRole = invite.CompanyRole,
                RecruiterRole = invite.RecruiterRole,
                IsOwner = false,
                JoinedUtc = DateTime.UtcNow,
                CreatedUtc = DateTime.UtcNow,
                CreatedByUserId = invite.CreatedByUserId
            });
        }

        invite.Status = InvitationStatus.Accepted;
        invite.AcceptedByUserId = userId;
        invite.AcceptedUtc = DateTime.UtcNow;
        invite.UpdatedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("me/profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var organisationIdValue = User.FindFirstValue(AppClaimTypes.OrganisationId);

        if (!Guid.TryParse(organisationIdValue, out var organisationId))
        {
            return BadRequest();
        }

        var organisation = await _dbContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == organisationId);

        if (organisation is null)
        {
            return NotFound();
        }

        return Ok(new OrganisationProfileDto
        {
            OrganisationId = organisation.Id,
            OrganisationType = organisation.Type.ToString(),
            Name = organisation.Name,
            LegalName = organisation.LegalName,
            WebsiteUrl = organisation.WebsiteUrl,
            Slug = organisation.Slug,
            LogoUrl = organisation.LogoUrl,
            Summary = organisation.Summary,
            PublicLocationText = organisation.PublicLocationText,
            PublicContactEmail = organisation.PublicContactEmail,
            PublicContactPhone = organisation.PublicContactPhone,
            IsPublicProfileEnabled = organisation.IsPublicProfileEnabled
        });
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateOrganisationProfileRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organisationIdValue = User.FindFirstValue(AppClaimTypes.OrganisationId);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Guid.TryParse(organisationIdValue, out var organisationId))
        {
            return BadRequest();
        }

        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(validationErrors);
        }

        var membership = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OrganisationId == organisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        if (membership is null || !membership.IsOwner)
        {
            return Forbid();
        }

        var organisation = await _dbContext.Organisations
            .FirstOrDefaultAsync(x => x.Id == organisationId);

        if (organisation is null)
        {
            return NotFound();
        }

        var slug = NormalizeSlug(request.Slug);
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var slugInUse = await _dbContext.Organisations
                .AnyAsync(x => x.Id != organisation.Id && x.Slug == slug);

            if (slugInUse)
            {
                return ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(UpdateOrganisationProfileRequestDto.Slug)] = ["That slug is already in use."]
                });
            }
        }

        organisation.Name = request.Name.Trim();
        organisation.NormalizedName = request.Name.Trim().ToUpperInvariant();
        organisation.LegalName = Clean(request.LegalName);
        organisation.WebsiteUrl = Clean(request.WebsiteUrl);
        organisation.Slug = slug;
        organisation.LogoUrl = Clean(request.LogoUrl);
        organisation.Summary = Clean(request.Summary);
        organisation.PublicLocationText = Clean(request.PublicLocationText);
        organisation.PublicContactEmail = Clean(request.PublicContactEmail);
        organisation.PublicContactPhone = Clean(request.PublicContactPhone);
        organisation.IsPublicProfileEnabled = request.IsPublicProfileEnabled;
        organisation.UpdatedUtc = DateTime.UtcNow;
        organisation.UpdatedByUserId = userId;

        await _dbContext.SaveChangesAsync();

        return Ok(new OrganisationProfileDto
        {
            OrganisationId = organisation.Id,
            OrganisationType = organisation.Type.ToString(),
            Name = organisation.Name,
            LegalName = organisation.LegalName,
            WebsiteUrl = organisation.WebsiteUrl,
            Slug = organisation.Slug,
            LogoUrl = organisation.LogoUrl,
            Summary = organisation.Summary,
            PublicLocationText = organisation.PublicLocationText,
            PublicContactEmail = organisation.PublicContactEmail,
            PublicContactPhone = organisation.PublicContactPhone,
            IsPublicProfileEnabled = organisation.IsPublicProfileEnabled
        });
    }

    private ObjectResult ValidationProblem(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeSlug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var slug = value.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\-{2,}", "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? null : slug;
    }
}
