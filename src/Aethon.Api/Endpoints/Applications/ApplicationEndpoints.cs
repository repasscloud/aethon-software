using System.Text.Json;
using Aethon.Api.Common;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Applications.Commands.AddApplicationComment;
using Aethon.Application.Applications.Commands.AddApplicationNote;
using Aethon.Application.Applications.Commands.AttachApplicationFile;
using Aethon.Application.Applications.Commands.ChangeApplicationStatus;
using Aethon.Application.Applications.Commands.ScheduleInterview;
using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Applications.Queries.GetApplicationById;
using Aethon.Application.Applications.Queries.GetApplicationFiles;
using Aethon.Application.Applications.Queries.GetApplicationTimeline;
using Aethon.Application.Applications.Queries.GetMyApplications;
using Aethon.Application.Applications.Services;
using Aethon.Application.AtsMatch;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.AtsMatch;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Applications;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpointsGroup(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/applications")
            .RequireAuthorization()
            .WithTags("Applications");

        group.MapGet("/status-options", () =>
        {
            var options = Enum.GetNames<ApplicationStatus>();
            return Results.Ok(options);
        });

        group.MapPost(string.Empty, async (
            [FromServices] SubmitJobApplicationHandler handler,
            HttpContext httpContext,
            SubmitJobApplicationCommand command,
            CancellationToken ct) =>
        {
            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/mine", async (
            [FromServices] GetMyApplicationsHandler handler,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await handler.HandleAsync(
                new GetMyApplicationsQuery
                {
                    Page = page,
                    PageSize = pageSize
                },
                ct);

            return result.ToMinimalApiResult();
        });

        group.MapGet("/{applicationId:guid}", async (
            [FromServices] GetApplicationByIdHandler handler,
            Guid applicationId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetApplicationByIdQuery
                {
                    ApplicationId = applicationId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        group.MapGet("/{applicationId:guid}/timeline", async (
            [FromServices] GetApplicationTimelineHandler handler,
            Guid applicationId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetApplicationTimelineQuery
                {
                    ApplicationId = applicationId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{applicationId:guid}/status", async (
            [FromServices] ChangeApplicationStatusHandler handler,
            HttpContext httpContext,
            Guid applicationId,
            ChangeApplicationStatusCommand request,
            CancellationToken ct) =>
        {
            var command = new ChangeApplicationStatusCommand
            {
                ApplicationId = applicationId,
                Status = request.Status,
                Reason = request.Reason,
                Notes = request.Notes
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{applicationId:guid}/notes", async (
            [FromServices] AddApplicationNoteHandler handler,
            HttpContext httpContext,
            Guid applicationId,
            AddApplicationNoteCommand request,
            CancellationToken ct) =>
        {
            var command = new AddApplicationNoteCommand
            {
                ApplicationId = applicationId,
                Content = request.Content
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{applicationId:guid}/comments", async (
            [FromServices] AddApplicationCommentHandler handler,
            HttpContext httpContext,
            Guid applicationId,
            AddApplicationCommentCommand request,
            CancellationToken ct) =>
        {
            var command = new AddApplicationCommentCommand
            {
                ApplicationId = applicationId,
                ParentCommentId = request.ParentCommentId,
                Content = request.Content
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{applicationId:guid}/interviews", async (
            [FromServices] ScheduleInterviewHandler handler,
            HttpContext httpContext,
            Guid applicationId,
            ScheduleInterviewCommand request,
            CancellationToken ct) =>
        {
            var command = new ScheduleInterviewCommand
            {
                ApplicationId = applicationId,
                Type = request.Type,
                Title = request.Title,
                Location = request.Location,
                MeetingUrl = request.MeetingUrl,
                Notes = request.Notes,
                ScheduledStartUtc = request.ScheduledStartUtc,
                ScheduledEndUtc = request.ScheduledEndUtc
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{applicationId:guid}/files", async (
            [FromServices] AttachApplicationFileHandler handler,
            HttpContext httpContext,
            Guid applicationId,
            AttachApplicationFileCommand request,
            CancellationToken ct) =>
        {
            var command = new AttachApplicationFileCommand
            {
                ApplicationId = applicationId,
                StoredFileId = request.StoredFileId,
                Category = request.Category,
                Notes = request.Notes
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/{applicationId:guid}/files", async (
            [FromServices] GetApplicationFilesHandler handler,
            Guid applicationId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetApplicationFilesQuery
                {
                    ApplicationId = applicationId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        // ── ATS match result ─────────────────────────────────────────────────
        group.MapGet("/{applicationId:guid}/match", async (
            [FromServices] AethonDbContext db,
            [FromServices] ICurrentUserAccessor currentUser,
            [FromServices] ApplicationAccessService accessService,
            Guid applicationId,
            CancellationToken ct) =>
        {
            if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
                return Results.Unauthorized();

            var application = await db.JobApplications
                .AsNoTracking()
                .Where(x => x.Id == applicationId)
                .Select(x => new { x.Id, x.UserId })
                .SingleOrDefaultAsync(ct);

            if (application is null)
                return Results.NotFound();

            var canManage = await accessService.CanManageApplicationAsync(currentUser.UserId, applicationId, ct);
            var isOwner   = application.UserId == currentUser.UserId;

            if (!canManage && !isOwner)
                return Results.Forbid();

            var queueItem = await db.AtsMatchQueue
                .AsNoTracking()
                .Where(q => q.JobApplicationId == applicationId)
                .OrderByDescending(q => q.CreatedUtc)
                .Select(q => new { q.Status, q.Provider, q.ErrorMessage })
                .FirstOrDefaultAsync(ct);

            if (queueItem is null)
                return Results.NotFound(new { error = "No ATS match has been queued for this application." });

            if (queueItem.Status != AtsMatchStatus.Completed)
            {
                return Results.Ok(new AtsMatchResultDto
                {
                    Status   = queueItem.Status.ToString(),
                    Provider = queueItem.Provider.ToString()
                });
            }

            var match = await db.AtsMatchResults
                .AsNoTracking()
                .Where(r => r.JobApplicationId == applicationId)
                .OrderByDescending(r => r.ProcessedUtc)
                .FirstOrDefaultAsync(ct);

            if (match is null)
                return Results.Ok(new AtsMatchResultDto { Status = queueItem.Status.ToString() });

            var strengths = match.Strengths is not null
                ? JsonSerializer.Deserialize<List<string>>(match.Strengths) ?? []
                : new List<string>();
            var gaps = match.Gaps is not null
                ? JsonSerializer.Deserialize<List<string>>(match.Gaps) ?? []
                : new List<string>();

            return Results.Ok(new AtsMatchResultDto
            {
                Status         = AtsMatchStatus.Completed.ToString(),
                Provider       = match.Provider.ToString(),
                OverallScore   = match.OverallScore,
                Recommendation = match.Recommendation.ToString(),
                DimensionScores = new AtsMatchDimensionScoresDto
                {
                    Skills         = match.SkillsScore,
                    Experience     = match.ExperienceScore,
                    Location       = match.LocationScore,
                    Salary         = match.SalaryScore,
                    Qualifications = match.QualificationsScore,
                    WorkRights     = match.WorkRightsScore
                },
                Strengths    = strengths,
                Gaps         = gaps,
                Summary      = match.Summary,
                Confidence   = match.Confidence,
                ModelUsed    = match.ModelUsed,
                ProcessedUtc = match.ProcessedUtc
            });
        });

        // ── Re-queue ATS match (employer / recruiter only) ───────────────────
        group.MapPost("/{applicationId:guid}/rematch", async (
            [FromServices] AethonDbContext db,
            [FromServices] ICurrentUserAccessor currentUser,
            [FromServices] ApplicationAccessService accessService,
            [FromServices] AtsPayloadBuilderService atsPayloadBuilder,
            Guid applicationId,
            CancellationToken ct) =>
        {
            if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
                return Results.Unauthorized();

            var canManage = await accessService.CanManageApplicationAsync(currentUser.UserId, applicationId, ct);
            if (!canManage)
                return Results.Forbid();

            var application = await db.JobApplications
                .AsNoTracking()
                .Where(x => x.Id == applicationId)
                .Select(x => new { x.Id, x.UserId, x.JobId })
                .SingleOrDefaultAsync(ct);

            if (application is null)
                return Results.NotFound();

            var job = await db.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == application.JobId, ct);

            if (job is null)
                return Results.NotFound();

            var provider = job.HasAiCandidateMatching ? AtsMatchProvider.Claude : AtsMatchProvider.Ollama;
            var priority = job.HasAiCandidateMatching ? 15 : 5;   // Slightly higher than initial submission

            string payloadJson;
            try
            {
                payloadJson = await atsPayloadBuilder.BuildJsonAsync(job, application.UserId, ct);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to build ATS payload: {ex.Message}");
            }

            var utcNow   = DateTime.UtcNow;
            var newItem  = new AtsMatchQueueItem
            {
                Id               = Guid.NewGuid(),
                JobApplicationId = applicationId,
                JobId            = application.JobId,
                CandidateUserId  = application.UserId,
                Provider         = provider,
                Priority         = priority,
                Status           = AtsMatchStatus.Pending,
                PayloadJson      = payloadJson,
                CreatedUtc       = utcNow,
                CreatedByUserId  = currentUser.UserId
            };

            db.AtsMatchQueue.Add(newItem);
            await db.SaveChangesAsync(ct);

            return Results.Accepted($"/api/v1/applications/{applicationId}/match",
                new { queueItemId = newItem.Id, provider = provider.ToString() });
        });
    }
}
