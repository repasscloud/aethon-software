using Aethon.Api.Common;
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
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

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
    }
}
