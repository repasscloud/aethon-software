using System.Text.Json.Serialization;
using Aethon.Api.Auth;
using Aethon.Api.Endpoints;
using Aethon.Api.Infrastructure;
using Aethon.Api.Infrastructure.Caching;
using Aethon.Api.Infrastructure.Email;
using Aethon.Api.Infrastructure.Files;
using Aethon.Api.Infrastructure.ResumeAnalysis;
using Aethon.Api.Infrastructure.Settings;
using Aethon.Api.Infrastructure.Syndication;
using Aethon.Api.Infrastructure.Workers;
using Aethon.Api.Middleware;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Files;
using Aethon.Application.Abstractions.ResumeAnalysis;
using Aethon.Application.Abstractions.Settings;
using Aethon.Application.Abstractions.Syndication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Validation;
using Aethon.Application.DependencyInjection;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<AethonDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
});

services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AethonDbContext>()
    .AddDefaultTokenProviders();

services.AddAethonAuth(configuration);

services.AddMemoryCache();
services.AddSingleton<IAppCache, MemoryAppCache>();

services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
services.AddScoped<IFileStorageService, LocalFileStorageService>();
services.AddScoped<JwtTokenService>();

services.Configure<EmailOptions>(configuration.GetSection("Email"));
services.AddScoped<IEmailService, MailerSendEmailService>();
services.AddScoped<IAppSettings, AppSettingsService>();

services.Configure<ClaudeOptions>(configuration.GetSection("Claude"));
services.AddScoped<IResumeAnalysisService, ClaudeResumeAnalysisService>();
services.AddHostedService<ResumeAnalysisWorker>();

services.AddHttpClient();
services.AddHostedService<WebhookDeliveryWorker>();
services.AddHostedService<DomainVerificationWorker>();
services.AddHostedService<JobExpiryWorker>();

services.AddScoped<ISystemSettingsService, SystemSettingsService>();
services.AddScoped<IGoogleIndexingService, GoogleIndexingService>();

services.AddApplicationServices();
services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

services.AddHealthChecks();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply pending EF migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();
    db.Database.Migrate();

    // Seed SuperAdmin role and admin account
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string superAdminRole = "SuperAdmin";
    const string adminRole = "Admin";
    const string supportRole = "Support";
    const string adminEmail = "aethon@localhost.com";
    const string adminPassword = "Aethon@Admin2026!";

    foreach (var role in new[] { superAdminRole, adminRole, supportRole })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new ApplicationRole { Name = role });
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Aethon Admin",
            UserType = Aethon.Shared.Enums.UserAccountType.Admin,
            IsEnabled = true,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, superAdminRole);
    }
    else
    {
        // Fix existing seed that may have wrong UserType
        if (adminUser.UserType != Aethon.Shared.Enums.UserAccountType.Admin)
        {
            adminUser.UserType = Aethon.Shared.Enums.UserAccountType.Admin;
            await userManager.UpdateAsync(adminUser);
        }
        if (!await userManager.IsInRoleAsync(adminUser, superAdminRole))
            await userManager.AddToRoleAsync(adminUser, superAdminRole);
    }

    // Seed SystemSettings defaults
    var settingsToSeed = new[]
    {
        new SystemSetting
        {
            Key = SystemSettingKeys.GoogleIndexingEnabled,
            Value = "false",
            Description = "Enable Google Indexing API syndication for published jobs.",
            UpdatedUtc = DateTime.UtcNow
        },
        new SystemSetting
        {
            Key = SystemSettingKeys.GoogleIndexingServiceAccount,
            Value = "",
            Description = "Google Service Account JSON key for the Indexing API (SuperAdmin only).",
            UpdatedUtc = DateTime.UtcNow
        }
    };

    foreach (var setting in settingsToSeed)
    {
        if (!await db.SystemSettings.AnyAsync(s => s.Key == setting.Key))
            db.SystemSettings.Add(setting);
    }
    await db.SaveChangesAsync();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapApplicationEndpoints();

app.Run();
