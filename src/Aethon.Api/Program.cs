using System.Security.Claims;
using Aethon.Data;
using Aethon.Data.Identity;
using Aethon.Data.Tenancy;
using Aethon.Shared.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
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
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AethonDbContext>()
    .AddApiEndpoints()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppUserClaimsPrincipalFactory>();

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
        Roles = user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
    });
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await InitialiseDatabaseAsync(app);

app.Run();

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
                membership => membership.TenantId,
                tenant => tenant.Id,
                (membership, tenant) => new { membership, tenant })
            .FirstOrDefaultAsync();

        if (membership is not null)
        {
            identity.AddClaim(new Claim(AppClaimTypes.TenantId, membership.tenant.Id.ToString()));
            identity.AddClaim(new Claim(AppClaimTypes.TenantSlug, membership.tenant.Slug));
            identity.AddClaim(new Claim(ClaimTypes.Role, membership.membership.RoleCode));
        }

        return identity;
    }
}