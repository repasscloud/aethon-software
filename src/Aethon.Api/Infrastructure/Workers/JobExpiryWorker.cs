using Aethon.Application.Abstractions.Syndication;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Infrastructure.Workers;

public sealed class JobExpiryWorker : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(30);

    private readonly IServiceProvider _services;
    private readonly ILogger<JobExpiryWorker> _logger;

    public JobExpiryWorker(IServiceProvider services, ILogger<JobExpiryWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredJobsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "JobExpiryWorker encountered an error.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredJobsAsync(CancellationToken ct)
    {
        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();
        var indexingService = scope.ServiceProvider.GetRequiredService<IGoogleIndexingService>();

        var utcNow = DateTime.UtcNow;

        var expiredJobs = await db.Jobs
            .Where(j => j.Status == JobStatus.Published
                     && j.PostingExpiresUtc != null
                     && j.PostingExpiresUtc <= utcNow)
            .ToListAsync(ct);

        if (expiredJobs.Count == 0)
            return;

        _logger.LogInformation("JobExpiryWorker: expiring {Count} job(s).", expiredJobs.Count);

        // We need to construct a base URL for the canonical job URLs.
        // Read from configuration — fall back to empty string (indexing will fail gracefully).
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var webBaseUrl = configuration["Email:WebBaseUrl"] ?? "";

        foreach (var job in expiredJobs)
        {
            job.Status = JobStatus.Closed;
            job.ClosedUtc = utcNow;
            job.StatusReason = "Expired";
            job.UpdatedUtc = utcNow;
        }

        await db.SaveChangesAsync(ct);

        // Fire Google Indexing DELETE for each expired job (no-op if disabled)
        foreach (var job in expiredJobs)
        {
            var jobUrl = JobUrlHelper.BuildPublicUrl(webBaseUrl, job.Id);
            await indexingService.NotifyRemovedAsync(job.Id, jobUrl, ct);
        }
    }
}
