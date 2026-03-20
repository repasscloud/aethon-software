using Aethon.Application.Abstractions.Files;
using Aethon.Application.Abstractions.ResumeAnalysis;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Aethon.Api.Infrastructure.Workers;

public sealed class ResumeAnalysisWorker : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceProvider _services;
    private readonly ILogger<ResumeAnalysisWorker> _logger;

    public ResumeAnalysisWorker(IServiceProvider services, ILogger<ResumeAnalysisWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ResumeAnalysisWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingAsync(stoppingToken);
            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AethonDbContext>();
        var analysisService = scope.ServiceProvider.GetRequiredService<IResumeAnalysisService>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var pending = await db.ResumeAnalyses
            .Include(a => a.StoredFile)
            .Where(a => a.Status == ResumeAnalysisStatus.Pending)
            .Take(5)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        _logger.LogInformation("Processing {Count} pending resume analyses.", pending.Count);

        foreach (var analysis in pending)
        {
            try
            {
                analysis.Status = ResumeAnalysisStatus.Processing;
                await db.SaveChangesAsync(ct);

                byte[] fileBytes;
                using (var stream = await fileStorage.OpenReadAsync(analysis.StoredFile.StoragePath, ct))
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms, ct);
                    fileBytes = ms.ToArray();
                }

                var result = await analysisService.AnalyseAsync(
                    analysis.StoredFile.OriginalFileName,
                    analysis.StoredFile.ContentType,
                    fileBytes,
                    ct);

                if (result.Success)
                {
                    analysis.Status = ResumeAnalysisStatus.Completed;
                    analysis.HeadlineSuggestion = result.HeadlineSuggestion;
                    analysis.SummaryExtract = result.SummaryExtract;
                    analysis.SkillsJson = result.Skills.Count > 0
                        ? JsonSerializer.Serialize(result.Skills)
                        : null;
                    analysis.ExperienceLevel = result.ExperienceLevel;
                    analysis.YearsExperience = result.YearsExperience;
                    analysis.AnalysedUtc = DateTime.UtcNow;
                    analysis.AnalysisError = null;
                }
                else
                {
                    analysis.Status = ResumeAnalysisStatus.Failed;
                    analysis.AnalysisError = result.Error;
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process ResumeAnalysis {Id}", analysis.Id);
                analysis.Status = ResumeAnalysisStatus.Failed;
                analysis.AnalysisError = ex.Message;
                try { await db.SaveChangesAsync(ct); } catch { /* best effort */ }
            }
        }
    }
}
