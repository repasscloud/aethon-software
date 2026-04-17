using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Api.Auth;
using Aethon.Api.Endpoints;
using Aethon.Api.Infrastructure;
using Aethon.Api.Infrastructure.Caching;
using Aethon.Api.Infrastructure.Email;
using Aethon.Api.Infrastructure.Files;
using Aethon.Api.Infrastructure.AtsMatch;
using Aethon.Api.Infrastructure.ResumeAnalysis;
using Aethon.Api.Infrastructure.Logging;
using Aethon.Application.Abstractions.AtsMatch;
using Aethon.Api.Infrastructure.Settings;
using Aethon.Api.Infrastructure.Stripe;
using Aethon.Api.Infrastructure.Syndication;
using Stripe;
using Aethon.Api.Infrastructure.Workers;
using Aethon.Api.Middleware;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Files;
using Aethon.Application.Abstractions.ResumeAnalysis;
using Aethon.Application.Abstractions.Logging;
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
using Microsoft.IdentityModel.Tokens;

// Load .env from the repository root (two levels up from the API project directory).
// Environment variables set by the OS / CI always take precedence over the .env file.
var envFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env");
if (!System.IO.File.Exists(envFile))
    envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!System.IO.File.Exists(envFile))
    envFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".env");
if (System.IO.File.Exists(envFile))
    DotNetEnv.Env.Load(envFile);

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
services.AddScoped<EmailOptionsResolver>();
services.AddScoped<IEmailService, MailerSendEmailService>();
services.AddScoped<IEmailTemplateService, EmailTemplateService>();
services.AddScoped<IAppSettings, AppSettingsService>();

services.Configure<ClaudeOptions>(configuration.GetSection("Claude"));
services.AddScoped<IResumeAnalysisService, ClaudeResumeAnalysisService>();
services.AddHostedService<ResumeAnalysisWorker>();

// ATS matching — Claude (paid) as the primary IAtsMatchingService, Ollama via accessor
services.Configure<OllamaOptions>(configuration.GetSection("Ollama"));
services.AddScoped<IAtsMatchingService, ClaudeAtsMatchingService>();
services.AddScoped<OllamaAtsMatchingService>();
services.AddScoped<OllamaAtsMatchingServiceAccessor>(sp =>
    new OllamaAtsMatchingServiceAccessor(sp.GetRequiredService<OllamaAtsMatchingService>()));
services.AddHostedService<AtsMatchClaudeWorker>();
services.AddHostedService<AtsMatchOllamaWorker>();

services.AddHttpClient();
services.AddHostedService<WebhookDeliveryWorker>();
services.AddHostedService<DomainVerificationWorker>();
services.AddHostedService<JobExpiryWorker>();
services.AddHostedService<IdentityVerificationWorker>();

services.AddScoped<ISystemSettingsService, SystemSettingsService>();
services.AddScoped<ISystemLogService, SystemLogService>();
services.AddScoped<IGoogleIndexingService, GoogleIndexingService>();

// Stripe
var stripeKey = configuration["Stripe:SecretKey"];
if (string.IsNullOrWhiteSpace(stripeKey))
    Console.Error.WriteLine("[STARTUP] WARNING: Stripe:SecretKey is empty. Checkout will fail. Check .env file.");
else
    Console.WriteLine($"[STARTUP] Stripe key loaded: {stripeKey[..7]}...");
