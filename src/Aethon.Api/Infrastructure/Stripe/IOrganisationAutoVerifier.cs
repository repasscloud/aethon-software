using Aethon.Data.Entities;

namespace Aethon.Api.Infrastructure.Stripe;

/// <summary>
/// Runs automated checks against an organisation to determine whether it
/// qualifies for Standard Employer Verification without manual review.
/// </summary>
public interface IOrganisationAutoVerifier
{
    /// <summary>
    /// Returns true if the organisation passes automated verification checks.
    /// Returns false if the case should fall through to manual admin review.
    /// </summary>
    Task<bool> CheckAsync(Organisation org, CancellationToken ct = default);
}
