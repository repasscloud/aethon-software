using Aethon.Shared.Jobs;

namespace Aethon.Application.RecruiterJobs;
public interface IRecruiterJobQueryService
{
    Task<IReadOnlyList<JobSummaryDto>> GetRecruiterJobsAsync(Guid recruiterUserId, CancellationToken cancellationToken);
}
