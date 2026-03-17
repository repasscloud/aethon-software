using Aethon.Shared.Applications;

namespace Aethon.Application.RecruiterApplications;

public interface IRecruiterApplicationCommandService
{
    Task AssignAsync(
        Guid recruiterUserId,
        Guid applicationId,
        AssignApplicationDto request,
        CancellationToken cancellationToken);

    Task UnassignAsync(
        Guid recruiterUserId,
        Guid applicationId,
        CancellationToken cancellationToken);
}