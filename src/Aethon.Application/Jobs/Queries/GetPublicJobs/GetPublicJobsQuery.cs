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
