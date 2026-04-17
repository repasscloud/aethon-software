using Aethon.Api.Common;
using Aethon.Application.Abstractions.Settings;
using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Jobs.Commands.EmailJobApplication;
using Aethon.Application.Jobs.Queries.GetPublicJobDetail;
using Aethon.Application.Jobs.Queries.GetPublicJobLocations;
using Aethon.Application.Jobs.Queries.GetPublicJobs;
using Aethon.Application.Candidates.Queries.GetPublicJobSeekerProfile;
using Aethon.Application.Organisations.Queries.GetPublicOrganisationProfile;
using Aethon.Application.Organisations.Queries.GetPublicOrganisationTeam;
using Aethon.Application.Organisations.Queries.GetPublicOrganisationTeamMember;
using Aethon.Application.Organisations.Queries.GetPublicOrganisations;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Public;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/public")
            .AllowAnonymous()
            .WithTags("Public");

        // GET /api/v1/public/features — publicly safe feature flag states
        group.MapGet("/features", async ([FromServices] ISystemSettingsService settings) =>
        {
            var launchPromoEnabled = await settings.GetBoolAsync(
                SystemSettingKeys.FeatureLaunchPromotionEnabled, defaultValue: true);

            return Results.Ok(new { LaunchPromotionEnabled = launchPromoEnabled });
        });

        // GET /api/v1/public/locations?q= — location search from the curated locations table
        group.MapGet("/locations", async (
            AethonDbContext db,
            string? q,
            CancellationToken ct) =>
        {
            var query = db.Locations.AsNoTracking().Where(l => l.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim().ToLower();
                query = query.Where(l =>
                    l.DisplayName.ToLower().Contains(search) ||
                    (l.City != null && l.City.ToLower().Contains(search)) ||
                    (l.State != null && l.State.ToLower().Contains(search)) ||
                    (l.Country != null && l.Country.ToLower().Contains(search)));
            }

            var results = await query
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.DisplayName)
                .Take(10)
                .Select(l => new
                {
                    l.Id,
                    l.DisplayName,
                    l.City,
                    l.State,
                    l.Country,
                    l.CountryCode,
                    l.Latitude,
                    l.Longitude
                })
                .ToListAsync(ct);

            return Results.Ok(results);
        });

        // GET /api/v1/public/job-seekers/{identifier}
        // identifier = slug (Public only) or userId GUID (access rules enforced by handler)
        group.MapGet("/job-seekers/{identifier}", async (
            [FromServices] GetPublicJobSeekerProfileHandler handler,
            string identifier,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(identifier, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/organisations
        group.MapGet("/organisations", async (
            [FromServices] GetPublicOrganisationsHandler handler,
            string? search,
            bool? verifiedOnly,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                search,
                verifiedOnly ?? false,
                page ?? 1,
                pageSize ?? 24,
                ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/organisations/{slug}
        group.MapGet("/organisations/{slug}", async (
            [FromServices] GetPublicOrganisationProfileHandler handler,
            string slug,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(slug, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/organisations/{slug}/team
        group.MapGet("/organisations/{slug}/team", async (
            [FromServices] GetPublicOrganisationTeamHandler handler,
            string slug,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(slug, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/organisations/{slug}/team/{memberSlug}
        group.MapGet("/organisations/{slug}/team/{memberSlug}", async (
            [FromServices] GetPublicOrganisationTeamMemberHandler handler,
            string slug,
            string memberSlug,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(slug, memberSlug, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/jobs/locations?q= — typeahead suggestions from active jobs
        group.MapGet("/jobs/locations", async (
            [FromServices] GetPublicJobLocationsHandler handler,
            string? q,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(q, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/jobs
        group.MapGet("/jobs", async (
            [FromServices] GetPublicJobsHandler handler,
            AethonDbContext db,
            HttpContext httpContext,
            string? city,
            DateRangeFilter? dateRange,
            JobCategory? category,
            JobRegion? region,
            string? country,
            string? keywords,
            string? organisationSlug,
            decimal? salaryMin,
            decimal? salaryMax,
            bool? verifiedOnly,
            WorkplaceType? workplaceType,
            bool? immediateStart,
            bool? showExternalJobs,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            // Resolve the viewer's age group so school-leaver-targeted jobs filter correctly.
            // Unauthenticated requests and non-job-seeker accounts will receive null (school leaver
            // targeted jobs are hidden).
            ApplicantAgeGroup? viewerAgeGroup = null;
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var viewerUserId))
            {
                viewerAgeGroup = await db.JobSeekerProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == viewerUserId)
                    .Select(p => (ApplicantAgeGroup?)p.AgeGroup)
                    .FirstOrDefaultAsync(ct);
            }

            var query = new GetPublicJobsQuery
            {
                City = city,
                DateRange = dateRange,
                Category = category,
                Region = region,
                Country = country,
                Keywords = keywords,
                OrganisationSlug = organisationSlug,
                SalaryMin = salaryMin,
                SalaryMax = salaryMax,
                VerifiedOnly = verifiedOnly ?? false,
                WorkplaceType = workplaceType,
                ImmediateStart = immediateStart ?? false,
                ShowExternalJobs = showExternalJobs ?? true,
                ViewerAgeGroup = viewerAgeGroup,
                Page = page ?? 1,
                PageSize = pageSize ?? 24
            };
            var result = await handler.HandleAsync(query, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/public/jobs/{id}
        group.MapGet("/jobs/{jobId:guid}", async (
            [FromServices] GetPublicJobDetailHandler handler,
            AethonDbContext db,
            HttpContext httpContext,
            Guid jobId,
            CancellationToken ct) =>
        {
            // Resolve viewer age group for school-leaver-targeted job access control
            ApplicantAgeGroup? viewerAgeGroup = null;
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var viewerUserId))
            {
                viewerAgeGroup = await db.JobSeekerProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == viewerUserId)
                    .Select(p => (ApplicantAgeGroup?)p.AgeGroup)
                    .FirstOrDefaultAsync(ct);
            }

            var result = await handler.HandleAsync(jobId, ct, viewerAgeGroup);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/public/jobs/{id}/apply-email  (anonymous — sends CV by email to employer)
        group.MapPost("/jobs/{jobId:guid}/apply-email", async (
            [FromServices] EmailJobApplicationHandler handler,
            Guid jobId,
            EmailJobApplicationRequestDto request,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, request, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/public/jobs/{id}/apply  (requires auth)
        group.MapPost("/jobs/{jobId:guid}/apply", async (
            [FromServices] SubmitJobApplicationHandler handler,
            HttpContext ctx,
            Guid jobId,
            CreateJobApplicationRequestDto request,
            CancellationToken ct) =>
        {
            var command = new SubmitJobApplicationCommand
            {
                JobId = jobId,
                CoverLetter = request.CoverLetter,
                Source = request.Source ?? "AethonPublicBoard",
                ScreeningAnswersJson = request.ScreeningAnswersJson
            };

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        }).RequireAuthorization();

        // GET /api/v1/public/sitemap/stats — counts used by the Web layer to build the sitemap index.
        group.MapGet("/sitemap/stats", async (AethonDbContext db, CancellationToken ct) =>
        {
            const int pageSize = 50_000;
            var utcNow = DateTime.UtcNow;

            var orgCount = await db.Set<Organisation>()
                .AsNoTracking()
                .CountAsync(o => o.IsPublicProfileEnabled
                              && o.Status == OrganisationStatus.Active
                              && o.Slug != null, ct);

            var jobCount = await db.Jobs
                .AsNoTracking()
                .CountAsync(j => j.Status == JobStatus.Published
                              && j.Visibility == JobVisibility.Public
                              && (j.PostingExpiresUtc == null || j.PostingExpiresUtc > utcNow)
                              && !j.IsSchoolLeaverTargeted
                              && j.OwnedByOrganisation.IsPublicProfileEnabled
                              && j.OwnedByOrganisation.Status == OrganisationStatus.Active
                              && j.OwnedByOrganisation.Slug != null, ct);

            return Results.Ok(new
            {
                OrgCount      = orgCount,
                JobCount      = jobCount,
                JobPageSize   = pageSize,
                JobTotalPages = Math.Max(1, (int)Math.Ceiling((double)jobCount / pageSize))
            });
        });

        // GET /api/v1/public/sitemap/orgs — all public org slugs for /sitemaps/organisations.xml.
        // Enhanced-verified orgs are ordered first; no pagination needed (orgs grow slowly).
        group.MapGet("/sitemap/orgs", async (AethonDbContext db, CancellationToken ct) =>
        {
            var orgs = await db.Set<Organisation>()
                .AsNoTracking()
                .Where(o => o.IsPublicProfileEnabled
                         && o.Status == OrganisationStatus.Active
                         && o.Slug != null)
                .OrderByDescending(o => (int)o.VerificationTier)
                .ThenByDescending(o => o.UpdatedUtc ?? o.CreatedUtc)
                .Select(o => new
                {
                    o.Slug,
                    Tier    = (int)o.VerificationTier,
                    LastMod = (o.UpdatedUtc ?? o.CreatedUtc).ToString("yyyy-MM-dd")
                })
                .ToListAsync(ct);

            return Results.Ok(orgs);
        });

        // GET /api/v1/public/sitemap/jobs?page=1 — paginated jobs for /sitemaps/jobs-N.xml.
        // 50,000 URLs per page; enhanced-verified employer jobs appear first.
        group.MapGet("/sitemap/jobs", async (AethonDbContext db, int page, CancellationToken ct) =>
        {
            const int pageSize = 50_000;
            var utcNow = DateTime.UtcNow;
            page = Math.Max(1, page);

            var baseQuery = db.Jobs
                .AsNoTracking()
                .Where(j => j.Status == JobStatus.Published
                         && j.Visibility == JobVisibility.Public
                         && (j.PostingExpiresUtc == null || j.PostingExpiresUtc > utcNow)
                         && !j.IsSchoolLeaverTargeted
                         && j.OwnedByOrganisation.IsPublicProfileEnabled
                         && j.OwnedByOrganisation.Status == OrganisationStatus.Active
                         && j.OwnedByOrganisation.Slug != null)
                .OrderByDescending(j => (int)j.OwnedByOrganisation.VerificationTier)
                .ThenByDescending(j => j.PublishedUtc);

            var totalCount = await baseQuery.CountAsync(ct);
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));

            var items = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    OrgSlug = j.OwnedByOrganisation.Slug,
                    OrgTier = (int)j.OwnedByOrganisation.VerificationTier,
                    LastMod = (j.UpdatedUtc ?? j.PublishedUtc ?? j.CreatedUtc).ToString("yyyy-MM-dd")
                })
                .ToListAsync(ct);

            return Results.Ok(new { Page = page, TotalPages = totalPages, TotalCount = totalCount, Items = items });
        });
    }
}
