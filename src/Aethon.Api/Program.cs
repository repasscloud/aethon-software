using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Aethon.Api.Auth;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Enums;
using Aethon.Data.Identity;
using Aethon.Data.Tenancy;
using Aethon.Shared.Auth;
using Aethon.Shared.Organisations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"] ?? "/keys"))
    .SetApplicationName(builder.Configuration["DataProtection:ApplicationName"] ?? "Aethon");

builder.Services.AddDbContext<AethonDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddIdentityCookies();

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("ApiPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 12;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AethonDbContext>()
    .AddApiEndpoints()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppUserClaimsPrincipalFactory>();
builder.Services.AddScoped<IRegistrationProvisioningService, RegistrationProvisioningService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Aethon.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Path = "/";
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

var corsOrigins =
    builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:5101", "https://localhost:7101", "https://app.aethon.software"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue("EnableHttpsRedirection", app.Environment.IsDevelopment()))
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

app.MapPost("/auth/register", async (
    RegisterRequestDto request,
    UserManager<ApplicationUser> userManager,
    IRegistrationProvisioningService registrationProvisioningService,
    IConfiguration configuration,
    ILoggerFactory loggerFactory) =>
{
    var validationErrors = ValidateRegisterRequest(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var existingUser = await userManager.FindByEmailAsync(request.Email.Trim());
    if (existingUser is not null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(RegisterRequestDto.Email)] = ["An account with this email address already exists."]
        });
    }

    var email = request.Email.Trim();
    var firstName = request.FirstName.Trim();
    var lastName = request.LastName.Trim();
    var displayName = $"{firstName} {lastName}".Trim();

    var user = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = email,
        Email = email,
        DisplayName = displayName,
        EmailConfirmed = false,
        IsEnabled = true
    };

    var createResult = await userManager.CreateAsync(user, request.Password);
    if (!createResult.Succeeded)
    {
        return Results.ValidationProblem(createResult.Errors
            .GroupBy(x => x.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                ? nameof(RegisterRequestDto.Password)
                : nameof(RegisterRequestDto.Email))
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.Description).ToArray()));
    }

    var provisioningResult = await registrationProvisioningService.ProvisionAsync(user, request);
    if (!provisioningResult.Succeeded)
    {
        await userManager.DeleteAsync(user);
        return Results.ValidationProblem(provisioningResult.Errors);
    }

    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

    var apiBaseUrl = (configuration["Api:BaseUrl"] ?? configuration["ASPNETCORE_URLS"]?.Split(';').FirstOrDefault() ?? "http://localhost:5201").TrimEnd('/');
    var confirmationUrl = QueryHelpers.AddQueryString(
        $"{apiBaseUrl}/auth/confirm-email",
        new Dictionary<string, string?>
        {
            ["userId"] = user.Id.ToString(),
            ["token"] = token
        });

    var logger = loggerFactory.CreateLogger("Aethon.Registration");
    logger.LogInformation("Email confirmation link for {Email}: {ConfirmationUrl}", email, confirmationUrl);

    return Results.Ok(new RegisterResultDto
    {
        Succeeded = true,
        RequiresEmailConfirmation = true,
        Email = email,
        DisplayName = displayName,
        RegistrationType = request.RegistrationType.Trim().ToLowerInvariant()
    });
})
.AllowAnonymous();

app.MapGet("/auth/confirm-email", async (
    string userId,
    string token,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) =>
{
    var webBaseUrl = (configuration["Web:BaseUrl"] ?? "http://localhost:5101").TrimEnd('/');

    if (!Guid.TryParse(userId, out var parsedUserId))
    {
        return Results.Redirect($"{webBaseUrl}/register/confirmed?status=invalid");
    }

    var user = await userManager.FindByIdAsync(parsedUserId.ToString());
    if (user is null)
    {
        return Results.Redirect($"{webBaseUrl}/register/confirmed?status=invalid");
    }

    var result = await userManager.ConfirmEmailAsync(user, token);
    if (!result.Succeeded)
    {
        return Results.Redirect($"{webBaseUrl}/register/confirmed?status=invalid");
    }

    return Results.Redirect($"{webBaseUrl}/register/confirmed?status=success");
})
.AllowAnonymous();

