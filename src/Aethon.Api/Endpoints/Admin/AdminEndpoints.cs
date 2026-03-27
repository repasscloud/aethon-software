using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        // All staff roles can access the admin group; per-endpoint checks enforce finer permissions.
        var group = app.MapGroup("/admin")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Admin", "Support"))
            .WithTags("Admin");

        // GET /api/v1/admin/stats
        group.MapGet("/stats", async (AethonDbContext db, CancellationToken ct) =>
        {
            var orgCount   = await db.Organisations.CountAsync(ct);
            var jobCount   = await db.Jobs.CountAsync(ct);
            var userCount  = await db.Users.CountAsync(ct);
            var fileCount  = await db.StoredFiles.CountAsync(ct);
            var verifiedCount = await db.Organisations
                .CountAsync(o => o.VerificationTier != VerificationTier.None, ct);
            var pendingStripeEvents = await db.StripePaymentEvents
                .CountAsync(e => e.Status == StripeEventStatus.Pending, ct);

            return Results.Ok(new
            {
                organisations = orgCount,
                jobs = jobCount,
                users = userCount,
                files = fileCount,
                verifiedOrganisations = verifiedCount,
                pendingStripeEvents
            });
        });

        // GET /api/v1/admin/organisations?search=&page=
        group.MapGet("/organisations", async (
            AethonDbContext db,
            string? search,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 50;
            var query = db.Organisations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToUpperInvariant();
                query = query.Where(o => o.NormalizedName.Contains(s) || (o.Slug != null && o.Slug.Contains(search.Trim().ToLower())));
            }

            var total = await query.CountAsync(ct);
            var orgs = await query
                .OrderBy(o => o.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.Slug,
                    o.Type,
                    o.Status,
                    o.VerificationTier,
                    o.VerifiedUtc,
                    o.IsPublicProfileEnabled,
                    o.CreatedUtc,
                    JobCount = o.OwnedJobs.Count
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items = orgs });
        });

        // GET /api/v1/admin/organisations/{orgId}
        group.MapGet("/organisations/{orgId:guid}", async (
            AethonDbContext db,
            Guid orgId,
            CancellationToken ct) =>
        {
            var org = await db.Organisations
                .AsNoTracking()
                .Where(o => o.Id == orgId)
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.LegalName,
                    o.Slug,
                    o.Type,
                    o.Status,
                    o.ClaimStatus,
                    o.VerificationTier,
                    o.VerifiedUtc,
                    o.VerifiedByUserId,
                    o.IsPublicProfileEnabled,
                    o.IsEqualOpportunityEmployer,
                    o.IsAccessibleWorkplace,
                    o.WebsiteUrl,
                    o.PublicLocationText,
                    o.PublicContactEmail,
                    o.PublicContactPhone,
                    o.PrimaryContactName,
                    o.PrimaryContactEmail,
                    o.PrimaryContactPhone,
                    o.LogoUrl,
                    o.CompanySize,
                    o.Industry,
                    o.ClaimedByUserId,
                    o.ClaimedUtc,
                    o.CreatedUtc,
                    JobCount = o.OwnedJobs.Count,
                    MemberCount = o.Memberships.Count
                })
                .FirstOrDefaultAsync(ct);

            if (org is null)
                return Results.NotFound(new { code = "organisations.not_found", message = "Organisation not found." });

            return Results.Ok(org);
        });

        // PUT /api/v1/admin/organisations/{orgId}/verification — SuperAdmin + Admin only
        group.MapPut("/organisations/{orgId:guid}/verification", async (
            HttpContext http,
            AethonDbContext db,
            [FromServices] ISystemSettingsService settings,
            Guid orgId,
            SetVerificationTierRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var org = await db.Organisations.FirstOrDefaultAsync(o => o.Id == orgId, ct);
            if (org is null)
                return Results.NotFound(new { code = "organisations.not_found", message = "Organisation not found." });

            var now = DateTime.UtcNow;
            var wasUnverified = org.VerificationTier == VerificationTier.None;
            org.VerificationTier = request.Tier;
            org.VerifiedUtc = request.Tier != VerificationTier.None ? now : null;
            org.UpdatedUtc = now;

            // Convert unused Standard promo credits → Premium when admin verifies the org
            // (respects the Feature.VerificationUpgradesPromoCredits toggle)
            if (request.Tier != VerificationTier.None && wasUnverified)
            {
                var shouldConvert = await settings.GetBoolAsync(
                    SystemSettingKeys.FeatureVerificationUpgradesPromoCredits, defaultValue: true, ct: ct);
                if (shouldConvert)
                {
                    var promoCredits = await db.OrganisationJobCredits
                        .Where(c =>
                            c.OrganisationId == orgId &&
                            c.CreditType == CreditType.JobPostingStandard &&
                            c.Source == CreditSource.LaunchPromotion &&
                            c.QuantityRemaining > 0 &&
                            c.ConvertedAt == null &&
                            (c.ExpiresAt == null || c.ExpiresAt > now))
                        .ToListAsync(ct);

                    foreach (var credit in promoCredits)
                    {
                        credit.CreditType = CreditType.JobPostingPremium;
                        credit.ConvertedAt = now;
                    }
                }
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { organisationId = orgId, tier = request.Tier.ToString() });
        });

        // GET /api/v1/admin/organisations/{orgId}/partnerships
        group.MapGet("/organisations/{orgId:guid}/partnerships", async (
            AethonDbContext db,
            Guid orgId,
            CancellationToken ct) =>
        {
            var partnerships = await db.OrganisationRecruitmentPartnerships
                .AsNoTracking()
                .Where(p => p.CompanyOrganisationId == orgId || p.RecruiterOrganisationId == orgId)
                .Select(p => new
                {
                    p.Id,
                    p.CompanyOrganisationId,
                    CompanyName = p.CompanyOrganisation.Name,
                    p.RecruiterOrganisationId,
                    RecruiterName = p.RecruiterOrganisation.Name,
                    p.Status,
                    p.Scope,
                    p.RecruiterCanCreateUnclaimedCompanyJobs,
                    p.RecruiterCanPublishJobs,
                    p.RecruiterCanManageCandidates,
                    p.RequestedByUserId,
                    p.ApprovedByUserId,
                    p.ApprovedUtc,
                    p.Notes,
                    p.CreatedUtc
                })
                .ToListAsync(ct);

            return Results.Ok(partnerships);
        });

        // PUT /api/v1/admin/organisations/{orgId}/partnerships/{pid}/status
        group.MapPut("/organisations/{orgId:guid}/partnerships/{pid:guid}/status", async (
            AethonDbContext db,
            Guid orgId,
            Guid pid,
            SetPartnershipStatusRequest request,
            CancellationToken ct) =>
        {
            var updated = await db.OrganisationRecruitmentPartnerships
                .Where(p => p.Id == pid && (p.CompanyOrganisationId == orgId || p.RecruiterOrganisationId == orgId))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, request.Status), ct);

            if (updated == 0)
                return Results.NotFound(new { code = "partnerships.not_found", message = "Partnership not found." });

            return Results.Ok(new { partnershipId = pid, status = request.Status.ToString() });
        });

        // POST /api/v1/admin/organisations/{orgId}/partnerships — Admin+ only
        group.MapPost("/organisations/{orgId:guid}/partnerships", async (
            HttpContext http,
            AethonDbContext db,
            Guid orgId,
            CreatePartnershipRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            // Verify the company org exists
            var companyExists = await db.Organisations.AnyAsync(o => o.Id == orgId, ct);
            if (!companyExists)
                return Results.NotFound(new { code = "organisations.not_found", message = "Company organisation not found." });

            // Verify recruiter org exists
            var recruiterExists = await db.Organisations.AnyAsync(o => o.Id == request.RecruiterOrganisationId, ct);
            if (!recruiterExists)
                return Results.NotFound(new { code = "organisations.recruiter_not_found", message = "Recruiter organisation not found." });

            // No duplicate
            var alreadyExists = await db.OrganisationRecruitmentPartnerships.AnyAsync(
                p => p.CompanyOrganisationId == orgId && p.RecruiterOrganisationId == request.RecruiterOrganisationId, ct);
            if (alreadyExists)
                return Results.Conflict(new { code = "partnerships.duplicate", message = "Partnership already exists." });

            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userGuid = userId != null && Guid.TryParse(userId, out var g) ? (Guid?)g : null;

            var partnership = new OrganisationRecruitmentPartnership
            {
                Id = Guid.NewGuid(),
                CompanyOrganisationId = orgId,
                RecruiterOrganisationId = request.RecruiterOrganisationId,
                Status = request.Status,
                Scope = request.Scope,
                RecruiterCanCreateUnclaimedCompanyJobs = request.RecruiterCanCreateUnclaimedCompanyJobs,
                RecruiterCanPublishJobs = request.RecruiterCanPublishJobs,
                RecruiterCanManageCandidates = request.RecruiterCanManageCandidates,
                RequestedByUserId = userGuid,
                ApprovedByUserId = request.Status == OrganisationRecruitmentPartnershipStatus.Active ? userGuid : null,
                ApprovedUtc = request.Status == OrganisationRecruitmentPartnershipStatus.Active ? DateTime.UtcNow : null,
                Notes = request.Notes,
                CreatedByUserId = userGuid,
                CreatedUtc = DateTime.UtcNow
            };

            db.OrganisationRecruitmentPartnerships.Add(partnership);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { partnershipId = partnership.Id });
        });

        // DELETE /api/v1/admin/organisations/{orgId}/partnerships/{pid} — Support+
        group.MapDelete("/organisations/{orgId:guid}/partnerships/{pid:guid}", async (
            AethonDbContext db,
            Guid orgId,
            Guid pid,
            CancellationToken ct) =>
        {
            var deleted = await db.OrganisationRecruitmentPartnerships
                .Where(p => p.Id == pid && (p.CompanyOrganisationId == orgId || p.RecruiterOrganisationId == orgId))
                .ExecuteDeleteAsync(ct);

            if (deleted == 0)
                return Results.NotFound(new { code = "partnerships.not_found", message = "Partnership not found." });

            return Results.NoContent();
        });

        // GET /api/v1/admin/jobs?search=&status=&page=
        group.MapGet("/jobs", async (
            AethonDbContext db,
            string? search,
            JobStatus? status,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 50;
            var query = db.Jobs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(s));
            }

            if (status.HasValue)
                query = query.Where(j => j.Status == status.Value);

            var total = await query.CountAsync(ct);
            var jobs = await query
                .OrderByDescending(j => j.CreatedUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Status,
                    j.Visibility,
                    j.Category,
                    j.LocationText,
                    j.EmploymentType,
                    j.WorkplaceType,
                    j.PublishedUtc,
                    j.CreatedUtc,
                    j.ApplyByUtc,
                    OrganisationId = j.OwnedByOrganisationId,
                    OrganisationName = j.OwnedByOrganisation.Name,
                    ApplicationCount = j.Applications.Count
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items = jobs });
        });

        // GET /api/v1/admin/jobs/{jobId}
        group.MapGet("/jobs/{jobId:guid}", async (
            AethonDbContext db,
            Guid jobId,
            CancellationToken ct) =>
        {
            var job = await db.Jobs
                .AsNoTracking()
                .Where(j => j.Id == jobId)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Status,
                    j.Visibility,
                    j.Category,
                    j.Department,
                    j.ReferenceCode,
                    j.ExternalReference,
                    j.LocationText,
                    j.LocationCity,
                    j.LocationState,
                    j.LocationCountry,
                    j.WorkplaceType,
                    j.EmploymentType,
                    j.Description,
                    j.Summary,
                    j.Requirements,
                    j.Benefits,
                    j.SalaryFrom,
                    j.SalaryTo,
                    j.SalaryCurrency,
                    j.ExternalApplicationUrl,
                    j.ApplicationEmail,
                    j.PostingTier,
                    j.PostingExpiresUtc,
                    j.ApplyByUtc,
                    j.PublishedUtc,
                    j.ClosedUtc,
                    j.CreatedUtc,
                    j.IsHighlighted,
                    j.IncludeCompanyLogo,
                    j.StatusReason,
                    OrganisationId = j.OwnedByOrganisationId,
                    OrganisationName = j.OwnedByOrganisation.Name,
                    OrganisationSlug = j.OwnedByOrganisation.Slug,
                    ApplicationCount = j.Applications.Count
                })
                .FirstOrDefaultAsync(ct);

            if (job is null)
                return Results.NotFound(new { code = "job.not_found", message = "Job not found." });

            return Results.Ok(job);
        });

        // GET /api/v1/admin/users?search=&page=
        group.MapGet("/users", async (
            AethonDbContext db,
            string? search,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 50;
            var query = db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u => u.Email!.ToLower().Contains(s) || u.DisplayName.ToLower().Contains(s));
            }

            var total = await query.CountAsync(ct);
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.DisplayName,
                    u.UserType,
                    u.IsEnabled,
                    u.IsIdentityVerified,
                    OrganisationName = db.OrganisationMemberships
                        .Where(m => m.UserId == u.Id && m.Status == MembershipStatus.Active)
                        .Select(m => m.Organisation.Name)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items = users });
        });

        // GET /api/v1/admin/users/{userId}
        group.MapGet("/users/{userId:guid}", async (
            AethonDbContext db,
            UserManager<ApplicationUser> userManager,
            Guid userId,
            CancellationToken ct) =>
        {
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user is null)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            var roles = await userManager.GetRolesAsync(user);

            var memberships = await db.OrganisationMemberships
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .Select(m => new
                {
                    m.OrganisationId,
                    OrganisationName = m.Organisation.Name,
                    OrganisationType = m.Organisation.Type,
                    m.Status,
                    m.IsOwner,
                    m.CompanyRole,
                    m.RecruiterRole,
                    m.JoinedUtc
                })
                .ToListAsync(ct);

            return Results.Ok(new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.UserType,
                user.IsEnabled,
                user.IsIdentityVerified,
                user.IdentityVerifiedUtc,
                user.IdentityVerificationNotes,
                user.IsPhoneNumberVerified,
                user.PhoneNumberConfirmed,
                user.EmailConfirmed,
                user.LockoutEnd,
                user.AccessFailedCount,
                Roles = roles,
                Memberships = memberships
            });
        });

        // POST /api/v1/admin/users — create Admin or Support account (SuperAdmin only)
        group.MapPost("/users", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration config,
            CreateStaffUserRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin"))
                return Results.Forbid();

            if (request.Role is not ("Admin" or "Support"))
                return Results.BadRequest(new { code = "users.invalid_role", message = "Role must be Admin or Support." });

            var userType = request.Role == "Admin"
                ? UserAccountType.Admin
                : UserAccountType.Support;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                UserType = userType,
                IsEnabled = true,
                EmailConfirmed = true,
                MustChangePassword = true  // Force password change on first login
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return Results.ValidationProblem(errors);
            }

            await userManager.AddToRoleAsync(user, request.Role);

            // Send invitation email if MailerSend is configured
            var webBaseUrl = config["WebBaseUrl"] ?? "https://localhost";
            await emailService.SendAsync(new EmailMessage
            {
                ToEmail = request.Email,
                ToName = request.DisplayName,
                Subject = "Your Aethon staff account has been created",
                TextBody = $"Hi {request.DisplayName},\n\nYour {request.Role} account has been created on the Aethon platform.\n\nLogin at {webBaseUrl}/login using:\nEmail: {request.Email}\nPassword: (the password set by your administrator)\n\nYou will be prompted to change your password on first login.\n\nAethon Team",
                HtmlBody = $"<p>Hi {request.DisplayName},</p><p>Your <strong>{request.Role}</strong> account has been created on the Aethon platform.</p><p>Login at <a href=\"{webBaseUrl}/login\">{webBaseUrl}/login</a> using:<br/>Email: <strong>{request.Email}</strong><br/>Password: <em>(the password set by your administrator)</em></p><p>You will be prompted to set a new password on first login.</p><p>Aethon Team</p>"
            }, ct);

            return Results.Ok(new { userId = user.Id, email = user.Email, role = request.Role });
        });

        // PUT /api/v1/admin/users/{userId}/status — SuperAdmin + Admin only
        group.MapPut("/users/{userId:guid}/status", async (
            HttpContext http,
            AethonDbContext db,
            Guid userId,
            SetUserStatusRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var updated = await db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsEnabled, request.IsEnabled), ct);

            if (updated == 0)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            return Results.Ok(new { userId, isEnabled = request.IsEnabled });
        });

        // POST /api/v1/admin/users/{userId}/reset-password — SuperAdmin + Admin + Support
        group.MapPost("/users/{userId:guid}/reset-password", async (
            UserManager<ApplicationUser> userManager,
            Guid userId,
            ResetPasswordRequest request) =>
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return Results.ValidationProblem(errors);
            }

            return Results.Ok(new { userId, message = "Password reset successfully." });
        });

        // POST /api/v1/admin/users/{userId}/verify-email — SuperAdmin + Admin + Support
        group.MapPost("/users/{userId:guid}/verify-email", async (
            UserManager<ApplicationUser> userManager,
            Guid userId) =>
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return Results.BadRequest(new { code = "users.email_confirm_failed", message = "Failed to confirm email." });

            return Results.Ok(new { userId, emailConfirmed = true });
        });

        // POST /api/v1/admin/users/{userId}/verify-identity — SuperAdmin + Admin + Support
        group.MapPost("/users/{userId:guid}/verify-identity", async (
            AethonDbContext db,
            Guid userId,
            VerifyIdentityRequest request,
            CancellationToken ct) =>
        {
            var updated = await db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.IsIdentityVerified, true)
                    .SetProperty(u => u.IdentityVerifiedUtc, DateTime.UtcNow)
                    .SetProperty(u => u.IdentityVerificationNotes, request.Notes),
                    ct);

            if (updated == 0)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            return Results.Ok(new { userId, identityVerified = true });
        });

        // POST /api/v1/admin/users/{userId}/force-password-change — all staff
        group.MapPost("/users/{userId:guid}/force-password-change", async (
            AethonDbContext db,
            Guid userId,
            CancellationToken ct) =>
        {
            var updated = await db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.MustChangePassword, true), ct);

            if (updated == 0)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            return Results.Ok(new { userId, mustChangePassword = true });
        });

        // POST /api/v1/admin/users/{userId}/force-mfa-setup — all staff
        group.MapPost("/users/{userId:guid}/force-mfa-setup", async (
            AethonDbContext db,
            Guid userId,
            CancellationToken ct) =>
        {
            var updated = await db.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.MustEnableMfa, true), ct);

            if (updated == 0)
                return Results.NotFound(new { code = "users.not_found", message = "User not found." });

            return Results.Ok(new { userId, mustEnableMfa = true });
        });

        // ── Identity verification requests ────────────────────────────────────

        // GET /api/v1/admin/verification-requests?status=&page= — all staff
        group.MapGet("/verification-requests", async (
            AethonDbContext db,
            string? status,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 50;
            var query = db.IdentityVerificationRequests.AsNoTracking().Include(r => r.User);

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<Aethon.Shared.Enums.VerificationRequestStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                query = query.Where(r => r.Status == parsedStatus);
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(r => r.CreatedUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    UserId = r.UserId,
                    UserDisplayName = r.User.DisplayName,
                    UserEmail = r.User.Email,
                    r.FullName,
                    r.EmailAddress,
                    r.PhoneNumber,
                    r.AdditionalNotes,
                    Status = r.Status.ToString(),
                    RequestedUtc = r.CreatedUtc,
                    r.ReviewedUtc,
                    r.ReviewNotes,
                    ReviewerType = r.ReviewerType == null ? null : r.ReviewerType.ToString()
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items });
        });

        // POST /api/v1/admin/verification-requests/{id}/approve — all staff
        group.MapPost("/verification-requests/{id:guid}/approve", async (
            AethonDbContext db,
            HttpContext ctx,
            Guid id,
            ReviewVerificationRequest? request,
            CancellationToken ct) =>
        {
            var userIdStr = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var reviewerId))
                return Results.Unauthorized();

            var vr = await db.IdentityVerificationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (vr is null)
                return Results.NotFound(new { code = "verification.not_found", message = "Verification request not found." });

            if (vr.Status != Aethon.Shared.Enums.VerificationRequestStatus.Pending)
                return Results.BadRequest(new { code = "verification.not_pending", message = "Only pending requests can be approved." });

            vr.Status = Aethon.Shared.Enums.VerificationRequestStatus.Approved;
            vr.ReviewedByUserId = reviewerId;
            vr.ReviewedUtc = DateTime.UtcNow;
            vr.ReviewerType = Aethon.Shared.Enums.VerificationReviewerType.Admin;
            vr.ReviewNotes = request?.Notes;

            await db.Users
                .Where(u => u.Id == vr.UserId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.IsIdentityVerified, true)
                    .SetProperty(u => u.IdentityVerifiedUtc, DateTime.UtcNow)
                    .SetProperty(u => u.IdentityVerificationNotes, $"Approved via admin verification request {id}"),
                    ct);

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { id, approved = true });
        });

        // POST /api/v1/admin/verification-requests/{id}/deny — all staff
        group.MapPost("/verification-requests/{id:guid}/deny", async (
            AethonDbContext db,
            [FromServices] Aethon.Application.Abstractions.Email.IEmailService emailService,
            HttpContext ctx,
            Guid id,
            ReviewVerificationRequest? request,
            CancellationToken ct) =>
        {
            var userIdStr = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var reviewerId))
                return Results.Unauthorized();

            var vr = await db.IdentityVerificationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (vr is null)
                return Results.NotFound(new { code = "verification.not_found", message = "Verification request not found." });

            if (vr.Status != Aethon.Shared.Enums.VerificationRequestStatus.Pending)
                return Results.BadRequest(new { code = "verification.not_pending", message = "Only pending requests can be denied." });

            vr.Status = Aethon.Shared.Enums.VerificationRequestStatus.Denied;
            vr.ReviewedByUserId = reviewerId;
            vr.ReviewedUtc = DateTime.UtcNow;
            vr.ReviewerType = Aethon.Shared.Enums.VerificationReviewerType.Admin;
            vr.ReviewNotes = request?.Notes;

            await db.SaveChangesAsync(ct);

            // Notify the user by email
            if (!string.IsNullOrWhiteSpace(vr.User?.Email))
            {
                try
                {
                    await emailService.SendAsync(new Aethon.Application.Abstractions.Email.EmailMessage
                    {
                        ToEmail = vr.User.Email,
                        ToName = vr.User.DisplayName,
                        Subject = "Your identity verification request — update",
                        TextBody = "Thank you for submitting an identity verification request on Aethon.\n\nUnfortunately, we were unable to verify your identity at this time. If you believe this is an error, please contact us.\n\nAethon Team",
                        HtmlBody = """
                            <!DOCTYPE html><html><body style="font-family:Arial,sans-serif;line-height:1.6;">
                            <h2>Identity verification update</h2>
                            <p>Thank you for submitting an identity verification request on Aethon.</p>
                            <p>Unfortunately, we were unable to verify your identity at this time. If you believe this is an error or would like to try again, please contact us.</p>
                            <p style="color:#666;font-size:0.9em;">— The Aethon Team</p>
                            </body></html>
                            """
                    }, ct);
                }
                catch { /* non-fatal — log would go here */ }
            }

            return Results.Ok(new { id, denied = true });
        });

        // POST /api/v1/admin/verification-requests/process — all staff (trigger)
        // Placeholder: runs the verification worker logic inline.
        // Currently just returns a count of pending requests.
        // Future: plug automated verification logic in here.
        group.MapPost("/verification-requests/process", async (
            AethonDbContext db,
            CancellationToken ct) =>
        {
            var pendingCount = await db.IdentityVerificationRequests
                .CountAsync(r => r.Status == Aethon.Shared.Enums.VerificationRequestStatus.Pending, ct);

            return Results.Ok(new
            {
                pendingCount,
                message = $"{pendingCount} pending request(s) awaiting review. Automated processing not yet enabled."
            });
        });

        // GET /api/v1/admin/files?search=&page=
        group.MapGet("/files", async (
            AethonDbContext db,
            string? search,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 50;
            var query = db.StoredFiles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(f => f.OriginalFileName.ToLower().Contains(s) || f.ContentType.ToLower().Contains(s));
            }

            var total = await query.CountAsync(ct);
            var files = await query
                .OrderByDescending(f => f.CreatedUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new
                {
                    f.Id,
                    f.OriginalFileName,
                    f.FileName,
                    f.ContentType,
                    f.LengthBytes,
                    f.StorageProvider,
                    f.StoragePath,
                    f.UploadedByUserId,
                    f.CreatedUtc,
                    UploaderEmail = db.Users
                        .Where(u => u.Id == f.UploadedByUserId)
                        .Select(u => u.Email)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items = files });
        });

        // GET /api/v1/admin/stripe-events?status=&page=
        group.MapGet("/stripe-events", async (
            AethonDbContext db,
            StripeEventStatus? status,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 50;
            var query = db.StripePaymentEvents.AsNoTracking();

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            var total = await query.CountAsync(ct);
            var events = await query
                .OrderByDescending(e => e.CreatedUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new
                {
                    e.Id,
                    e.StripeEventId,
                    e.EventType,
                    e.AmountTotal,
                    e.Currency,
                    e.CustomerEmail,
                    e.Status,
                    e.InternalNotes,
                    e.CompletedByUserId,
                    e.CompletedUtc,
                    e.CreatedUtc,
                    e.OrganisationId,
                    OrganisationName = e.Organisation != null ? e.Organisation.Name : null,
                    e.PurchaseType,
                    e.PurchaseMetaJson
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items = events });
        });

        // POST /api/v1/admin/stripe-events/{eventId}/approve-verification — Admin+ only
        group.MapPost("/stripe-events/{eventId:guid}/approve-verification", async (
            HttpContext http,
            AethonDbContext db,
            [FromServices] ISystemSettingsService settings,
            Guid eventId,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var stripeEvent = await db.StripePaymentEvents.FirstOrDefaultAsync(e => e.Id == eventId, ct);
            if (stripeEvent is null)
                return Results.NotFound(new { code = "stripe_events.not_found", message = "Event not found." });

            if (stripeEvent.OrganisationId is null)
                return Results.BadRequest(new { code = "stripe_events.no_org", message = "Event has no associated organisation." });

            // Determine verification tier from stored metadata
            var tier = VerificationTier.StandardEmployer;
            if (!string.IsNullOrEmpty(stripeEvent.PurchaseMetaJson))
            {
                try
                {
                    var meta = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(stripeEvent.PurchaseMetaJson);
                    if (meta?.TryGetValue("verification_tier", out var tierStr) == true && tierStr == "enhanced")
                        tier = VerificationTier.EnhancedTrusted;
                }
                catch { }
            }

            var org = await db.Organisations.FirstOrDefaultAsync(o => o.Id == stripeEvent.OrganisationId, ct);
            if (org is null)
                return Results.NotFound(new { code = "organisations.not_found", message = "Organisation not found." });

            var now = DateTime.UtcNow;
            var wasUnverified = org.VerificationTier == VerificationTier.None;

            org.VerificationTier = tier;
            org.VerifiedUtc = now;
            org.UpdatedUtc = now;

            if (wasUnverified)
            {
                var shouldConvert = await settings.GetBoolAsync(
                    SystemSettingKeys.FeatureVerificationUpgradesPromoCredits, defaultValue: true, ct: ct);
                if (shouldConvert)
                {
                    var promoCredits = await db.OrganisationJobCredits
                        .Where(c =>
                            c.OrganisationId == org.Id &&
                            c.CreditType == CreditType.JobPostingStandard &&
                            c.Source == CreditSource.LaunchPromotion &&
                            c.QuantityRemaining > 0 &&
                            c.ConvertedAt == null &&
                            (c.ExpiresAt == null || c.ExpiresAt > now))
                        .ToListAsync(ct);

                    foreach (var credit in promoCredits)
                    {
                        credit.CreditType = CreditType.JobPostingPremium;
                        credit.ConvertedAt = now;
                    }
                }
            }

            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userGuid = userId != null && Guid.TryParse(userId, out var g) ? (Guid?)g : null;

            stripeEvent.Status = StripeEventStatus.Completed;
            stripeEvent.InternalNotes = $"Approved: {tier} verification granted by admin.";
            stripeEvent.CompletedByUserId = userGuid;
            stripeEvent.CompletedUtc = now;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { organisationId = org.Id, tier = tier.ToString() });
        });

        // POST /api/v1/admin/stripe-events/{eventId}/reject-verification — Admin+ only
        group.MapPost("/stripe-events/{eventId:guid}/reject-verification", async (
            HttpContext http,
            AethonDbContext db,
            Guid eventId,
            RejectVerificationRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userGuid = userId != null && Guid.TryParse(userId, out var g) ? (Guid?)g : null;

            var now = DateTime.UtcNow;
            var updated = await db.StripePaymentEvents
                .Where(e => e.Id == eventId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, StripeEventStatus.Reviewed)
                    .SetProperty(e => e.InternalNotes, string.IsNullOrWhiteSpace(request.Reason)
                        ? "Verification rejected by admin."
                        : $"Verification rejected by admin: {request.Reason}")
                    .SetProperty(e => e.CompletedByUserId, userGuid)
                    .SetProperty(e => e.CompletedUtc, now),
                    ct);

            if (updated == 0)
                return Results.NotFound(new { code = "stripe_events.not_found", message = "Event not found." });

            return Results.Ok(new { eventId });
        });

        // PUT /api/v1/admin/stripe-events/{eventId}
        group.MapPut("/stripe-events/{eventId:guid}", async (
            HttpContext http,
            AethonDbContext db,
            Guid eventId,
            UpdateStripeEventRequest request,
            CancellationToken ct) =>
        {
            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userGuid = userId != null && Guid.TryParse(userId, out var g) ? (Guid?)g : null;

            var updated = await db.StripePaymentEvents
                .Where(e => e.Id == eventId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, request.Status)
                    .SetProperty(e => e.InternalNotes, request.InternalNotes)
                    .SetProperty(e => e.CompletedByUserId, request.Status == StripeEventStatus.Completed ? userGuid : null)
                    .SetProperty(e => e.CompletedUtc, request.Status == StripeEventStatus.Completed ? (DateTime?)DateTime.UtcNow : null),
                    ct);

            if (updated == 0)
                return Results.NotFound(new { code = "stripe_events.not_found", message = "Event not found." });

            return Results.Ok(new { eventId, status = request.Status.ToString() });
        });

        // ─── System Settings ──────────────────────────────────────────────────────

        // GET /api/v1/admin/settings
        group.MapGet("/settings", async (
            ISystemSettingsService settings,
            AethonDbContext db,
            HttpContext http,
            CancellationToken ct) =>
        {
            var isSuperAdmin = http.User.IsInRole("SuperAdmin");

            var allSettings = await db.SystemSettings
                .AsNoTracking()
                .OrderBy(s => s.Key)
                .Select(s => new
                {
                    s.Key,
                    // Mask the SA JSON for non-SuperAdmins
                    Value = s.Key == SystemSettingKeys.GoogleIndexingServiceAccount && !isSuperAdmin
                        ? (string.IsNullOrEmpty(s.Value) ? "" : "••••••••")
                        : s.Value,
                    s.Description,
                    s.UpdatedUtc,
                    s.UpdatedByUserId
                })
                .ToListAsync(ct);

            return Results.Ok(allSettings);
        });

        // PUT /api/v1/admin/settings/{key}
        group.MapPut("/settings/{key}", async (
            string key,
            UpdateSettingRequest request,
            ISystemSettingsService settings,
            HttpContext http,
            CancellationToken ct) =>
        {
            // Only SuperAdmin can update the Service Account JSON
            if (key == SystemSettingKeys.GoogleIndexingServiceAccount && !http.User.IsInRole("SuperAdmin"))
                return Results.Forbid();

            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userGuid = userId != null && Guid.TryParse(userId, out var g) ? (Guid?)g : null;

            await settings.SetAsync(key, request.Value, updatedByUserId: userGuid, ct: ct);

            return Results.Ok(new { key, updated = true });
        });

        // ─── System Logs ─────────────────────────────────────────────────────────

        // GET /api/v1/admin/logs
        group.MapGet("/logs", async (
            AethonDbContext db,
            SystemLogLevel? level,
            string? category,
            string? search,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 200);
            var query = db.SystemLogs.AsNoTracking().AsQueryable();

            if (level.HasValue)
                query = query.Where(l => l.Level == level.Value);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(l => l.Category == category);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.Message.Contains(search) || (l.Details != null && l.Details.Contains(search)));

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(l => l.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.TimestampUtc,
                    l.Level,
                    l.Category,
                    l.Message,
                    l.Details,
                    l.ExceptionType,
                    l.ExceptionMessage,
                    l.UserId,
                    l.RequestPath
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items });
        });

        // DELETE /api/v1/admin/logs
        // Purges logs older than N days (default 30). SuperAdmin only.
        group.MapDelete("/logs", async (
            AethonDbContext db,
            HttpContext http,
            int olderThanDays = 30,
            CancellationToken ct = default) =>
        {
            if (!http.User.IsInRole("SuperAdmin"))
                return Results.Forbid();

            var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
            var deleted = await db.SystemLogs
                .Where(l => l.TimestampUtc < cutoff)
                .ExecuteDeleteAsync(ct);

            return Results.Ok(new { deleted, cutoffDate = cutoff });
        });

        // ─── Locations ────────────────────────────────────────────────────────────

        // GET /api/v1/admin/locations
        group.MapGet("/locations", async (
            AethonDbContext db,
            string? search,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 100;
            var query = db.Locations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(l => l.DisplayName.ToLower().Contains(s));
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.DisplayName,
                    l.City,
                    l.State,
                    l.Country,
                    l.CountryCode,
                    l.Latitude,
                    l.Longitude,
                    l.IsActive,
                    l.SortOrder,
                    l.CreatedUtc
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items });
        });

        // POST /api/v1/admin/locations — add single location
        group.MapPost("/locations", async (
            HttpContext http,
            AethonDbContext db,
            UpsertLocationRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var nextOrder = await db.Locations.AnyAsync(ct)
                ? await db.Locations.MaxAsync(l => l.SortOrder, ct) + 1
                : 1;

            var location = new Location
            {
                Id = Guid.NewGuid(),
                DisplayName = request.DisplayName.Trim(),
                City = request.City?.Trim(),
                State = request.State?.Trim(),
                Country = request.Country?.Trim(),
                CountryCode = request.CountryCode?.Trim().ToUpper(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsActive = true,
                SortOrder = nextOrder,
                CreatedUtc = DateTime.UtcNow
            };

            db.Locations.Add(location);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { locationId = location.Id, displayName = location.DisplayName });
        });

        // POST /api/v1/admin/locations/bulk — add multiple locations at once
        group.MapPost("/locations/bulk", async (
            HttpContext http,
            AethonDbContext db,
            List<UpsertLocationRequest> requests,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var nextOrder = await db.Locations.AnyAsync(ct)
                ? await db.Locations.MaxAsync(l => l.SortOrder, ct) + 1
                : 1;

            var locations = requests.Select((r, i) => new Location
            {
                Id = Guid.NewGuid(),
                DisplayName = r.DisplayName.Trim(),
                City = r.City?.Trim(),
                State = r.State?.Trim(),
                Country = r.Country?.Trim(),
                CountryCode = r.CountryCode?.Trim().ToUpper(),
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                IsActive = true,
                SortOrder = nextOrder + i,
                CreatedUtc = DateTime.UtcNow
            }).ToList();

            db.Locations.AddRange(locations);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { added = locations.Count });
        });

        // PUT /api/v1/admin/locations/{locationId}
        group.MapPut("/locations/{locationId:guid}", async (
            HttpContext http,
            AethonDbContext db,
            Guid locationId,
            UpsertLocationRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var updated = await db.Locations
                .Where(l => l.Id == locationId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.DisplayName, request.DisplayName.Trim())
                    .SetProperty(l => l.City, request.City)
                    .SetProperty(l => l.State, request.State)
                    .SetProperty(l => l.Country, request.Country)
                    .SetProperty(l => l.CountryCode, request.CountryCode != null ? request.CountryCode.ToUpper() : null)
                    .SetProperty(l => l.Latitude, request.Latitude)
                    .SetProperty(l => l.Longitude, request.Longitude)
                    .SetProperty(l => l.IsActive, request.IsActive),
                    ct);

            if (updated == 0)
                return Results.NotFound(new { code = "locations.not_found", message = "Location not found." });

            return Results.Ok(new { locationId, updated = true });
        });

        // DELETE /api/v1/admin/locations/{locationId}
        group.MapDelete("/locations/{locationId:guid}", async (
            HttpContext http,
            AethonDbContext db,
            Guid locationId,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var deleted = await db.Locations
                .Where(l => l.Id == locationId)
                .ExecuteDeleteAsync(ct);

            if (deleted == 0)
                return Results.NotFound(new { code = "locations.not_found", message = "Location not found." });

            return Results.NoContent();
        });

        // GET /api/v1/admin/organisations/{orgId}/credits
        group.MapGet("/organisations/{orgId:guid}/credits", async (
            AethonDbContext db,
            Guid orgId,
            CancellationToken ct) =>
        {
            var orgExists = await db.Organisations.AnyAsync(o => o.Id == orgId, ct);
            if (!orgExists)
                return Results.NotFound(new { code = "organisations.not_found", message = "Organisation not found." });

            var credits = await db.OrganisationJobCredits
                .AsNoTracking()
                .Where(c => c.OrganisationId == orgId)
                .OrderByDescending(c => c.CreatedUtc)
                .Select(c => new
                {
                    c.Id,
                    c.CreditType,
                    c.Source,
                    c.QuantityOriginal,
                    c.QuantityRemaining,
                    c.ExpiresAt,
                    c.ConvertedAt,
                    c.GrantedByUserId,
                    c.GrantNote,
                    c.StripePaymentEventId,
                    c.CreatedUtc
                })
                .ToListAsync(ct);

            return Results.Ok(credits);
        });

        // POST /api/v1/admin/organisations/{orgId}/credits/grant — Admin+ only
        group.MapPost("/organisations/{orgId:guid}/credits/grant", async (
            HttpContext http,
            AethonDbContext db,
            Guid orgId,
            AdminGrantCreditRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var orgExists = await db.Organisations.AnyAsync(o => o.Id == orgId, ct);
            if (!orgExists)
                return Results.NotFound(new { code = "organisations.not_found", message = "Organisation not found." });

            if (request.Quantity <= 0)
                return Results.BadRequest(new { code = "credits.invalid_quantity", message = "Quantity must be greater than zero." });

            var userId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userGuid = userId != null && Guid.TryParse(userId, out var g) ? (Guid?)g : null;

            var now = DateTime.UtcNow;
            var credit = new OrganisationJobCredit
            {
                Id = Guid.NewGuid(),
                OrganisationId = orgId,
                CreditType = request.CreditType,
                Source = CreditSource.AdminGrant,
                QuantityOriginal = request.Quantity,
                QuantityRemaining = request.Quantity,
                ExpiresAt = request.ExpiresAt,
                GrantedByUserId = userGuid,
                GrantNote = request.Note,
                CreatedUtc = now,
                CreatedByUserId = userGuid
            };

            db.OrganisationJobCredits.Add(credit);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { creditId = credit.Id, organisationId = orgId, creditType = credit.CreditType.ToString(), quantity = request.Quantity });
        });

        // GET /api/v1/admin/syndication-records?page=
        group.MapGet("/syndication-records", async (
            AethonDbContext db,
            int page = 1,
            CancellationToken ct = default) =>
        {
            const int pageSize = 100;

            var records = await db.JobSyndicationRecords
                .AsNoTracking()
                .OrderByDescending(r => r.SubmittedUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.JobId,
                    JobTitle = r.Job.Title,
                    r.Provider,
                    r.Status,
                    r.ExternalRef,
                    r.SubmittedUtc,
                    r.LastAttemptUtc,
                    r.LastErrorMessage
                })
                .ToListAsync(ct);

            return Results.Ok(records);
        });
    }

    public sealed class SetVerificationTierRequest
    {
        public VerificationTier Tier { get; set; }
    }

    public sealed class SetPartnershipStatusRequest
    {
        public OrganisationRecruitmentPartnershipStatus Status { get; set; }
    }

    public sealed class CreatePartnershipRequest
    {
        public Guid RecruiterOrganisationId { get; set; }
        public OrganisationRecruitmentPartnershipStatus Status { get; set; } = OrganisationRecruitmentPartnershipStatus.Active;
        public OrganisationRecruitmentPartnershipScope Scope { get; set; } = OrganisationRecruitmentPartnershipScope.None;
        public bool RecruiterCanCreateUnclaimedCompanyJobs { get; set; }
        public bool RecruiterCanPublishJobs { get; set; }
        public bool RecruiterCanManageCandidates { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class SetUserStatusRequest
    {
        public bool IsEnabled { get; set; }
    }

    public sealed class CreateStaffUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public sealed class VerifyIdentityRequest
    {
        public string? Notes { get; set; }
    }

    public sealed class UpdateStripeEventRequest
    {
        public StripeEventStatus Status { get; set; }
        public string? InternalNotes { get; set; }
    }

    public sealed class UpdateSettingRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public sealed class UpsertLocationRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class RejectVerificationRequest
    {
        public string? Reason { get; set; }
    }

    public sealed class ReviewVerificationRequest
    {
        public string? Notes { get; set; }
    }

    public sealed class AdminGrantCreditRequest
    {
        public CreditType CreditType { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Note { get; set; }
    }
}
