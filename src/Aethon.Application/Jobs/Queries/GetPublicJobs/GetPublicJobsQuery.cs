using Aethon.Shared.Enums;

namespace Aethon.Application.Jobs.Queries.GetPublicJobs;

public sealed class GetPublicJobsQuery
{
    public string? City { get; init; }
    public DateRangeFilter? DateRange { get; init; }
    public JobCategory? Category { get; init; }
    public JobRegion? Region { get; init; }
    public string? Country { get; init; }
    public string? Keywords { get; init; }
    public string? OrganisationSlug { get; init; }
    public decimal? SalaryMin { get; init; }
    public decimal? SalaryMax { get; init; }
    public bool VerifiedOnly { get; init; }
    public WorkplaceType? WorkplaceType { get; init; }
    public bool ImmediateStart { get; init; }

    /// <summary>Centre latitude for radius-based search.</summary>
    public double? Latitude { get; init; }

    /// <summary>Centre longitude for radius-based search.</summary>
    public double? Longitude { get; init; }

    /// <summary>Search radius in kilometres. Default 25 KM when Latitude/Longitude are provided.</summary>
    public double RadiusKm { get; init; } = 25;

    /// <summary>
    /// Age group of the authenticated viewer, resolved by the endpoint from the job seeker profile.
    /// Null means unauthenticated or non-job-seeker. School-leaver-targeted jobs are hidden unless
    /// this is ApplicantAgeGroup.SchoolLeaver.
    /// </summary>
    public ApplicantAgeGroup? ViewerAgeGroup { get; init; }

    /// <summary>1-based page number. Defaults to 1.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Number of results per page. Clamped to 1–100. Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;
}

public enum DateRangeFilter
{
    Today = 1,
    Last3Days,
    LastWeek,
    Last14Days,
    Last30Days,
    Last60Days,
    Last90Days,
    Over90Days
}
