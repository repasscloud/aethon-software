using Aethon.Api.Common;
using Aethon.Api.Infrastructure.Stripe;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Settings;
using Aethon.Application.Abstractions.Syndication;
using Aethon.Application.Common.Caching;
using Aethon.Shared.Utilities;
using Aethon.Application.Applications.Queries.GetApplicationsForJob;
using Aethon.Application.Jobs.Commands.CloseJob;
using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Commands.PublishJob;
using Aethon.Application.Jobs.Commands.PutJobOnHold;
using Aethon.Application.Jobs.Commands.ReturnJobToDraft;
using Aethon.Application.Jobs.Commands.UpdateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Aethon.Application.Jobs.Queries.GetMyOrgJobs;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Jobs;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/jobs")
            .RequireAuthorization()
            .WithTags("Jobs");

        group.MapPost(string.Empty, async (
            [FromServices] CreateJobHandler handler,
            HttpContext httpContext,
            CreateJobCommand command,
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

        group.MapGet("/{jobId:guid}", async (
            [FromServices] GetJobByIdHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetJobByIdQuery
                {
                    JobId = jobId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        // GET /api/v1/jobs/my-org  — must be before /{jobId:guid} to avoid routing conflict
        group.MapGet("/my-org", async (
            [FromServices] GetMyOrgJobsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapPut("/{jobId:guid}", async (
            [FromServices] UpdateJobHandler handler,
            [FromServices] IGoogleIndexingService indexing,
            [FromServices] IConfiguration config,
            [FromServices] AethonDbContext db,
            HttpContext ctx,
            Guid jobId,
            UpdateJobRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(jobId, request, ct);

            // Fire Google Indexing UPDATE if job is published
            if (result.Succeeded)
            {
                var isPublished = await db.Jobs.AsNoTracking()
                    .AnyAsync(j => j.Id == jobId && j.Status == Aethon.Shared.Enums.JobStatus.Published, ct);
                if (isPublished)
                {
                    var baseUrl = config["Email:WebBaseUrl"] ?? config["ApiBaseUrl"] ?? "";
                    await indexing.NotifyUpdatedAsync(jobId, JobUrlHelper.BuildPublicUrl(baseUrl, jobId), ct);
                }
            }

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/publish", async (
            [FromServices] PublishJobHandler handler,
            [FromServices] JobPublishBillingService billing,
            [FromServices] IGoogleIndexingService indexing,
            [FromServices] IConfiguration config,
            AethonDbContext db,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);

            // No posting credits — attempt off-session Stripe charge, then retry
            if (!result.Succeeded && result.ErrorCode == "billing.no_credits")
            {
                var job = await db.Jobs.AsNoTracking()
                    .Where(j => j.Id == jobId)
                    .Select(j => new { j.OwnedByOrganisationId, j.PostingTier })
                    .FirstOrDefaultAsync(ct);

                if (job is null)
                    return result.ToMinimalApiResult();

                var requiredType = job.PostingTier == JobPostingTier.Premium
                    ? CreditType.JobPostingPremium
                    : CreditType.JobPostingStandard;

                var (charged, chargeError) = await billing.ChargeAndGrantPostingCreditAsync(
                    job.OwnedByOrganisationId, requiredType, ct);

                if (!charged)
                    return Results.Problem(
                        title: "Payment required",
                        detail: chargeError ?? "No posting credits available and payment failed.",
                        statusCode: 402);

                // Credit was granted — retry the publish handler
                result = await handler.HandleAsync(jobId, ct);
            }

            if (result.Succeeded)
            {
                var baseUrl = config["Email:WebBaseUrl"] ?? config["ApiBaseUrl"] ?? "";
                await indexing.NotifyPublishedAsync(jobId, JobUrlHelper.BuildPublicUrl(baseUrl, jobId), ct);
            }

            return result.ToMinimalApiResult();
        });

        // POST /jobs/{jobId}/addons — apply paid add-ons to a published job
        group.MapPost("/{jobId:guid}/addons", async (
            [FromServices] JobAddonBillingService addonBilling,
            [FromServices] IAppCache cache,
            [FromServices] ICurrentUserAccessor currentUser,
            AethonDbContext db,
            Guid jobId,
            JobAddonRequestDto request,
            CancellationToken ct) =>
        {
            var job = await db.Jobs
                .Include(j => j.OwnedByOrganisation)
                .FirstOrDefaultAsync(j => j.Id == jobId, ct);

            if (job is null)
                return Results.NotFound(new { code = "jobs.not_found" });

            if (job.Status is not (JobStatus.Published or JobStatus.OnHold))
                return Results.BadRequest(new { code = "jobs.invalid_status", message = "Add-ons can only be applied to published or on-hold jobs." });

            var isPremium  = job.PostingTier == JobPostingTier.Premium;
            var isVerified = job.OwnedByOrganisation.IsVerified;
            var orgId      = job.OwnedByOrganisationId;
            var now        = DateTime.UtcNow;
            var errors     = new List<string>();

            // ── Highlight colour ──────────────────────────────────────────────
            if (request.AddHighlight && !string.IsNullOrWhiteSpace(request.HighlightColour))
            {
                var alreadyHighlighted = job.IsHighlighted;
                if (!alreadyHighlighted && !isPremium)
                {
                    var (ok, err) = await addonBilling.ChargeAddonAsync(
                        orgId, SystemSettingKeys.StripePriceAddonHighlight, "Highlight colour add-on", ct);
                    if (!ok) errors.Add($"Highlight: {err}");
                }

                if (errors.Count == 0)
                {
                    job.IsHighlighted    = true;
                    job.HighlightColour  = request.HighlightColour.Trim();
                }
            }

            // ── AI candidate matching ─────────────────────────────────────────
            if (request.AddAiMatching && errors.Count == 0)
            {
                if (!job.HasAiCandidateMatching && !isPremium)
                {
                    var (ok, err) = await addonBilling.ChargeAddonAsync(
                        orgId, SystemSettingKeys.StripePriceAddonAiMatching, "AI candidate matching add-on", ct);
                    if (!ok) errors.Add($"AI matching: {err}");
                }

                if (errors.Count == 0)
                    job.HasAiCandidateMatching = true;
            }

            // ── Sticky top ────────────────────────────────────────────────────
            if (request.StickyDuration > 0 && errors.Count == 0)
            {
                var stickyType = request.StickyDuration switch
                {
                    1  => CreditType.StickyTop24h,
                    7  => CreditType.StickyTop7d,
                    _  => CreditType.StickyTop30d
                };

                // Only charge if not already sticky for a future date
                var needsSticky = !job.StickyUntilUtc.HasValue || job.StickyUntilUtc.Value <= now;
                if (needsSticky)
                {
                    var (ok, err) = await addonBilling.ConsumeOrChargeStickyAsync(
                        orgId, jobId, stickyType, isVerified, ct);
                    if (!ok) errors.Add($"Sticky: {err}");
                }

                if (errors.Count == 0)
                    job.StickyUntilUtc = now.AddDays(request.StickyDuration);
            }

            if (errors.Count > 0)
                return Results.Problem(
                    title: "Payment required",
                    detail: string.Join(" | ", errors),
                    statusCode: 402);

            job.UpdatedUtc      = now;
            job.UpdatedByUserId = currentUser.IsAuthenticated ? currentUser.UserId : null;
            await db.SaveChangesAsync(ct);
            await cache.RemoveAsync(CacheKeys.JobDetail(jobId), ct);

            return Results.Ok(new
            {
                isHighlighted         = job.IsHighlighted,
                highlightColour       = job.HighlightColour,
                hasAiCandidateMatching = job.HasAiCandidateMatching,
                stickyUntilUtc        = job.StickyUntilUtc
            });
        });

        group.MapPost("/{jobId:guid}/close", async (
            [FromServices] CloseJobHandler handler,
            [FromServices] IGoogleIndexingService indexing,
            [FromServices] IConfiguration config,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);

            if (result.Succeeded)
            {
                var baseUrl = config["Email:WebBaseUrl"] ?? config["ApiBaseUrl"] ?? "";
                await indexing.NotifyRemovedAsync(jobId, JobUrlHelper.BuildPublicUrl(baseUrl, jobId), ct);
            }

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/return-to-draft", async (
            [FromServices] ReturnJobToDraftHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/put-on-hold", async (
            [FromServices] PutJobOnHoldHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/{jobId:guid}/applications", async (
            [FromServices] GetApplicationsForJobHandler handler,
            Guid jobId,
            ApplicationStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await handler.HandleAsync(
                new GetApplicationsForJobQuery
                {
                    JobId = jobId,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                },
                ct);

            return result.ToMinimalApiResult();
        });
    }
}
