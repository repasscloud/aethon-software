using Aethon.Shared.Jobs;

namespace Aethon.Application.CompanyJobsApproval;

public interface ICompanyJobApprovalQueryService
{
    Task<IReadOnlyList<JobSummaryDto>> GetPendingApprovalsAsync(Guid companyUserId, CancellationToken cancellationToken);
}
