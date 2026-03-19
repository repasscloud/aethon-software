using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Queries.GetApplicationsForJob;

public sealed class GetApplicationsForJobQuery
{
    public Guid JobId { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public ApplicationStatus? Status { get; init; }
}
