using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Aethon.Application.Abstractions.Settings;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Import.Commands.ImportJobs;

/// <summary>
/// Handles the ingestion of one or more jobs from an external import feed.
///
/// Per-job logic:
///   1. Validates the API key.
///   2. Finds or creates the import organisation (ImportOrg_{sourceSite}_{companyName}).
///   3. Finds or creates a system user bound to that organisation.
///   4. Upserts the job keyed on (sourceSite + externalId) via ExternalReference.
///      - If the record exists: updates all mutable fields.
///        If publishedUtc moves to an earlier date, PostingExpiresUtc is always set to publishedUtc + 30 days.
///      - If the record does not exist: creates it as Published / Public / Imported tier, never touching billing credits.
/// </summary>
public sealed class ImportJobsHandler
{
    private readonly AethonDbContext _db;
    private readonly ISystemSettingsService _settings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeProvider _clock;

    public ImportJobsHandler(
        AethonDbContext db,
        ISystemSettingsService settings,
        UserManager<ApplicationUser> userManager,
        IDateTimeProvider clock)
    {
        _db       = db;
        _settings = settings;
        _userManager = userManager;
        _clock    = clock;
    }

    // ─── Single job ───────────────────────────────────────────────────────────

    public async Task<Result<ImportJobResult>> HandleAsync(
        string providedApiKey,
        ImportJobDto dto,
        CancellationToken ct = default)
    {
        var authError = await ValidateApiKeyAsync(providedApiKey, ct);
        if (authError is not null)
            return Result<ImportJobResult>.Failure("import.unauthorized", authError);

        return await IngestSingleAsync(dto, ct);
    }

    // ─── Bulk jobs ────────────────────────────────────────────────────────────

    public async Task<Result<List<ImportJobResult>>> HandleBulkAsync(
        string providedApiKey,
        List<ImportJobDto> dtos,
        CancellationToken ct = default)
    {
        var authError = await ValidateApiKeyAsync(providedApiKey, ct);
        if (authError is not null)
            return Result<List<ImportJobResult>>.Failure("import.unauthorized", authError);

        if (dtos is null || dtos.Count == 0)
            return Result<List<ImportJobResult>>.Failure("import.empty", "No jobs provided.");

        if (dtos.Count > 500)
            return Result<List<ImportJobResult>>.Failure("import.too_many", "Bulk import is limited to 500 jobs per request.");

        var results = new List<ImportJobResult>(dtos.Count);
        foreach (var dto in dtos)
        {
            var r = await IngestSingleAsync(dto, ct);
            if (r.Succeeded)
                results.Add(r.Value!);
            // Per-job failures are absorbed — continue processing remaining jobs.
        }

        return Result<List<ImportJobResult>>.Success(results);
    }

    // ─── Core ingestion ───────────────────────────────────────────────────────

