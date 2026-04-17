namespace Aethon.Api.Infrastructure.Workers;

/// <summary>
/// Placeholder background worker for automated identity verification processing.
/// Currently a no-op — reserved for future integration with a third-party ID verification provider.
/// When implemented, this worker will:
///   1. Poll for Pending IdentityVerificationRequests that have supporting document URLs
///   2. Submit them to the verification provider (e.g. Stripe Identity, Onfido, Veriff)
///   3. Update the request status to Approved or Denied and set IsIdentityVerified on ApplicationUser
///   4. Write ReviewNotes, ReviewedUtc, ReviewerType = System, ReviewedByUserId = null
/// </summary>
public sealed class IdentityVerificationWorker : BackgroundService
{
    private readonly ILogger<IdentityVerificationWorker> _logger;

    public IdentityVerificationWorker(ILogger<IdentityVerificationWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IdentityVerificationWorker started (placeholder — no automated processing configured).");

        // TODO: replace this loop with real processing logic when a provider is integrated
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