StripeConfiguration.ApiKey = stripeKey;
services.AddScoped<IOrganisationAutoVerifier, OrganisationAutoVerifier>();
services.AddHttpClient("AutoVerifier", c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("Aethon-Verifier/1.0");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = true,
    MaxAutomaticRedirections = 5
});
services.AddScoped<StripeWebhookProcessor>();
services.AddScoped<StripeCheckoutService>();
services.AddScoped<JobPublishBillingService>();
services.AddScoped<JobAddonBillingService>();

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

    var seedAdminEmail    = app.Configuration["Seed:AdminEmail"];
    var seedAdminPassword = app.Configuration["Seed:AdminPassword"];

    foreach (var role in new[] { superAdminRole, adminRole, supportRole })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new ApplicationRole { Name = role });
    }

    if (string.IsNullOrWhiteSpace(seedAdminEmail) || string.IsNullOrWhiteSpace(seedAdminPassword))
    {
        app.Logger.LogWarning(
            "Seed:AdminEmail or Seed:AdminPassword is not configured — " +
            "skipping SuperAdmin seed. Set these via environment variables " +
            "SEED__ADMINEMAIL and SEED__ADMINPASSWORD.");
    }
    else
    {
        var adminUser = await userManager.FindByEmailAsync(seedAdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = seedAdminEmail,
                Email = seedAdminEmail,
                DisplayName = "Aethon Admin",
                UserType = Aethon.Shared.Enums.UserAccountType.Admin,
                IsEnabled = true,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(adminUser, seedAdminPassword);
            if (createResult.Succeeded)
                await userManager.AddToRoleAsync(adminUser, superAdminRole);
            else
                app.Logger.LogError("SuperAdmin seed failed: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
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
        },

        // ── Stripe ──────────────────────────────────────────────────────────
        new SystemSetting { Key = SystemSettingKeys.StripeSecretKey,                        Value = "", Description = "Stripe secret API key (sk_test_xxx or sk_live_xxx). Managed via admin UI.", UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripeWebhookSecret,                    Value = "", Description = "Stripe webhook signing secret (whsec_xxx). Managed via admin UI.", UpdatedUtc = DateTime.UtcNow },

        // Verification price IDs
        new SystemSetting { Key = SystemSettingKeys.StripePriceVerificationStandard,        Value = "", Description = "Stripe Price ID: Standard Employer Verification (A$49).",        UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceVerificationEnhanced,        Value = "", Description = "Stripe Price ID: Enhanced Trusted Employer (A$149).",             UpdatedUtc = DateTime.UtcNow },

        // Bundle price IDs
        new SystemSetting { Key = SystemSettingKeys.StripePriceBundleStandardVerificationPost, Value = "", Description = "Stripe Price ID: Bundle Standard Verification + First Standard Post (A$68).", UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceBundleEnhancedVerificationPost, Value = "", Description = "Stripe Price ID: Bundle Enhanced Verification + First Premium Post (A$208).", UpdatedUtc = DateTime.UtcNow },

        // Job Standard credit packs
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobStandard1x,  Value = "", Description = "Stripe Price ID: 1x Standard Job Post credit.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobStandard5x,  Value = "", Description = "Stripe Price ID: 5x Standard Job Post credits.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobStandard10x, Value = "", Description = "Stripe Price ID: 10x Standard Job Post credits.", UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobStandard20x, Value = "", Description = "Stripe Price ID: 20x Standard Job Post credits.", UpdatedUtc = DateTime.UtcNow },

        // Job Premium credit packs
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobPremium1x,   Value = "", Description = "Stripe Price ID: 1x Premium Job Post credit.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobPremium5x,   Value = "", Description = "Stripe Price ID: 5x Premium Job Post credits.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobPremium10x,  Value = "", Description = "Stripe Price ID: 10x Premium Job Post credits.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceJobPremium20x,  Value = "", Description = "Stripe Price ID: 20x Premium Job Post credits.",  UpdatedUtc = DateTime.UtcNow },

        // Sticky — verified org
        new SystemSetting { Key = SystemSettingKeys.StripePriceStickyVerified24h,  Value = "", Description = "Stripe Price ID: Sticky 24h — verified org (A$9).",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceStickyVerified7d,   Value = "", Description = "Stripe Price ID: Sticky 7d — verified org (A$39).",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceStickyVerified30d,  Value = "", Description = "Stripe Price ID: Sticky 30d — verified org (A$79).",  UpdatedUtc = DateTime.UtcNow },

        // Sticky — unverified org
        new SystemSetting { Key = SystemSettingKeys.StripePriceStickyUnverified24h,  Value = "", Description = "Stripe Price ID: Sticky 24h — unverified org (A$15).",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceStickyUnverified7d,   Value = "", Description = "Stripe Price ID: Sticky 7d — unverified org (A$49).",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceStickyUnverified30d,  Value = "", Description = "Stripe Price ID: Sticky 30d — unverified org (A$99).",  UpdatedUtc = DateTime.UtcNow },

        // Standard add-ons
        new SystemSetting { Key = SystemSettingKeys.StripePriceAddonHighlight,  Value = "", Description = "Stripe Price ID: Standard add-on — Highlight Colour (A$9).",       UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceAddonVideo,      Value = "", Description = "Stripe Price ID: Standard add-on — Video Embed (A$9).",             UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.StripePriceAddonAiMatching, Value = "", Description = "Stripe Price ID: Standard add-on — AI Candidate Matching (A$9).",   UpdatedUtc = DateTime.UtcNow },

        // ── Display Prices ──────────────────────────────────────────────────
        // Plain number strings (no currency symbol). Shown as "A$xx" in the UI.
        // Update via seed script or Admin → Stripe Products.
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobStandard1x,  Value = "19",  Description = "Display price: Standard job post — 1 credit.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobStandard5x,  Value = "",    Description = "Display price: Standard job post — 5 credits.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobStandard10x, Value = "",    Description = "Display price: Standard job post — 10 credits.", UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobStandard20x, Value = "",    Description = "Display price: Standard job post — 20 credits.", UpdatedUtc = DateTime.UtcNow },

        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobPremium1x,   Value = "69",  Description = "Display price: Premium job post — 1 credit.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobPremium5x,   Value = "",    Description = "Display price: Premium job post — 5 credits.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobPremium10x,  Value = "",    Description = "Display price: Premium job post — 10 credits.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceJobPremium20x,  Value = "",    Description = "Display price: Premium job post — 20 credits.",  UpdatedUtc = DateTime.UtcNow },

        new SystemSetting { Key = SystemSettingKeys.DisplayPriceVerificationStandard,             Value = "49",  Description = "Display price: Standard Employer Verification.",          UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceVerificationEnhanced,             Value = "149", Description = "Display price: Enhanced Trusted Employer Verification.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceBundleStandardVerificationPost,   Value = "68",  Description = "Display price: Standard Verification + first post bundle.", UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceBundleEnhancedVerificationPost,   Value = "208", Description = "Display price: Enhanced Verification + first post bundle.", UpdatedUtc = DateTime.UtcNow },

        new SystemSetting { Key = SystemSettingKeys.DisplayPriceAddonHighlight,  Value = "9", Description = "Display price: Add-on — Highlight Colour.",        UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceAddonVideo,      Value = "9", Description = "Display price: Add-on — Video Embed.",             UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceAddonAiMatching, Value = "9", Description = "Display price: Add-on — AI Candidate Matching.",   UpdatedUtc = DateTime.UtcNow },

        new SystemSetting { Key = SystemSettingKeys.DisplayPriceStickyVerified24h,   Value = "", Description = "Display price: Sticky 24h — verified org.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceStickyVerified7d,    Value = "", Description = "Display price: Sticky 7d — verified org.",    UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceStickyVerified30d,   Value = "", Description = "Display price: Sticky 30d — verified org.",   UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceStickyUnverified24h, Value = "", Description = "Display price: Sticky 24h — unverified org.", UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceStickyUnverified7d,  Value = "", Description = "Display price: Sticky 7d — unverified org.",  UpdatedUtc = DateTime.UtcNow },
        new SystemSetting { Key = SystemSettingKeys.DisplayPriceStickyUnverified30d, Value = "", Description = "Display price: Sticky 30d — unverified org.", UpdatedUtc = DateTime.UtcNow },

        // ── Site ──────────────────────────────────────────────────────────────
        new SystemSetting { Key = SystemSettingKeys.SiteBaseUrl, Value = (Environment.GetEnvironmentVariable("SiteBaseUrl") ?? "").TrimEnd('/'), Description = "Canonical public base URL of the web frontend (e.g. https://app.aethonsoftware.com). Used for sitemap generation and absolute URL construction.", UpdatedUtc = DateTime.UtcNow },

        // ── Import Feed ───────────────────────────────────────────────────────
        new SystemSetting
        {
            Key         = SystemSettingKeys.ImportApiKey,
            Value       = "",
            Description = "Long random API key for the external job import feed endpoint (/api/v1/import/jobs). " +
                          "Rotate via Admin → Settings → Import API Key. Falls back to IMPORT_API_KEY env var if empty.",
            UpdatedUtc  = DateTime.UtcNow
        },
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

// LinkedIn OAuth callback — mapped outside /api/v1 to match the registered redirect URI
app.MapGet("/signin-linkedin", async (
    HttpContext http,
    IConfiguration config,
    AethonDbContext db,
    IHttpClientFactory httpClientFactory,
    IFileStorageService fileStorage,
    CancellationToken ct) =>
{
    var query = http.Request.Query;
    var webBaseUrl = config["WebBaseUrl"] ?? "http://localhost:5200";
    var profileUrl = $"{webBaseUrl}/app/jobseeker/profile";

    if (query.TryGetValue("error", out var liError))
        return Results.Redirect($"{profileUrl}?linkedin_error={Uri.EscapeDataString(liError.ToString())}");

    var code = query["code"].ToString();
    var returnedState = query["state"].ToString();
    var expectedState = http.Request.Cookies["li_state"];
    var userIdStr = http.Request.Cookies["li_user_id"];

    if (string.IsNullOrWhiteSpace(code) ||
        string.IsNullOrWhiteSpace(expectedState) ||
        !string.Equals(expectedState, returnedState, StringComparison.Ordinal) ||
        !Guid.TryParse(userIdStr, out var userId))
    {
        return Results.Redirect($"{profileUrl}?linkedin_error=invalid_state");
    }

    http.Response.Cookies.Delete("li_state");
    http.Response.Cookies.Delete("li_user_id");

    var clientId = config["LinkedIn:ClientId"];
    var clientSecret = config["LinkedIn:ClientSecret"];
    var redirectUri = config["LinkedIn:RedirectUri"];

    if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(redirectUri))
        return Results.Redirect($"{profileUrl}?linkedin_error=not_configured");

    using var httpClient = httpClientFactory.CreateClient();

    // Exchange code for access token
    var tokenReq = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "authorization_code",
        ["code"] = code,
        ["client_id"] = clientId,
        ["client_secret"] = clientSecret,
        ["redirect_uri"] = redirectUri
    });

    using var tokenResp = await httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", tokenReq, ct);
    if (!tokenResp.IsSuccessStatusCode)
        return Results.Redirect($"{profileUrl}?linkedin_error=token_exchange_failed");

    var tokenJson = await tokenResp.Content.ReadAsStringAsync(ct);
    using var tokenDoc = JsonDocument.Parse(tokenJson);
    if (!tokenDoc.RootElement.TryGetProperty("access_token", out var atEl))
        return Results.Redirect($"{profileUrl}?linkedin_error=no_access_token");

    var accessToken = atEl.GetString()!;

    // Fetch LinkedIn profile (r_liteprofile scope — name + profile picture)
    using var profileReq = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/me?projection=(id,firstName,lastName,profilePicture(displayImage~:playableStreams))");
    profileReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    profileReq.Headers.Add("LinkedIn-Version", "202510.03");

    using var profileResp = await httpClient.SendAsync(profileReq, ct);
    if (!profileResp.IsSuccessStatusCode)
        return Results.Redirect($"{profileUrl}?linkedin_error=profile_fetch_failed");

    var profileJson = await profileResp.Content.ReadAsStringAsync(ct);
    using var profileDoc = JsonDocument.Parse(profileJson);
    var root = profileDoc.RootElement;

    var linkedInId = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;

    // Extract firstName / lastName from localized property
    static string? GetLocalized(JsonElement nameEl)
    {
        if (!nameEl.TryGetProperty("localized", out var loc)) return null;
        foreach (var prop in loc.EnumerateObject())
            return prop.Value.GetString();
        return null;
    }

    string? firstName = root.TryGetProperty("firstName", out var fnEl) ? GetLocalized(fnEl) : null;
    string? lastName = root.TryGetProperty("lastName", out var lnEl) ? GetLocalized(lnEl) : null;

    // Extract best-quality profile picture URL
    string? pictureUrl = null;
    if (root.TryGetProperty("profilePicture", out var ppEl) &&
        ppEl.TryGetProperty("displayImage~", out var displayEl) &&
        displayEl.TryGetProperty("elements", out var elements))
    {
        // Take the last (highest resolution) element
        JsonElement? best = null;
        foreach (var el in elements.EnumerateArray()) best = el;
        if (best.HasValue &&
            best.Value.TryGetProperty("identifiers", out var ids) &&
            ids.GetArrayLength() > 0)
        {
            pictureUrl = ids[0].TryGetProperty("identifier", out var urlEl) ? urlEl.GetString() : null;
        }
    }

    // Load or create the job seeker profile
    var profile = await db.JobSeekerProfiles
        .FirstOrDefaultAsync(p => p.UserId == userId, ct);

    var now = DateTime.UtcNow;

    if (profile is null)
    {
        profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedUtc = now,
            CreatedByUserId = userId
        };
        db.JobSeekerProfiles.Add(profile);
    }

    profile.LinkedInId = linkedInId;
    profile.LinkedInVerifiedAt = now;
    profile.UpdatedUtc = now;
    profile.UpdatedByUserId = userId;

    // Only update name if not locked
    if (!profile.IsNameLocked)
    {
        if (!string.IsNullOrWhiteSpace(firstName)) profile.FirstName = firstName;
        if (!string.IsNullOrWhiteSpace(lastName)) profile.LastName = lastName;
    }

    // Download and store profile picture
    if (!string.IsNullOrWhiteSpace(pictureUrl))
    {
        try
        {
            using var picResp = await httpClient.GetAsync(pictureUrl, ct);
            if (picResp.IsSuccessStatusCode)
            {
                var bytes = await picResp.Content.ReadAsByteArrayAsync(ct);
                var ext = picResp.Content.Headers.ContentType?.MediaType?.Contains("png") == true ? "png" : "jpg";
                var storagePath = await fileStorage.SaveAsync($"profile-picture-{userId}.{ext}", bytes);

                var storedFile = new StoredFile
                {
                    Id = Guid.NewGuid(),
                    FileName = $"profile-picture-{userId}.{ext}",
                    OriginalFileName = $"linkedin-profile-picture.{ext}",
                    ContentType = picResp.Content.Headers.ContentType?.MediaType ?? "image/jpeg",
                    LengthBytes = bytes.Length,
                    StorageProvider = "local",
                    StoragePath = storagePath,
                    UploadedByUserId = userId,
                    CreatedUtc = now,
                    CreatedByUserId = userId
                };
                db.StoredFiles.Add(storedFile);
                await db.SaveChangesAsync(ct);

                profile.ProfilePictureStoredFileId = storedFile.Id;
            }
        }
        catch
        {
            // Profile picture download failure is non-fatal
        }
    }

    await db.SaveChangesAsync(ct);

    return Results.Redirect($"{profileUrl}?linkedin_connected=1");
});

app.Run();
