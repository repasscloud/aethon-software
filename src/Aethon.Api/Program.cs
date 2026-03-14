using System.Security.Claims;
using Aethon.Data;
using Aethon.Data.Identity;
using Aethon.Data.Tenancy;
using Aethon.Shared.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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
    options.Cookie.Name = "__Host-Aethon.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7101",
                "https://app.aethon.software")
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

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

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

app.MapPost("/auth/logout-cookie", [Authorize] async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.NoContent();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await InitialiseDatabaseAsync(app);

app.Run();

static async Task InitialiseDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await db.Database.MigrateAsync();
        await SeedDevelopmentDataAsync(scope.ServiceProvider);
    }
}

static async Task SeedDevelopmentDataAsync(IServiceProvider services)
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