    private async Task<Result<ImportJobResult>> IngestSingleAsync(ImportJobDto dto, CancellationToken ct)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.SourceSite))
            return Result<ImportJobResult>.Failure("import.source_site_required", "SourceSite is required.");

        if (string.IsNullOrWhiteSpace(dto.ExternalId))
            return Result<ImportJobResult>.Failure("import.external_id_required", "ExternalId is required.");

        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            return Result<ImportJobResult>.Failure("import.company_name_required", "CompanyName is required.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<ImportJobResult>.Failure("import.title_required", "Title is required.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            return Result<ImportJobResult>.Failure("import.description_required", "Description is required.");

        if (string.IsNullOrWhiteSpace(dto.ExternalApplicationUrl))
            return Result<ImportJobResult>.Failure("import.application_url_required", "ExternalApplicationUrl is required.");

        var externalRef = BuildExternalReference(dto.SourceSite, dto.ExternalId);

        var utcNow = _clock.UtcNow;

        // Find or create the import organisation
        var org = await FindOrCreateImportOrgAsync(dto, utcNow, ct);

        // Find or create the system user for this org
        var systemUser = await FindOrCreateImportUserAsync(org, utcNow, ct);

        // Build summary: use provided summary, or auto-generate from description
        var summary = BuildSummary(dto.Summary, dto.Description);

        // Upsert — update if already imported, create if new
        var existingJob = await _db.Jobs
            .Where(j => j.ExternalReference == externalRef && j.IsImported)
            .FirstOrDefaultAsync(ct);

        if (existingJob is not null)
        {
            var incomingPublished = dto.PublishedUtc ?? utcNow;

            // If publishedUtc is being moved to an earlier date, always set expiry to 30 days from it.
            // Otherwise honour the incoming expiry (or keep the existing one if none supplied).
            DateTime? newExpiry;
            if (existingJob.PublishedUtc.HasValue && incomingPublished < existingJob.PublishedUtc.Value)
                newExpiry = incomingPublished.AddDays(30);
            else
                newExpiry = dto.PostingExpiresUtc ?? existingJob.PostingExpiresUtc;

            existingJob.Title                  = dto.Title.Trim();
            existingJob.Description            = dto.Description.Trim();
            existingJob.Summary                = summary;
            existingJob.Requirements           = Normalize(dto.Requirements);
            existingJob.Benefits               = Normalize(dto.Benefits);
            existingJob.Department             = Normalize(dto.Department);
            existingJob.Keywords               = Normalize(dto.Keywords);
            existingJob.WorkplaceType          = dto.WorkplaceType;
            existingJob.EmploymentType         = dto.EmploymentType;
            existingJob.Category               = dto.Category;
            existingJob.ExternalApplicationUrl = dto.ExternalApplicationUrl.Trim();
            existingJob.LocationText           = Normalize(dto.LocationText);
            existingJob.LocationCity           = Normalize(dto.LocationCity);
            existingJob.LocationState          = Normalize(dto.LocationState);
            existingJob.LocationCountry        = Normalize(dto.LocationCountry);
            existingJob.LocationCountryCode    = Normalize(dto.LocationCountryCode);
            existingJob.LocationLatitude       = dto.LocationLatitude;
            existingJob.LocationLongitude      = dto.LocationLongitude;
            existingJob.LocationPlaceId        = Normalize(dto.LocationPlaceId);
            existingJob.SalaryFrom             = dto.SalaryFrom;
            existingJob.SalaryTo               = dto.SalaryTo;
            existingJob.SalaryCurrency         = dto.SalaryCurrency;
            existingJob.PublishedUtc           = incomingPublished;
            existingJob.PostingExpiresUtc      = newExpiry;
            existingJob.Regions                = dto.Regions.Count > 0
                                                    ? JsonSerializer.Serialize(dto.Regions, _enumJson)
                                                    : null;
            existingJob.Countries              = dto.Countries.Count > 0
                                                    ? JsonSerializer.Serialize(dto.Countries)
                                                    : null;
            existingJob.IncludeCompanyLogo     = !string.IsNullOrWhiteSpace(dto.CompanyLogoUrl);
            existingJob.ShortUrlCode           = Normalize(dto.Slug);
            existingJob.UpdatedUtc             = utcNow;
            existingJob.UpdatedByUserId        = systemUser.Id;

            await _db.SaveChangesAsync(ct);

            return Result<ImportJobResult>.Success(new ImportJobResult
            {
                JobId             = existingJob.Id,
                ExternalReference = externalRef,
                WasDuplicate      = false,
                WasUpdated        = true
            });
        }

        var incomingPublishedNew = dto.PublishedUtc ?? utcNow;

        var job = new Job
        {
            Id                          = Guid.NewGuid(),
            OwnedByOrganisationId       = org.Id,
            CreatedByIdentityUserId     = systemUser.Id,
            CreatedByUser               = null!,
            CreatedByType               = JobCreatedByType.ImportFeed,
            Status                      = JobStatus.Published,
            Visibility                  = JobVisibility.Public,
            PostingTier                 = JobPostingTier.Imported,
            IsImported                  = true,
            CreatedForUnclaimedCompany  = true,

            Title                       = dto.Title.Trim(),
            Description                 = dto.Description.Trim(),
            Summary                     = summary,
            Requirements                = Normalize(dto.Requirements),
            Benefits                    = Normalize(dto.Benefits),
            Department                  = Normalize(dto.Department),
            Keywords                    = Normalize(dto.Keywords),

            WorkplaceType               = dto.WorkplaceType,
            EmploymentType              = dto.EmploymentType,
            Category                    = dto.Category,

            ExternalReference           = externalRef,
            ExternalApplicationUrl      = dto.ExternalApplicationUrl.Trim(),

            // Location — accepted as-is from the source
            LocationText                = Normalize(dto.LocationText),
            LocationCity                = Normalize(dto.LocationCity),
            LocationState               = Normalize(dto.LocationState),
            LocationCountry             = Normalize(dto.LocationCountry),
            LocationCountryCode         = Normalize(dto.LocationCountryCode),
            LocationLatitude            = dto.LocationLatitude,
            LocationLongitude           = dto.LocationLongitude,
            LocationPlaceId             = Normalize(dto.LocationPlaceId),

            // Salary — fully optional for imports
            SalaryFrom                  = dto.SalaryFrom,
            SalaryTo                    = dto.SalaryTo,
            SalaryCurrency              = dto.SalaryCurrency,

            // Dates
            PublishedUtc                = incomingPublishedNew,
            ApprovedUtc                 = utcNow,
            PostingExpiresUtc           = dto.PostingExpiresUtc,

            // JSON lists
            Regions                     = dto.Regions.Count > 0
                                            ? JsonSerializer.Serialize(dto.Regions, _enumJson)
                                            : null,
            Countries                   = dto.Countries.Count > 0
                                            ? JsonSerializer.Serialize(dto.Countries)
                                            : null,

            // Logo — if the org has a logo URL show it on the listing
            IncludeCompanyLogo          = !string.IsNullOrWhiteSpace(dto.CompanyLogoUrl),

            // AI matching is never enabled for imported jobs
            HasAiCandidateMatching      = false,

            // School-leaver flags are never set for imported jobs
            IsSuitableForSchoolLeavers  = false,
            IsSchoolLeaverTargeted      = false,

            // Slug from source feed (stored as ShortUrlCode)
            ShortUrlCode                = Normalize(dto.Slug),

            CreatedUtc                  = utcNow,
            CreatedByUserId             = systemUser.Id
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(ct);

        return Result<ImportJobResult>.Success(new ImportJobResult
        {
            JobId             = job.Id,
            ExternalReference = externalRef,
            WasDuplicate      = false,
            WasUpdated        = false
        });
    }

    // ─── Find or create import organisation ──────────────────────────────────

    private async Task<Organisation> FindOrCreateImportOrgAsync(
        ImportJobDto dto,
        DateTime utcNow,
        CancellationToken ct)
    {
        var orgName       = BuildImportOrgName(dto.SourceSite, dto.CompanyName);
        var normalizedName = orgName.ToUpperInvariant();

        var org = await _db.Organisations
            .FirstOrDefaultAsync(o => o.NormalizedName == normalizedName, ct);

        if (org is not null)
        {
            // Update logo URL if a new one is provided and the org doesn't have one yet
            if (!string.IsNullOrWhiteSpace(dto.CompanyLogoUrl) && string.IsNullOrWhiteSpace(org.LogoUrl))
            {
                org.LogoUrl    = dto.CompanyLogoUrl;
                org.UpdatedUtc = utcNow;
                await _db.SaveChangesAsync(ct);
            }
            return org;
        }

        org = new Organisation
        {
            Id                     = Guid.NewGuid(),
            Type                   = OrganisationType.Company,
            Status                 = OrganisationStatus.Active,
            ClaimStatus            = OrganisationClaimStatus.NotApplicable,
            Name                   = orgName,
            NormalizedName         = normalizedName,
            IsPublicProfileEnabled = false,
            LogoUrl                = string.IsNullOrWhiteSpace(dto.CompanyLogoUrl) ? null : dto.CompanyLogoUrl,
            CreatedUtc             = utcNow
        };

        _db.Organisations.Add(org);
        await _db.SaveChangesAsync(ct);

        return org;
    }

    // ─── Find or create import system user ───────────────────────────────────

    private async Task<ApplicationUser> FindOrCreateImportUserAsync(
        Organisation org,
        DateTime utcNow,
        CancellationToken ct)
    {
        // Deterministic email for the system user bound to this import org
        var email = BuildImportUserEmail(org.NormalizedName);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null) return user;

        user = new ApplicationUser
        {
            Id               = Guid.NewGuid(),
            UserName         = email,
            Email            = email,
            NormalizedEmail  = email.ToUpperInvariant(),
            DisplayName      = $"{org.Name} (Import Feed)",
            UserType         = UserAccountType.Company,
            IsEnabled        = true,
            EmailConfirmed   = true
        };

        // The system account is never logged into — use a random secure password
        var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                     + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            // Attempt to load again in case of race condition
            var retry = await _userManager.FindByEmailAsync(email);
            if (retry is not null) return retry;
            throw new InvalidOperationException(
                $"Failed to create import system user for org '{org.Name}': " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        // Link user to organisation as owner
        var membership = new OrganisationMembership
        {
            Id             = Guid.NewGuid(),
            OrganisationId = org.Id,
            UserId         = user.Id,
            CompanyRole    = CompanyRole.Owner,
            Status         = MembershipStatus.Active,
            IsOwner        = true,
            JoinedUtc      = utcNow,
            CreatedUtc     = utcNow,
            CreatedByUserId = user.Id
        };

        _db.OrganisationMemberships.Add(membership);
        await _db.SaveChangesAsync(ct);

        return user;
    }

    // ─── API key validation ───────────────────────────────────────────────────

    private async Task<string?> ValidateApiKeyAsync(string providedKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(providedKey))
            return "Import API key is required.";

        // Prefer DB-stored key; fall back to environment variable
        var storedKey = await _settings.GetStringAsync(SystemSettingKeys.ImportApiKey, ct);
        if (string.IsNullOrWhiteSpace(storedKey))
            storedKey = Environment.GetEnvironmentVariable("IMPORT_API_KEY");

        if (string.IsNullOrWhiteSpace(storedKey))
            return "Import API key has not been configured. Set it via Admin → Settings → Import API Key.";

        return string.Equals(providedKey, storedKey, StringComparison.Ordinal)
            ? null
            : "Invalid import API key.";
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>ImportOrg_{sourceSite}_{companyName} — max 200 chars.</summary>
    private static string BuildImportOrgName(string sourceSite, string companyName)
    {
        var safe = Regex.Replace(companyName.Trim(), @"[^\w\s\.\-]", "").Trim();
        var name = $"ImportOrg_{sourceSite.Trim()}_{safe}";
        return name.Length > 200 ? name[..200] : name;
    }

    /// <summary>Deterministic internal email for the system user of this import org.</summary>
    private static string BuildImportUserEmail(string normalizedOrgName)
    {
        // Sanitise to produce a valid email local-part
        var safe = Regex.Replace(normalizedOrgName.ToLower(), @"[^a-z0-9\-_]", "-");
        var local = safe.Length > 50 ? safe[..50] : safe;
        return $"sys-{local}@aethon-import.internal";
    }

    /// <summary>{sourceSite}_{externalId} — max 150 chars.</summary>
    private static string BuildExternalReference(string sourceSite, string externalId)
    {
        var raw = $"{sourceSite.Trim()}_{externalId.Trim()}";
        return raw.Length > 150 ? raw[..150] : raw;
    }

    /// <summary>
    /// Returns provided summary, or strips HTML from description and takes the first 200 chars.
    /// </summary>
    private static string? BuildSummary(string? providedSummary, string description)
    {
        if (!string.IsNullOrWhiteSpace(providedSummary))
            return providedSummary.Trim();

        var plain = StripHtml(description);
        if (string.IsNullOrWhiteSpace(plain)) return null;
        return plain.Length <= 200 ? plain : plain[..200].TrimEnd();
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        // Remove tags and collapse whitespace
        var noTags = Regex.Replace(html, "<[^>]*>", " ");
        var decoded = System.Net.WebUtility.HtmlDecode(noTags);
        return Regex.Replace(decoded, @"\s+", " ").Trim();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static readonly JsonSerializerOptions _enumJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
