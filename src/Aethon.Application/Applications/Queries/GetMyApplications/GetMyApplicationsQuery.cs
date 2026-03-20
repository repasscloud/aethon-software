namespace Aethon.Application.Applications.Queries.GetMyApplications;

public sealed class GetMyApplicationsQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
