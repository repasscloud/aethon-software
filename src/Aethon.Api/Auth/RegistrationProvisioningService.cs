using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Data.Tenancy;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Auth;

public sealed class RegistrationProvisioningService : IRegistrationProvisioningService
{
    private static readonly HashSet<string> PublicEmailDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com",
        "googlemail.com",
        "outlook.com",
        "hotmail.com",
        "live.com",
        "msn.com",
        "yahoo.com",
        "icloud.com",
        "me.com",
        "aol.com",
        "proton.me",
        "protonmail.com"
    };

    private readonly AethonDbContext _dbContext;

    public RegistrationProvisioningService(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegistrationProvisioningResult> ProvisionAsync(
        ApplicationUser user,
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var registrationType = request.RegistrationType.Trim().ToLowerInvariant();
        var email = request.Email.Trim();
        var emailDomain = ExtractDomain(email);

        if (string.IsNullOrWhiteSpace(emailDomain))
        {
            return Failure(nameof(RegisterRequestDto.Email), "Email domain could not be determined.");
        }

        var defaultTenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Slug == "default" && x.IsEnabled, cancellationToken);

        if (defaultTenant is null)
        {
            return Failure(string.Empty, "Default tenant is not configured.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var hasTenantMembership = await _dbContext.UserTenantMemberships
            .AnyAsync(x => x.UserId == user.Id && x.TenantId == defaultTenant.Id, cancellationToken);

        if (!hasTenantMembership)
        {
            _dbContext.UserTenantMemberships.Add(new UserTenantMembership
            {
                UserId = user.Id,
                TenantId = defaultTenant.Id,
                RoleCode = GetTenantRoleCode(registrationType),
                IsDefault = true
            });
        }

        switch (registrationType)
        {
            case "jobseeker":
            {
                var existingProfile = await _dbContext.JobSeekerProfiles
                    .AnyAsync(x => x.UserId == user.Id, cancellationToken);

                if (!existingProfile)
                {
                    _dbContext.JobSeekerProfiles.Add(new JobSeekerProfile
                    {
                        Id = NewStringId(),
                        UserId = user.Id,
                        OpenToWork = true,
                        CreatedUtc = DateTime.UtcNow
                    });
                }

                break;
            }

            case "company":
            case "recruiter":
            {
                if (PublicEmailDomains.Contains(emailDomain))
                {
                    return Failure(nameof(RegisterRequestDto.Email), "A business email address is required for this registration type.");
                }

                var normalizedDomain = emailDomain.ToUpperInvariant();
                var existingDomain = await _dbContext.OrganisationDomains
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.NormalizedDomain == normalizedDomain, cancellationToken);

                if (existingDomain is not null)
                {
                    return Failure(nameof(RegisterRequestDto.Email), "An organisation already exists for this email domain.");
                }

                var organisationType = registrationType == "company"
                    ? OrganisationType.Company
                    : OrganisationType.RecruiterAgency;

                var organisation = new Organisation
                {
                    Id = NewStringId(),
                    Type = organisationType,
                    Status = OrganisationStatus.Active,
                    ClaimStatus = OrganisationClaimStatus.Claimed,
                    Name = request.OrganisationName!.Trim(),
                    NormalizedName = request.OrganisationName.Trim().ToUpperInvariant(),
                    IsProvisionedByRecruiter = false,
                    ClaimedByUserId = user.Id.ToString(),
                    ClaimedUtc = DateTime.UtcNow,
                    PrimaryContactName = user.DisplayName,
                    PrimaryContactEmail = email,
                    CreatedUtc = DateTime.UtcNow
                };

                _dbContext.Organisations.Add(organisation);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var organisationDomain = new OrganisationDomain
                {
                    Id = NewStringId(),
                    OrganisationId = organisation.Id,
                    Domain = emailDomain,
                    NormalizedDomain = normalizedDomain,
                    IsPrimary = true,
                    Status = DomainStatus.Verified,
                    VerificationMethod = DomainVerificationMethod.Email,
                    TrustLevel = DomainTrustLevel.Medium,
                    VerificationEmailAddress = email,
                    VerificationRequestedUtc = DateTime.UtcNow,
                    VerifiedUtc = DateTime.UtcNow,
                    VerifiedByUserId = user.Id.ToString(),
                    CreatedUtc = DateTime.UtcNow
                };

                _dbContext.OrganisationDomains.Add(organisationDomain);
                await _dbContext.SaveChangesAsync(cancellationToken);

                organisation.PrimaryDomainId = organisationDomain.Id;

                var membership = new OrganisationMembership
                {
                    Id = NewStringId(),
                    OrganisationId = organisation.Id,
                    UserId = user.Id,
                    Status = MembershipStatus.Active,
                    IsOwner = true,
                    JoinedUtc = DateTime.UtcNow,
                    CreatedUtc = DateTime.UtcNow,
                    CompanyRole = organisationType == OrganisationType.Company ? CompanyRole.Owner : null,
                    RecruiterRole = organisationType == OrganisationType.RecruiterAgency ? RecruiterRole.Owner : null
                };

                _dbContext.OrganisationMemberships.Add(membership);
                await _dbContext.SaveChangesAsync(cancellationToken);

                break;
            }

            default:
                return Failure(nameof(RegisterRequestDto.RegistrationType), "Registration type is invalid.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RegistrationProvisioningResult
        {
            Succeeded = true
        };
    }

    private static string GetTenantRoleCode(string registrationType)
    {
        return registrationType switch
        {
            "company" => "CompanyOwner",
            "recruiter" => "RecruiterOwner",
            "jobseeker" => "JobSeeker",
            _ => "User"
        };
    }

    private static string ExtractDomain(string email)
    {
        var atIndex = email.LastIndexOf('@');
        if (atIndex < 0 || atIndex == email.Length - 1)
        {
            return string.Empty;
        }

        return email[(atIndex + 1)..].Trim().ToLowerInvariant();
    }

    private static string NewStringId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private static RegistrationProvisioningResult Failure(string key, string message)
    {
        return new RegistrationProvisioningResult
        {
            Succeeded = false,
            Errors = new Dictionary<string, string[]>
            {
                [key] = [message]
            }
        };
    }
}
