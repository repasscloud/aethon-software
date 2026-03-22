using Aethon.Application.Abstractions.Email;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;
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
            Guid orgId,
            SetVerificationTierRequest request,
            CancellationToken ct) =>
        {
            if (!http.User.IsInRole("SuperAdmin") && !http.User.IsInRole("Admin"))
                return Results.Forbid();

            var updated = await db.Organisations
                .Where(o => o.Id == orgId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.VerificationTier, request.Tier)
                    .SetProperty(o => o.VerifiedUtc, request.Tier != VerificationTier.None ? DateTime.UtcNow : (DateTime?)null),
                    ct);

            if (updated == 0)
                return Results.NotFound(new { code = "organisations.not_found", message = "Organisation not found." });

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
                    e.CreatedUtc
                })
                .ToListAsync(ct);

            return Results.Ok(new { total, page, pageSize, items = events });
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
}
