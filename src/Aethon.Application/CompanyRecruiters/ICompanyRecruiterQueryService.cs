using Aethon.Shared.RecruiterCompanies;

namespace Aethon.Application.CompanyRecruiters;

public interface ICompanyRecruiterQueryService
{
    Task<IReadOnlyList<RecruiterCompanyRelationshipDto>> GetPendingRequestsAsync(
        Guid companyUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RecruiterCompanyRelationshipDto>> GetRelationshipsAsync(
        Guid companyUserId,
        CancellationToken cancellationToken);
}
