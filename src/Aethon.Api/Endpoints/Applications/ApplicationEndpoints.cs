using Aethon.Application.Applications.Commands.AddApplicationComment;
using Aethon.Application.Applications.Commands.AddApplicationNote;
using Aethon.Application.Applications.Commands.ChangeApplicationStatus;
using Aethon.Application.Applications.Commands.ScheduleInterview;
using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Applications.Queries.GetApplicationById;
using Aethon.Application.Applications.Queries.GetApplicationTimeline;
using Aethon.Application.Applications.Queries.GetApplicationsForJob;
using Aethon.Application.Applications.Queries.GetMyApplications;
using Microsoft.AspNetCore.Mvc;
using Aethon.Shared.Enums;

namespace Aethon.Api.Endpoints.Applications;

public static class ApplicationEndpoints
{
    public static void MapApplicationEndpointsGroup(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/applications").RequireAuthorization();

        group.MapPost("/", async (
            [FromServices] SubmitJobApplicationHandler handler,
            SubmitJobApplicationCommand command,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/my", async (
            [FromServices] GetMyApplicationsHandler handler,
            int page,
            int pageSize,
            CancellationToken ct) =>
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

        group.MapGet("/{id:guid}", async (
            [FromServices] GetApplicationByIdHandler handler,
            Guid id,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetApplicationByIdQuery { ApplicationId = id }, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/{id:guid}/timeline", async (
            [FromServices] GetApplicationTimelineHandler handler,
            Guid id,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetApplicationTimelineQuery { ApplicationId = id }, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/job/{jobId:guid}", async (
            [FromServices] GetApplicationsForJobHandler handler,
            Guid jobId,
            int page,
            int pageSize,
            ApplicationStatus? status,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetApplicationsForJobQuery
                {
                    JobId = jobId,
                    Page = page,
                    PageSize = pageSize,
                    Status = status
                },
                ct);

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{id:guid}/status", async (
            [FromServices] ChangeApplicationStatusHandler handler,
            Guid id,
            ChangeApplicationStatusCommand command,
            CancellationToken ct) =>
        {
            command = command with { ApplicationId = id };
            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{id:guid}/notes", async (
            [FromServices] AddApplicationNoteHandler handler,
            Guid id,
            AddApplicationNoteCommand command,
            CancellationToken ct) =>
        {
            command = new AddApplicationNoteCommand
            {
                ApplicationId = id,
                Content = command.Content
            };

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{id:guid}/comments", async (
            [FromServices] AddApplicationCommentHandler handler,
            Guid id,
            AddApplicationCommentCommand command,
            CancellationToken ct) =>
        {
            command = new AddApplicationCommentCommand
            {
                ApplicationId = id,
                ParentCommentId = command.ParentCommentId,
                Content = command.Content
            };

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{id:guid}/interviews", async (
            [FromServices] ScheduleInterviewHandler handler,
            Guid id,
            ScheduleInterviewCommand command,
            CancellationToken ct) =>
        {
            command = command with { ApplicationId = id };
            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });
    }
}