app.MapPost("/auth/browser-login", async (
    HttpContext httpContext,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) =>
{
    var form = await httpContext.Request.ReadFormAsync();

    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();
    var rememberMe = string.Equals(form["rememberMe"], "on", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(form["rememberMe"], "true", StringComparison.OrdinalIgnoreCase);

    var webBaseUrl = (configuration["Web:BaseUrl"] ?? "http://localhost:5101").TrimEnd('/');
    var returnPath = NormaliseReturnPath(form["returnPath"].ToString());

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        return Results.Redirect(BuildLoginRedirect(webBaseUrl, returnPath, "Please enter your email and password."));
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user is null || !user.IsEnabled)
    {
        return Results.Redirect(BuildLoginRedirect(webBaseUrl, returnPath, "Invalid email or password."));
    }

    var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

    if (result.IsNotAllowed)
    {
        return Results.Redirect(BuildLoginRedirect(webBaseUrl, returnPath, "Please confirm your email address before signing in."));
    }

    if (!result.Succeeded)
    {
        return Results.Redirect(BuildLoginRedirect(webBaseUrl, returnPath, "Invalid email or password."));
    }

    return Results.Redirect($"{webBaseUrl}{returnPath}");
})
.DisableAntiforgery();

app.MapPost("/auth/browser-logout", async (
    HttpContext httpContext,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration) =>
{
    var form = await httpContext.Request.ReadFormAsync();

    var webBaseUrl = (configuration["Web:BaseUrl"] ?? "http://localhost:5101").TrimEnd('/');
    var returnPath = NormaliseReturnPath(form["returnPath"].ToString(), "/login");

    await signInManager.SignOutAsync();

    return Results.Redirect($"{webBaseUrl}{returnPath}");
})
.DisableAntiforgery();

app.MapGet("/auth/me", [Authorize] (ClaimsPrincipal user) =>
{
    return Results.Ok(new AuthResultDto
    {
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
        Email = user.FindFirstValue(ClaimTypes.Email),
        DisplayName = user.FindFirstValue(AppClaimTypes.DisplayName),
        TenantId = user.FindFirstValue(AppClaimTypes.TenantId),
        TenantSlug = user.FindFirstValue(AppClaimTypes.TenantSlug),
        AppType = user.FindFirstValue(AppClaimTypes.AppType),
        OrganisationId = user.FindFirstValue(AppClaimTypes.OrganisationId),
        OrganisationName = user.FindFirstValue(AppClaimTypes.OrganisationName),
        OrganisationType = user.FindFirstValue(AppClaimTypes.OrganisationType),
        IsOrganisationOwner = string.Equals(
            user.FindFirstValue(AppClaimTypes.IsOrganisationOwner),
            "true",
            StringComparison.OrdinalIgnoreCase),
        CompanyRole = user.FindFirstValue(AppClaimTypes.CompanyRole),
        RecruiterRole = user.FindFirstValue(AppClaimTypes.RecruiterRole),
        HasJobSeekerProfile = string.Equals(
            user.FindFirstValue(AppClaimTypes.HasJobSeekerProfile),
            "true",
            StringComparison.OrdinalIgnoreCase),
        Roles = user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
    });
});

app.MapGet("/org/me/members", [Authorize] async (
    ClaimsPrincipal user,
    AethonDbContext dbContext) =>
{
    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var organisationId = user.FindFirstValue(AppClaimTypes.OrganisationId);

    if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(organisationId))
    {
        return Results.BadRequest();
    }

    var organisation = await dbContext.Organisations
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == organisationId);

    if (organisation is null)
    {
        return Results.NotFound();
    }

    var currentMembership = await dbContext.OrganisationMemberships
        .AsNoTracking()
        .FirstOrDefaultAsync(x =>
            x.OrganisationId == organisationId &&
            x.UserId == userId &&
            x.Status == MembershipStatus.Active);

    if (currentMembership is null)
    {
        return Results.Forbid();
    }

    var members = await dbContext.OrganisationMemberships
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

    var pendingInvites = await dbContext.OrganisationInvitations
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

    return Results.Ok(new OrganisationMembersResponseDto
    {
        OrganisationId = organisation.Id,
        OrganisationName = organisation.Name,
        OrganisationType = organisation.Type == OrganisationType.RecruiterAgency ? "recruiter" : "company",
        IsOwner = currentMembership.IsOwner,
        CanInvite = currentMembership.IsOwner,
        Members = members,
        PendingInvites = pendingInvites
    });
});

