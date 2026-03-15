using Aethon.Data;
using Aethon.Data.Identity;
using Aethon.Data.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Infrastructure;

public static class DatabaseStartup
{
    public static async Task InitialiseDatabaseAsync(WebApplication app)
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

    private static async Task SeedInitialDataAsync(IServiceProvider services)
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
}