using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Infrastructure.Workers;

/// <summary>
/// Background worker that runs every hour and auto-verifies organisation domains
/// whose DNS TXT record matches the expected verification value.
/// </summary>
public sealed class DomainVerificationWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DomainVerificationWorker> _logger;

    public DomainVerificationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<DomainVerificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a short period on startup to let the API fully initialise
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "DomainVerificationWorker encountered an unexpected error.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();

        // Only process domains that have been set up for DNS TXT verification and
        // that have a verification token + DNS record value to check against.
        var pendingDomains = await db.Set<OrganisationDomain>()
            .Where(d =>
                d.Status == DomainStatus.Pending &&
                d.VerificationMethod == DomainVerificationMethod.DnsTxt &&
                d.VerificationDnsRecordName != null &&
                d.VerificationDnsRecordValue != null)
            .ToListAsync(ct);

        if (pendingDomains.Count == 0)
            return;

        _logger.LogInformation(
            "DomainVerificationWorker: checking {Count} pending DNS TXT domain(s).",
            pendingDomains.Count);

        var dns = new LookupClient(new LookupClientOptions
        {
            UseCache = false,
            Timeout = TimeSpan.FromSeconds(5),
            Retries = 1
        });

        var now = DateTime.UtcNow;
        var verified = 0;

        foreach (var domain in pendingDomains)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var txtRecordName = domain.VerificationDnsRecordName!;
                var expectedValue = domain.VerificationDnsRecordValue!;

                var result = await dns.QueryAsync(txtRecordName, QueryType.TXT, cancellationToken: ct);

                var found = result.Answers
                    .OfType<TxtRecord>()
                    .SelectMany(r => r.Text)
                    .Any(txt => string.Equals(txt, expectedValue, StringComparison.OrdinalIgnoreCase));

                if (found)
                {
                    domain.Status = DomainStatus.Verified;
                    domain.TrustLevel = DomainTrustLevel.High;
                    domain.VerifiedUtc = now;
                    verified++;

                    _logger.LogInformation(
                        "DomainVerificationWorker: verified domain {Domain} for org {OrgId}.",
                        domain.Domain,
                        domain.OrganisationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "DomainVerificationWorker: DNS lookup failed for {Domain}.",
                    domain.Domain);
            }
        }

        if (verified > 0)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("DomainVerificationWorker: verified {Count} domain(s).", verified);
        }
    }
}