app.MapPost("/org/me/invites", [Authorize] async (
    ClaimsPrincipal user,
    CreateOrganisationInviteRequestDto request,
    AethonDbContext dbContext) =>
{
    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var organisationId = user.FindFirstValue(AppClaimTypes.OrganisationId);

    if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(organisationId))
    {
        return Results.BadRequest();
    }

    var validationErrors = ValidateInviteRequest(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var membership = await dbContext.OrganisationMemberships
        .AsNoTracking()
        .Include(x => x.Organisation)
        .FirstOrDefaultAsync(x =>
            x.OrganisationId == organisationId &&
            x.UserId == userId &&
            x.Status == MembershipStatus.Active);

    if (membership is null || !membership.IsOwner)
    {
        return Results.Forbid();
    }

    var inviteEmail = request.Email.Trim();
    var normalizedEmail = inviteEmail.ToUpperInvariant();
    var emailDomain = inviteEmail[(inviteEmail.LastIndexOf('@') + 1)..].Trim().ToLowerInvariant();

    var existingPendingInvite = await dbContext.OrganisationInvitations
        .FirstOrDefaultAsync(x =>
            x.OrganisationId == organisationId &&
            x.NormalizedEmail == normalizedEmail &&
            x.Status == InvitationStatus.Pending);

    if (existingPendingInvite is not null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(CreateOrganisationInviteRequestDto.Email)] = ["A pending invite already exists for this email address."]
        });
    }

    var existingMembership = await dbContext.OrganisationMemberships
        .AnyAsync(x =>
            x.OrganisationId == organisationId &&
            x.User.Email != null &&
            x.User.NormalizedEmail == normalizedEmail &&
            x.Status == MembershipStatus.Active);

    if (existingMembership)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
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
            return Results.ValidationProblem(new Dictionary<string, string[]>
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
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateOrganisationInviteRequestDto.RecruiterRole)] = ["A valid recruiter role is required."]
            });
        }

        recruiterRole = parsedRecruiterRole;
    }

    var invite = new OrganisationInvitation
    {
        Id = Guid.NewGuid().ToString("N"),
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
        CreatedByUserId = userId.ToString()
    };

    dbContext.OrganisationInvitations.Add(invite);
    await dbContext.SaveChangesAsync();

    return Results.Ok(new OrganisationInviteDto
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
});

app.MapPost("/org/invites/accept", [Authorize] async (
    ClaimsPrincipal user,
    AcceptOrganisationInviteRequestDto request,
    AethonDbContext dbContext) =>
{
    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = user.FindFirstValue(ClaimTypes.Email);

    if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest();
    }

    var normalizedEmail = email.ToUpperInvariant();

    var invite = await dbContext.OrganisationInvitations
        .FirstOrDefaultAsync(x =>
            x.Token == request.Token &&
            x.Status == InvitationStatus.Pending);

    if (invite is null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(AcceptOrganisationInviteRequestDto.Token)] = ["Invitation was not found."]
        });
    }

    if (invite.ExpiresUtc < DateTime.UtcNow)
    {
        invite.Status = InvitationStatus.Expired;
        invite.UpdatedUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(AcceptOrganisationInviteRequestDto.Token)] = ["Invitation has expired."]
        });
    }

    if (!string.Equals(invite.NormalizedEmail, normalizedEmail, StringComparison.Ordinal))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(AcceptOrganisationInviteRequestDto.Token)] = ["This invitation does not belong to your signed-in account."]
        });
    }

    var alreadyMember = await dbContext.OrganisationMemberships
        .AnyAsync(x =>
            x.OrganisationId == invite.OrganisationId &&
            x.UserId == userId &&
            x.Status == MembershipStatus.Active);

    if (!alreadyMember)
    {
        dbContext.OrganisationMemberships.Add(new OrganisationMembership
        {
            Id = Guid.NewGuid().ToString("N"),
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
    invite.AcceptedByUserId = userId.ToString();
    invite.AcceptedUtc = DateTime.UtcNow;
    invite.UpdatedUtc = DateTime.UtcNow;

    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await InitialiseDatabaseAsync(app);

app.Run();

static Dictionary<string, string[]> ValidateInviteRequest(CreateOrganisationInviteRequestDto request)
{
    var context = new ValidationContext(request);
    var results = new List<ValidationResult>();

    Validator.TryValidateObject(request, context, results, validateAllProperties: true);

    return results
        .SelectMany(x =>
        {
            var memberNames = x.MemberNames.Any() ? x.MemberNames : [string.Empty];
            return memberNames.Select(memberName => new
            {
                Key = memberName,
                Message = x.ErrorMessage ?? "Validation error."
            });
        })
        .GroupBy(x => x.Key)
        .ToDictionary(
            x => x.Key,
            x => x.Select(y => y.Message).Distinct().ToArray());
}

static Dictionary<string, string[]> ValidateRegisterRequest(RegisterRequestDto request)
{
    var context = new ValidationContext(request);
    var results = new List<ValidationResult>();

    Validator.TryValidateObject(request, context, results, validateAllProperties: true);

    return results
        .SelectMany(x =>
        {
            var memberNames = x.MemberNames.Any() ? x.MemberNames : [string.Empty];
            return memberNames.Select(memberName => new
            {
                Key = memberName,
                Message = x.ErrorMessage ?? "Validation error."
            });
        })
        .GroupBy(x => x.Key)
        .ToDictionary(
            x => x.Key,
            x => x.Select(y => y.Message).Distinct().ToArray());
}

static bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/auth")
           || request.Path.StartsWithSegments("/login")
           || request.Path.StartsWithSegments("/register")
           || request.Path.StartsWithSegments("/manage")
           || request.Path.StartsWithSegments("/health");
}

static string NormaliseReturnPath(string? value, string fallback = "/")
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return fallback;
    }

    if (!value.StartsWith('/'))
    {
        value = "/" + value;
    }

    if (value.StartsWith("//"))
    {
        return fallback;
    }

    if (Uri.TryCreate(value, UriKind.Absolute, out _))
    {
        return fallback;
    }

    return value;
}

