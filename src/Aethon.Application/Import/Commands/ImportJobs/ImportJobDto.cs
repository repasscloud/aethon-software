using Aethon.Shared.Enums;

namespace Aethon.Application.Import.Commands.ImportJobs;

/// <summary>
/// Represents a single job posting supplied by an external import feed.
/// Required fields match the Job entity minimums; all other fields are optional.
/// </summary>
public sealed class ImportJobDto
{
    // ─── Source identity ──────────────────────────────────────────────────────

    /// <summary>
    /// Short identifier for the source website / feed (e.g. "remoteok.com", "linkedin").
    /// Used to namespace org names and ExternalReference values.
    /// </summary>
    public string SourceSite { get; init; } = string.Empty;

    /// <summary>
    /// The ID this job has in the source system.
    /// Combined with SourceSite to form ExternalReference = "{SourceSite}_{ExternalId}".
    /// </summary>
    public string ExternalId { get; init; } = string.Empty;

    // ─── Company ──────────────────────────────────────────────────────────────

    /// <summary>Company name as reported by the source feed.</summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>Optional URL to the company logo image. Stored directly as LogoUrl on the import org.</summary>
    public string? CompanyLogoUrl { get; init; }

    // ─── Required job fields ──────────────────────────────────────────────────

    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public WorkplaceType WorkplaceType { get; init; }
    public EmploymentType? EmploymentType { get; init; }

    // ─── Classification ───────────────────────────────────────────────────────

    public JobCategory? Category { get; init; }
    public string? Keywords { get; init; }
    public List<JobRegion> Regions { get; init; } = [];
    public List<string> Countries { get; init; } = [];

    // ─── Core optional fields ─────────────────────────────────────────────────

    public string? Summary { get; init; }
    public string? Requirements { get; init; }
    public string? Benefits { get; init; }
    public string? Department { get; init; }

    // ─── Salary ───────────────────────────────────────────────────────────────

    public decimal? SalaryFrom { get; init; }
    public decimal? SalaryTo { get; init; }
    public CurrencyCode? SalaryCurrency { get; init; }

    // ─── Dates ────────────────────────────────────────────────────────────────

    /// <summary>When the job was originally published on the source platform.</summary>
    public DateTime? PublishedUtc { get; init; }

    /// <summary>When this job posting should expire and be hidden.</summary>
    public DateTime? PostingExpiresUtc { get; init; }

    // ─── Application routing ─────────────────────────────────────────────────

    /// <summary>
    /// URL candidates should be directed to when applying.
    /// Required — imported jobs always use an external application URL.
    /// </summary>
    public string ExternalApplicationUrl { get; init; } = string.Empty;

    // ─── Location ────────────────────────────────────────────────────────────

    public string? LocationText { get; init; }
    public string? LocationCity { get; init; }
    public string? LocationState { get; init; }
    public string? LocationCountry { get; init; }
    public string? LocationCountryCode { get; init; }
    public double? LocationLatitude { get; init; }
    public double? LocationLongitude { get; init; }
    public string? LocationPlaceId { get; init; }

    // ─── Slug ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Optional slug from the source platform.
    /// Stored as ShortUrlCode and used to build the /jobs/{orgSlug}/{slug} URL if provided.
    /// </summary>
    public string? Slug { get; init; }
}
