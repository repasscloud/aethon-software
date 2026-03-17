using Aethon.Shared.CompanyJobsApproval;

namespace Aethon.Application.CompanyJobsApproval;

public interface ICompanyJobApprovalCommandService
{
    Task ApproveAsync(Guid companyUserId, Guid jobId, CancellationToken cancellationToken);
    Task RejectAsync(Guid companyUserId, Guid jobId, RejectJobApprovalDto request, CancellationToken cancellationToken);
    Task PublishAsync(Guid companyUserId, Guid jobId, CancellationToken cancellationToken);
    Task PutOnHoldAsync(Guid companyUserId, Guid jobId, HoldJobDto request, CancellationToken cancellationToken);
    Task CancelAsync(Guid companyUserId, Guid jobId, CancelJobDto request, CancellationToken cancellationToken);
}