static string BuildLoginRedirect(string webBaseUrl, string returnPath, string error)
{
    var loginUrl = $"{webBaseUrl}/login";

    var query = new Dictionary<string, string?>
    {
        ["ReturnUrl"] = returnPath,
        ["error"] = error
    };

    return QueryHelpers.AddQueryString(loginUrl, query);
}

static async Task InitialiseDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();

    var shouldApplyMigrations = app.Configuration.GetValue("ApplyMigrationsOnStartup", app.Environment.IsDevelopment());
    var shouldSeed = app.Configuration.GetValue("SeedOnStartup", app.Environment.IsDevelopment());

    if (shouldApplyMigrations)
    {
        await db.Database.MigrateAsync();
    }

    if (shouldSeed)
    {
        await SeedInitialDataAsync(scope.ServiceProvider);
    }
}

static async Task SeedInitialDataAsync(IServiceProvider services)
{
    var db = services.GetRequiredService<AethonDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    const string tenantAdminRole = "TenantAdmin";
    const string defaultTenantSlug = "default";
    const string adminEmail = "admin@aethon.local";
    const string adminPassword = "ChangeThis123!";

    if (!await roleManager.RoleExistsAsync(tenantAdminRole))
    {
        var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = tenantAdminRole,
            NormalizedName = tenantAdminRole.ToUpperInvariant()
        });

        if (!createRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create role '{tenantAdminRole}': {string.Join("; ", createRoleResult.Errors.Select(x => x.Description))}");
        }
    }

    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == defaultTenantSlug);
    if (tenant is null)
    {
        tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Default Tenant",
            Slug = defaultTenantSlug,
            IsEnabled = true
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
    }

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            NormalizedUserName = adminEmail.ToUpperInvariant(),
            NormalizedEmail = adminEmail.ToUpperInvariant(),
            DisplayName = "Aethon Admin",
            EmailConfirmed = true,
            IsEnabled = true
        };

        var createUserResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createUserResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create admin user: {string.Join("; ", createUserResult.Errors.Select(x => x.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(admin, tenantAdminRole))
    {
        var addToRoleResult = await userManager.AddToRoleAsync(admin, tenantAdminRole);
        if (!addToRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to add admin user to role '{tenantAdminRole}': {string.Join("; ", addToRoleResult.Errors.Select(x => x.Description))}");
        }
    }

    var membershipExists = await db.UserTenantMemberships.AnyAsync(x =>
        x.UserId == admin.Id &&
        x.TenantId == tenant.Id);

    if (!membershipExists)
    {
        db.UserTenantMemberships.Add(new UserTenantMembership
        {
            UserId = admin.Id,
            TenantId = tenant.Id,
            RoleCode = tenantAdminRole,
            IsDefault = true
        });

        await db.SaveChangesAsync();
    }
}

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

        var membership = await _dbContext.UserTenantMemberships
            .Where(x => x.UserId == user.Id && x.IsDefault)
            .Join(
                _dbContext.Tenants,
                userTenantMembership => userTenantMembership.TenantId,
                tenant => tenant.Id,
                (userTenantMembership, tenant) => new { userTenantMembership, tenant })
            .FirstOrDefaultAsync();

        if (membership is not null)
        {
            identity.AddClaim(new Claim(AppClaimTypes.TenantId, membership.tenant.Id.ToString()));
            identity.AddClaim(new Claim(AppClaimTypes.TenantSlug, membership.tenant.Slug));
            identity.AddClaim(new Claim(ClaimTypes.Role, membership.userTenantMembership.RoleCode));
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

