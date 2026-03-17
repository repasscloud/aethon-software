using Aethon.Shared.Jobs;

namespace Aethon.Application.RecruiterJobs;

public interface IRecruiterJobCommandService
{
    Task<JobDetailDto> CreateDraftAsync(Guid recruiterUserId, RecruiterCreateJobDraftDto request, CancellationToken cancellationToken);
    Task<JobDetailDto> UpdateDraftAsync(Guid recruiterUserId, Guid jobId, RecruiterUpdateJobDraftDto request, CancellationToken cancellationToken);
    Task SubmitForApprovalAsync(Guid recruiterUserId, Guid jobId, CancellationToken cancellationToken);
    Task WithdrawAsync(Guid recruiterUserId, Guid jobId, CancellationToken cancellationToken);
}
