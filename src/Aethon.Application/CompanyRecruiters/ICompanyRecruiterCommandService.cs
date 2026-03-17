using Aethon.Shared.CompanyRecruiters;

namespace Aethon.Application.CompanyRecruiters;

public interface ICompanyRecruiterCommandService
{
    Task ApproveAsync(
        Guid companyUserId,
        Guid relationshipId,
        ApproveRecruiterCompanyRequestDto request,
        CancellationToken cancellationToken);

    Task RejectAsync(
        Guid companyUserId,
        Guid relationshipId,
        RejectRecruiterCompanyRequestDto request,
        CancellationToken cancellationToken);

    Task SuspendAsync(
        Guid companyUserId,
        Guid relationshipId,
        CancellationToken cancellationToken);

    Task InviteAsync(
        Guid companyUserId,
        CreateCompanyRecruiterInviteDto request,
        CancellationToken cancellationToken);
}
