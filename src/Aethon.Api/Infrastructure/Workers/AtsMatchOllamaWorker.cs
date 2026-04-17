using System.Text.Json;
using Aethon.Application.Abstractions.AtsMatch;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Infrastructure.Workers;

/// <summary>
/// Background worker that processes ATS match queue items for the Ollama (free / offline) tier.
/// Polls every 60 seconds, takes up to 2 items. Ollama is CPU-only and slower than Claude,
/// so the batch size is kept small to avoid overloading the container.
/// Max 3 attempts per item before marking as Failed.
/// </summary>
public sealed class AtsMatchOllamaWorker : BackgroundService
{
    private const int MaxAttempts = 3;
    private const int BatchSize   = 2;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IServiceProvider _services;
    private readonly ILogger<AtsMatchOllamaWorker> _logger;

    public AtsMatchOllamaWorker(IServiceProvider services, ILogger<AtsMatchOllamaWorker> logger)
    {
        _services = services;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AtsMatchOllamaWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in AtsMatchOllamaWorker.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("AtsMatchOllamaWorker stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope = _services.CreateAsyncScope();
        var db      = scope.ServiceProvider.GetRequiredService<AethonDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<OllamaAtsMatchingServiceAccessor>().Service;

        var pending = await db.AtsMatchQueue
            .Where(q => q.Provider == AtsMatchProvider.Ollama
                     && q.Status   == AtsMatchStatus.Pending
                     && q.Attempts <  MaxAttempts)
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        _logger.LogInformation("AtsMatchOllamaWorker: processing {Count} items.", pending.Count);

        foreach (var item in pending)
        {
            await ProcessItemAsync(db, service, item, ct);
        }
    }

    private async Task ProcessItemAsync(
        AethonDbContext db,
        IAtsMatchingService service,
        AtsMatchQueueItem item,
        CancellationToken ct)
    {
        item.Status         = AtsMatchStatus.Processing;
        item.Attempts++;
        item.LastAttemptUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        try
        {
            var response = await service.MatchAsync(item.PayloadJson, ct);

            if (!response.Success)
            {
                _logger.LogWarning("Ollama ATS match failed for queue item {Id}: {Error}", item.Id, response.Error);
                item.Status       = item.Attempts >= MaxAttempts ? AtsMatchStatus.Failed : AtsMatchStatus.Pending;
                item.ErrorMessage = response.Error;
                await db.SaveChangesAsync(ct);
                return;
            }

            var result = BuildResult(item, response);
            db.AtsMatchResults.Add(result);

            item.Status       = AtsMatchStatus.Completed;
            item.ProcessedUtc = DateTime.UtcNow;
            item.ErrorMessage = null;

            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Ollama ATS match completed for application {AppId}: score={Score} recommendation={Rec}",
                item.JobApplicationId, response.OverallScore, response.Recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Ollama ATS match queue item {Id}.", item.Id);
            item.Status       = item.Attempts >= MaxAttempts ? AtsMatchStatus.Failed : AtsMatchStatus.Pending;
            item.ErrorMessage = ex.Message;
            try { await db.SaveChangesAsync(ct); } catch { /* best-effort */ }
        }
    }

    private static AtsMatchResult BuildResult(AtsMatchQueueItem item, AtsMatchResponse r) => new()
    {
        Id                  = Guid.NewGuid(),
        AtsMatchQueueItemId = item.Id,
        JobApplicationId    = item.JobApplicationId,
        JobId               = item.JobId,
        CandidateUserId     = item.CandidateUserId,
        Provider            = AtsMatchProvider.Ollama,
        ModelUsed           = r.ModelUsed ?? "ollama",
        OverallScore        = r.OverallScore,
        SkillsScore         = r.SkillsScore,
        ExperienceScore     = r.ExperienceScore,
        LocationScore       = r.LocationScore,
        SalaryScore         = r.SalaryScore,
        QualificationsScore = r.QualificationsScore,
        WorkRightsScore     = r.WorkRightsScore,
        Recommendation      = r.Recommendation,
        Strengths           = r.Strengths.Count > 0 ? JsonSerializer.Serialize(r.Strengths) : null,
        Gaps                = r.Gaps.Count      > 0 ? JsonSerializer.Serialize(r.Gaps)      : null,
        Summary             = r.Summary,
        Confidence          = r.Confidence,
        RawResponseJson     = r.RawResponseJson,
        TokensUsed          = null,   // Ollama does not report token usage via /api/chat
        ProcessedUtc        = DateTime.UtcNow,
        CreatedUtc          = DateTime.UtcNow
    };
}

/// <summary>
/// Thin accessor that lets OllamaAtsMatchWorker resolve the Ollama service specifically
/// without conflict with the Claude registration of IAtsMatchingService.
/// </summary>
public sealed class OllamaAtsMatchingServiceAccessor
{
    public IAtsMatchingService Service { get; }
    public OllamaAtsMatchingServiceAccessor(IAtsMatchingService service) => Service = service;
}
