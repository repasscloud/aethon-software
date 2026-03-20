using Aethon.Api.Auth;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;

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
            JwtTokenService tokenService,
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

                var domainId = Guid.NewGuid();
                var orgId = Guid.NewGuid();

                var emailDomain = request.Email.Contains('@')
                    ? request.Email.Split('@')[1].Trim().ToLowerInvariant()
                    : string.Empty;

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
                    PrimaryDomainId = string.IsNullOrEmpty(emailDomain) ? null : domainId,
                    CreatedByUserId = user.Id,
                    CreatedUtc = now
                };

                db.Set<Organisation>().Add(org);

                if (!string.IsNullOrEmpty(emailDomain))
                {
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
                }

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

                await db.SaveChangesAsync();
            }

            return Results.Ok(new RegisterResultDto
            {
                Succeeded = true,
                RequiresEmailConfirmation = false,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                RegistrationType = normalizedType
            });
        });

        group.MapPost("/login", async (
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            LoginRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid credentials." });
            }

            var valid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!valid)
            {
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid credentials." });
            }

            var token = tokenService.GenerateToken(user);

            return Results.Ok(new AuthTokenResponse
            {
                Token = token
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

    public sealed class AuthTokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
