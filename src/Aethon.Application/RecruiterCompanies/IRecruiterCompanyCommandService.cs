using Aethon.Shared.RecruiterCompanies;

namespace Aethon.Application.RecruiterCompanies;

public interface IRecruiterCompanyCommandService
{
    Task<RecruiterCompanyRelationshipDto> CreateRequestAsync(
        Guid recruiterUserId,
        CreateRecruiterCompanyRequestDto request,
        CancellationToken cancellationToken);

    Task CancelRequestAsync(
        Guid recruiterUserId,
        Guid relationshipId,
        CancellationToken cancellationToken);
}