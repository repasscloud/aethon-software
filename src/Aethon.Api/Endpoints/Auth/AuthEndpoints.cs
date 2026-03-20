using Aethon.Api.Auth;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            UserManager<ApplicationUser> userManager,
            AethonDbContext db,
            RegisterRequest request) =>
        {
            var now = DateTime.UtcNow;
            var normalizedType = request.RegistrationType.Trim().ToLowerInvariant();

            var userType = normalizedType switch
            {
                "company" => UserAccountType.Company,
                "recruiter" => UserAccountType.RecruiterAgency,
                _ => UserAccountType.JobSeeker
            };

            var displayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim();

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = displayName,
                    UserType = userType
                };

                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                    return Results.ValidationProblem(errors);
                }

                if (normalizedType is "company" or "recruiter")
                {
                    var orgType = normalizedType == "company"
                        ? OrganisationType.Company
                        : OrganisationType.RecruiterAgency;

                    var orgId = Guid.NewGuid();

                    var emailDomain = request.Email.Contains('@')
                        ? request.Email.Split('@')[1].Trim().ToLowerInvariant()
                        : string.Empty;

                    // Save org first without PrimaryDomainId to avoid circular FK dependency
                    var org = new Organisation
                    {
                        Id = orgId,
                        Type = orgType,
                        Status = OrganisationStatus.Active,
                        ClaimStatus = OrganisationClaimStatus.NotApplicable,
                        Name = request.OrganisationName!.Trim(),
                        NormalizedName = request.OrganisationName!.Trim().ToUpperInvariant(),
                        IsPublicProfileEnabled = false,
                        IsVerified = false,
                        ClaimedByUserId = user.Id,
                        ClaimedUtc = now,
                        PrimaryDomainId = null,
                        CreatedByUserId = user.Id,
                        CreatedUtc = now
                    };

                    db.Set<Organisation>().Add(org);

                    var membership = new OrganisationMembership
                    {
                        Id = Guid.NewGuid(),
                        OrganisationId = orgId,
                        UserId = user.Id,
                        Status = MembershipStatus.Active,
                        IsOwner = true,
                        CompanyRole = orgType == OrganisationType.Company ? CompanyRole.Owner : null,
                        RecruiterRole = orgType == OrganisationType.RecruiterAgency ? RecruiterRole.Owner : null,
                        JoinedUtc = now,
                        CreatedByUserId = user.Id,
                        CreatedUtc = now
                    };

                    db.Set<OrganisationMembership>().Add(membership);

                    // First save: org + membership (no circular dep)
                    await db.SaveChangesAsync();

                    // Now add the domain and link it back
                    if (!string.IsNullOrEmpty(emailDomain))
                    {
                        var domainId = Guid.NewGuid();

                        var domain = new OrganisationDomain
                        {
                            Id = domainId,
                            OrganisationId = orgId,
                            Domain = emailDomain,
                            NormalizedDomain = emailDomain.ToUpperInvariant(),
                            IsPrimary = true,
                            Status = DomainStatus.Pending,
                            VerificationMethod = DomainVerificationMethod.None,
                            TrustLevel = DomainTrustLevel.Low,
                            CreatedByUserId = user.Id,
                            CreatedUtc = now
                        };

                        db.Set<OrganisationDomain>().Add(domain);
                        await db.SaveChangesAsync();

                        // Second save: update org's PrimaryDomainId now that domain exists
                        await db.Set<Organisation>()
                            .Where(o => o.Id == orgId)
                            .ExecuteUpdateAsync(s => s.SetProperty(o => o.PrimaryDomainId, domainId));
                    }
                }

                await transaction.CommitAsync();

                return Results.Ok(new RegisterResultDto
                {
                    Succeeded = true,
                    RequiresEmailConfirmation = false,
                    Email = request.Email,
                    DisplayName = displayName,
                    RegistrationType = normalizedType
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        group.MapPost("/login", async (
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            AethonDbContext db,
            LoginRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid email or password." });
            }

            var token = tokenService.GenerateToken(user);

            // Determine app type from user type and org membership
            var appType = user.UserType switch
            {
                UserAccountType.JobSeeker => "jobseeker",
                UserAccountType.Company => "employer",
                UserAccountType.RecruiterAgency => "recruiter",
                _ => "jobseeker"
            };

            string? organisationId = null;
            string? organisationName = null;
            string? organisationType = null;
            string? companyRole = null;
            string? recruiterRole = null;
            bool isOwner = false;

            if (user.UserType is UserAccountType.Company or UserAccountType.RecruiterAgency)
            {
                var membership = await db.Set<OrganisationMembership>()
                    .Include(m => m.Organisation)
                    .Where(m => m.UserId == user.Id && m.Status == MembershipStatus.Active)
                    .OrderByDescending(m => m.IsOwner)
                    .FirstOrDefaultAsync();

                if (membership is not null)
                {
                    organisationId = membership.OrganisationId.ToString();
                    organisationName = membership.Organisation.Name;
                    organisationType = membership.Organisation.Type.ToString().ToLowerInvariant();
                    isOwner = membership.IsOwner;
                    companyRole = membership.CompanyRole?.ToString();
                    recruiterRole = membership.RecruiterRole?.ToString();
                }
            }

            return Results.Ok(new LoginResponse
            {
                Token = token,
                UserId = user.Id.ToString(),
                Email = user.Email!,
                DisplayName = user.DisplayName,
                AppType = appType,
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                OrganisationType = organisationType,
                CompanyRole = companyRole,
                RecruiterRole = recruiterRole,
                IsOrganisationOwner = isOwner
            });
        });
    }

    public sealed class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RegistrationType { get; set; } = string.Empty;
        public string? OrganisationName { get; set; }
    }

    public sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AppType { get; set; } = string.Empty;
        public string? OrganisationId { get; set; }
        public string? OrganisationName { get; set; }
        public string? OrganisationType { get; set; }
        public string? CompanyRole { get; set; }
        public string? RecruiterRole { get; set; }
        public bool IsOrganisationOwner { get; set; }
    }
}
