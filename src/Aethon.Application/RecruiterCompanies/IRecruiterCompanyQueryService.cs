using Aethon.Shared.RecruiterCompanies;

namespace Aethon.Application.RecruiterCompanies;

public interface IRecruiterCompanyQueryService
{
    Task<IReadOnlyList<RecruiterCompanyRelationshipDto>> GetRecruiterRelationshipsAsync(
        Guid recruiterUserId,
        CancellationToken cancellationToken);
}

