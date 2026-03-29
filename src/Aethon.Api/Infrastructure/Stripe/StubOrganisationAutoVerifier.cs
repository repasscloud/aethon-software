using Aethon.Data.Entities;

namespace Aethon.Api.Infrastructure.Stripe;

/// <summary>
/// Stub implementation — always returns false, routing all Standard verification
/// payments to manual admin review. Replace this with real checks when ready.
/// </summary>
public sealed class StubOrganisationAutoVerifier : IOrganisationAutoVerifier
{
    public Task<bool> CheckAsync(Organisation org, CancellationToken ct = default)
        => Task.FromResult(false);
}
