using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.Jobs;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/jobs");

        group.MapPost("/", async (
            [FromServices] CreateJobHandler handler,
            CreateJobCommand command,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/{id:guid}", async (
            [FromServices] GetJobByIdHandler handler,
            Guid id,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetJobByIdQuery { JobId = id }, ct);
            return result.ToMinimalApiResult();
        });
    }
}
