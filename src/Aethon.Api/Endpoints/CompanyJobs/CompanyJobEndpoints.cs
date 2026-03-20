using Aethon.Api.Common;
using Aethon.Application.CompanyJobs.Commands.ApproveRecruiterJob;
using Aethon.Application.CompanyJobs.Commands.RejectRecruiterJob;
using Aethon.Application.CompanyJobs.Queries.GetPendingJobApprovals;
using Aethon.Shared.CompanyJobsApproval;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.CompanyJobs;

public static class CompanyJobEndpoints
{
    public static void MapCompanyJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company/jobs/approvals")
            .RequireAuthorization()
            .WithTags("CompanyJobs");

        group.MapGet(string.Empty, async (
            [FromServices] GetPendingJobApprovalsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/approve", async (
            [FromServices] ApproveRecruiterJobHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/reject", async (
            [FromServices] RejectRecruiterJobHandler handler,
            HttpContext ctx,
            Guid jobId,
            RejectJobApprovalDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(jobId, request, ct);
            return result.ToMinimalApiResult();
        });
    }
}
