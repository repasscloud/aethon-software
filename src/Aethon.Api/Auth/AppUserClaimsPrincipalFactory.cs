using System.Security.Claims;
using Aethon.Data;
using Aethon.Data.Identity;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Auth;

public sealed class AppUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly AethonDbContext _dbContext;

    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        AethonDbContext dbContext)
        : base(userManager, roleManager, optionsAccessor)
    {
        _dbContext = dbContext;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim(AppClaimTypes.DisplayName, user.DisplayName ?? string.Empty));

        var tenantMembership = await _dbContext.UserTenantMemberships
            .Where(x => x.UserId == user.Id && x.IsDefault)
            .Join(
                _dbContext.Tenants,
                membership => membership.TenantId,
                tenant => tenant.Id,
                (membership, tenant) => new { membership, tenant })
            .FirstOrDefaultAsync();

        if (tenantMembership is not null)
        {
            identity.AddClaim(new Claim(AppClaimTypes.TenantId, tenantMembership.tenant.Id.ToString()));
            identity.AddClaim(new Claim(AppClaimTypes.TenantSlug, tenantMembership.tenant.Slug));
            identity.AddClaim(new Claim(ClaimTypes.Role, tenantMembership.membership.RoleCode));
        }

        var organisationMembership = await _dbContext.OrganisationMemberships
            .Where(x => x.UserId == user.Id && x.Status == MembershipStatus.Active)
            .Select(x => new
            {
                x.OrganisationId,
                x.IsOwner,
                x.CompanyRole,
                x.RecruiterRole,
                x.JoinedUtc,
                OrganisationName = x.Organisation.Name,
                OrganisationType = x.Organisation.Type
            })
            .OrderByDescending(x => x.IsOwner)
            .ThenBy(x => x.JoinedUtc)
            .FirstOrDefaultAsync();

        var hasJobSeekerProfile = await _dbContext.JobSeekerProfiles
            .AnyAsync(x => x.UserId == user.Id);

        identity.AddClaim(new Claim(
            AppClaimTypes.HasJobSeekerProfile,
            hasJobSeekerProfile ? "true" : "false"));

        if (organisationMembership is not null)
        {
            var appType = organisationMembership.OrganisationType == OrganisationType.RecruiterAgency
                ? "recruiter"
                : "employer";

            identity.AddClaim(new Claim(AppClaimTypes.AppType, appType));
            identity.AddClaim(new Claim(AppClaimTypes.OrganisationId, organisationMembership.OrganisationId));
            identity.AddClaim(new Claim(AppClaimTypes.OrganisationName, organisationMembership.OrganisationName));
            identity.AddClaim(new Claim(
                AppClaimTypes.OrganisationType,
                organisationMembership.OrganisationType == OrganisationType.RecruiterAgency ? "recruiter" : "company"));
            identity.AddClaim(new Claim(
                AppClaimTypes.IsOrganisationOwner,
                organisationMembership.IsOwner ? "true" : "false"));

            if (organisationMembership.CompanyRole is not null)
            {
                identity.AddClaim(new Claim(
                    AppClaimTypes.CompanyRole,
                    organisationMembership.CompanyRole.Value.ToString()));
            }

            if (organisationMembership.RecruiterRole is not null)
            {
                identity.AddClaim(new Claim(
                    AppClaimTypes.RecruiterRole,
                    organisationMembership.RecruiterRole.Value.ToString()));
            }
        }
        else if (hasJobSeekerProfile)
        {
            identity.AddClaim(new Claim(AppClaimTypes.AppType, "jobseeker"));
        }

        return identity;
    }
}
