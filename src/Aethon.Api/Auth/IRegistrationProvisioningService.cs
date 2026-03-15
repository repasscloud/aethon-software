using Aethon.Data.Identity;
using Aethon.Shared.Auth;

namespace Aethon.Api.Auth;

public interface IRegistrationProvisioningService
{
    Task<RegistrationProvisioningResult> ProvisionAsync(
        ApplicationUser user,
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);
}

public sealed class RegistrationProvisioningResult
{
    public bool Succeeded { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
