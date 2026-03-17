using Aethon.Shared.Applications;

namespace Aethon.Application.RecruiterApplications;

public interface IRecruiterApplicationQueryService
{
    Task<IReadOnlyList<ApplicationSummaryDto>> GetAssignedApplicationsAsync(
        Guid recruiterUserId,
        CancellationToken cancellationToken);
}