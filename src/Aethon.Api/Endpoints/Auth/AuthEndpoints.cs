using System.Security.Claims;
using Aethon.Api.Auth;
using Aethon.Application.Abstractions.Email;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            UserManager<ApplicationUser> userManager,
            AethonDbContext db,
            RegisterRequest request) =>
        {
            var now = DateTime.UtcNow;
            var normalizedType = request.RegistrationType.Trim().ToLowerInvariant();

            var userType = normalizedType switch
            {
                "company" => UserAccountType.Company,
                "recruiter" => UserAccountType.RecruiterAgency,
                _ => UserAccountType.JobSeeker
            };

            var displayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}".Trim();

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = displayName,
                    UserType = userType
                };

                var result = await userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = result.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                    return Results.ValidationProblem(errors);
                }

                if (normalizedType is "company" or "recruiter")
                {
                    var orgType = normalizedType == "company"
                        ? OrganisationType.Company
                        : OrganisationType.RecruiterAgency;

                    var orgId = Guid.NewGuid();

                    var emailDomain = request.Email.Contains('@')
                        ? request.Email.Split('@')[1].Trim().ToLowerInvariant()
                        : string.Empty;

                    // Save org first without PrimaryDomainId to avoid circular FK dependency
                    var org = new Organisation
                    {
                        Id = orgId,
                        Type = orgType,
                        Status = OrganisationStatus.Active,
                        ClaimStatus = OrganisationClaimStatus.NotApplicable,
                        Name = request.OrganisationName!.Trim(),
                        NormalizedName = request.OrganisationName!.Trim().ToUpperInvariant(),
                        IsPublicProfileEnabled = false,
                        ClaimedByUserId = user.Id,
                        ClaimedUtc = now,
                        PrimaryDomainId = null,
                        CreatedByUserId = user.Id,
                        CreatedUtc = now
                    };

                    db.Set<Organisation>().Add(org);

                    var membership = new OrganisationMembership
                    {
                        Id = Guid.NewGuid(),
                        OrganisationId = orgId,
                        UserId = user.Id,
                        Status = MembershipStatus.Active,
                        IsOwner = true,
                        CompanyRole = orgType == OrganisationType.Company ? CompanyRole.Owner : null,
                        RecruiterRole = orgType == OrganisationType.RecruiterAgency ? RecruiterRole.Owner : null,
                        JoinedUtc = now,
                        CreatedByUserId = user.Id,
                        CreatedUtc = now
                    };

                    db.Set<OrganisationMembership>().Add(membership);

                    // First save: org + membership (no circular dep)
                    await db.SaveChangesAsync();

                    // Now add the domain and link it back.
                    // Since RequiresEmailConfirmation = false (email is implicitly trusted at
                    // registration), we mark the registration domain as Verified immediately.
                    if (!string.IsNullOrEmpty(emailDomain))
                    {
                        var domainId = Guid.NewGuid();

                        var domain = new OrganisationDomain
                        {
                            Id = domainId,
                            OrganisationId = orgId,
                            Domain = emailDomain,
                            NormalizedDomain = emailDomain.ToUpperInvariant(),
                            IsPrimary = true,
                            Status = DomainStatus.Verified,
                            VerificationMethod = DomainVerificationMethod.None,
                            TrustLevel = DomainTrustLevel.High,
                            VerifiedUtc = now,
                            VerifiedByUserId = user.Id,
                            CreatedByUserId = user.Id,
                            CreatedUtc = now
                        };

                        db.Set<OrganisationDomain>().Add(domain);
                        await db.SaveChangesAsync();

                        // Second save: update org's PrimaryDomainId now that domain exists
                        await db.Set<Organisation>()
                            .Where(o => o.Id == orgId)
                            .ExecuteUpdateAsync(s => s.SetProperty(o => o.PrimaryDomainId, domainId));
                    }
                }

                await transaction.CommitAsync();

                return Results.Ok(new RegisterResultDto
                {
                    Succeeded = true,
                    RequiresEmailConfirmation = false,
                    Email = request.Email,
                    DisplayName = displayName,
                    RegistrationType = normalizedType
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        group.MapPost("/login", async (
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            AethonDbContext db,
            LoginRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid email or password." });
            }

            var roles = await userManager.GetRolesAsync(user);
            var isSuperAdmin = roles.Contains("SuperAdmin");
            var isAdmin = roles.Contains("Admin");
            var isSupport = roles.Contains("Support");

            // If 2FA is enabled, return a short-lived ticket — client must complete the 2FA step
            if (user.TwoFactorEnabled)
            {
                var ticket = tokenService.GenerateTwoFactorTicket(user.Id);
                return Results.Ok(new LoginResponse
                {
                    RequiresTwoFactor = true,
                    TwoFactorTicket = ticket
                });
            }

            var token = tokenService.GenerateToken(user, roles);

            // Determine app type from user type and org membership
            var appType = isSuperAdmin ? "superadmin"
                : isAdmin ? "admin"
                : isSupport ? "support"
                : user.UserType switch
                {
                    UserAccountType.JobSeeker => "jobseeker",
                    UserAccountType.Company => "employer",
                    UserAccountType.RecruiterAgency => "recruiter",
                    _ => "jobseeker"
                };

            string? organisationId = null;
            string? organisationName = null;
            string? organisationType = null;
            string? companyRole = null;
            string? recruiterRole = null;
            bool isOwner = false;

            if (user.UserType is UserAccountType.Company or UserAccountType.RecruiterAgency)
            {
                var membership = await db.Set<OrganisationMembership>()
                    .Include(m => m.Organisation)
                    .Where(m => m.UserId == user.Id && m.Status == MembershipStatus.Active)
                    .OrderByDescending(m => m.IsOwner)
                    .FirstOrDefaultAsync();

                if (membership is not null)
                {
                    organisationId = membership.OrganisationId.ToString();
                    organisationName = membership.Organisation.Name;
                    organisationType = membership.Organisation.Type.ToString().ToLowerInvariant();
                    isOwner = membership.IsOwner;
                    companyRole = membership.CompanyRole?.ToString();
                    recruiterRole = membership.RecruiterRole?.ToString();
                }
            }

            // MustEnableMfa is only actionable if 2FA is not already enabled
            var mustEnableMfa = user.MustEnableMfa && !user.TwoFactorEnabled;

            return Results.Ok(new LoginResponse
            {
                Token = token,
                UserId = user.Id.ToString(),
                Email = user.Email!,
                DisplayName = user.DisplayName,
                AppType = appType,
                IsSuperAdmin = isSuperAdmin,
                IsAdmin = isAdmin,
                IsSupport = isSupport,
                MustChangePassword = user.MustChangePassword,
                MustEnableMfa = mustEnableMfa,
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                OrganisationType = organisationType,
                CompanyRole = companyRole,
                RecruiterRole = recruiterRole,
                IsOrganisationOwner = isOwner
            });
        });

        // POST /auth/verify-2fa — complete a 2FA login
        group.MapPost("/verify-2fa", async (
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            AethonDbContext db,
            VerifyTwoFactorRequest request) =>
        {
            var userId = tokenService.ValidateTwoFactorTicket(request.TwoFactorTicket);
            if (userId is null)
                return Results.BadRequest(new { code = "auth.invalid_ticket", message = "Invalid or expired two-factor ticket." });

            var user = await userManager.FindByIdAsync(userId.Value.ToString());
            if (user is null)
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid credentials." });

            var isValid = await userManager.VerifyTwoFactorTokenAsync(
                user, TokenOptions.DefaultAuthenticatorProvider, request.Code);

            if (!isValid)
                return Results.BadRequest(new { code = "auth.invalid_code", message = "Invalid authenticator code." });

            var roles = await userManager.GetRolesAsync(user);
            var isSuperAdmin = roles.Contains("SuperAdmin");
            var isAdmin = roles.Contains("Admin");
            var isSupport = roles.Contains("Support");
            var token = tokenService.GenerateToken(user, roles);

            var appType = isSuperAdmin ? "superadmin"
                : isAdmin ? "admin"
                : isSupport ? "support"
                : user.UserType switch
                {
                    UserAccountType.JobSeeker => "jobseeker",
                    UserAccountType.Company => "employer",
                    UserAccountType.RecruiterAgency => "recruiter",
                    _ => "jobseeker"
                };

            string? organisationId = null;
            string? organisationName = null;
            string? organisationType = null;
            string? companyRole = null;
            string? recruiterRole = null;
            bool isOwner = false;

            if (user.UserType is UserAccountType.Company or UserAccountType.RecruiterAgency)
            {
                var membership = await db.Set<OrganisationMembership>()
                    .Include(m => m.Organisation)
                    .Where(m => m.UserId == user.Id && m.Status == MembershipStatus.Active)
                    .OrderByDescending(m => m.IsOwner)
                    .FirstOrDefaultAsync();

                if (membership is not null)
                {
                    organisationId = membership.OrganisationId.ToString();
                    organisationName = membership.Organisation.Name;
                    organisationType = membership.Organisation.Type.ToString().ToLowerInvariant();
                    isOwner = membership.IsOwner;
                    companyRole = membership.CompanyRole?.ToString();
                    recruiterRole = membership.RecruiterRole?.ToString();
                }
            }

            var mustEnableMfa = user.MustEnableMfa && !user.TwoFactorEnabled;

            return Results.Ok(new LoginResponse
            {
                Token = token,
                UserId = user.Id.ToString(),
                Email = user.Email!,
                DisplayName = user.DisplayName,
                AppType = appType,
                IsSuperAdmin = isSuperAdmin,
                IsAdmin = isAdmin,
                IsSupport = isSupport,
                MustChangePassword = user.MustChangePassword,
                MustEnableMfa = mustEnableMfa,
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                OrganisationType = organisationType,
                CompanyRole = companyRole,
                RecruiterRole = recruiterRole,
                IsOrganisationOwner = isOwner
            });
        });

        // POST /auth/change-password — authenticated, changes the user's own password
        group.MapPost("/change-password", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager,
            AethonDbContext db,
            ChangePasswordRequest request,
            CancellationToken ct) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return Results.ValidationProblem(errors);
            }

            // Clear the force-change flag
            if (user.MustChangePassword)
            {
                await db.Users.Where(u => u.Id == user.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.MustChangePassword, false), ct);
            }

            return Results.Ok(new { message = "Password changed successfully." });
        }).RequireAuthorization();

        // GET /auth/mfa/status — returns whether 2FA is currently enabled
        group.MapGet("/mfa/status", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            return Results.Ok(new { twoFactorEnabled = user.TwoFactorEnabled });
        }).RequireAuthorization();

        // GET /auth/mfa/setup — returns authenticator key + QR code for setup
        group.MapGet("/mfa/setup", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            // Only generate a new key if the user has none yet — avoids invalidating a
            // QR code the user already scanned (Blazor Server calls OnInitializedAsync twice)
            var key = await userManager.GetAuthenticatorKeyAsync(user);
            if (key is null)
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                key = await userManager.GetAuthenticatorKeyAsync(user);
            }
            if (key is null) return Results.Problem("Could not generate authenticator key.");

            var email = Uri.EscapeDataString(user.Email ?? user.UserName ?? "user");
            var issuer = Uri.EscapeDataString("Aethon");
            var uri = $"otpauth://totp/{issuer}:{email}?secret={key}&issuer={issuer}&digits=6";

            // Generate QR code as base64 PNG
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(uri, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(10);
            var qrBase64 = Convert.ToBase64String(qrBytes);

            return Results.Ok(new
            {
                authenticatorUri = uri,
                key,
                qrCodeBase64 = qrBase64,
                twoFactorEnabled = user.TwoFactorEnabled
            });
        }).RequireAuthorization();

        // POST /auth/mfa/setup/reset-key — explicitly regenerates the authenticator key
        group.MapPost("/mfa/setup/reset-key", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            await userManager.ResetAuthenticatorKeyAsync(user);

            return Results.Ok(new { message = "Authenticator key reset." });
        }).RequireAuthorization();

        // POST /auth/mfa/setup — verifies a TOTP code and enables 2FA
        group.MapPost("/mfa/setup", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager,
            AethonDbContext db,
            MfaSetupRequest request,
            CancellationToken ct) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            var isValid = await userManager.VerifyTwoFactorTokenAsync(
                user, TokenOptions.DefaultAuthenticatorProvider, request.Code);

            if (!isValid)
                return Results.BadRequest(new { code = "mfa.invalid_code", message = "Invalid authenticator code. Please try again." });

            await userManager.SetTwoFactorEnabledAsync(user, true);

            // Clear the force-enable flag
            if (user.MustEnableMfa)
            {
                await db.Users.Where(u => u.Id == user.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.MustEnableMfa, false), ct);
            }

            return Results.Ok(new { message = "Two-factor authentication enabled successfully." });
        }).RequireAuthorization();

        // DELETE /auth/mfa — disables 2FA for the current user
        group.MapDelete("/mfa", async (
            HttpContext http,
            UserManager<ApplicationUser> userManager) =>
        {
            var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            await userManager.SetTwoFactorEnabledAsync(user, false);
            await userManager.ResetAuthenticatorKeyAsync(user);

            return Results.Ok(new { message = "Two-factor authentication disabled." });
        }).RequireAuthorization();

        // POST /auth/forgot-password — public, sends reset link via email
        group.MapPost("/forgot-password", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration config,
            ForgotPasswordRequest request) =>
        {
            // Always return Ok — never reveal whether the email exists
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Results.Ok(new { message = "If that email is registered, a reset link has been sent." });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var webBaseUrl = config["WebBaseUrl"] ?? "http://localhost:5200";
            var resetUrl = $"{webBaseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            var requiresMfa = user.TwoFactorEnabled;

            var htmlBody = $"""
                <p>Hi {System.Net.WebUtility.HtmlEncode(user.DisplayName)},</p>
                <p>We received a request to reset your Aethon password.</p>
                {(requiresMfa ? "<p><strong>Your account has two-factor authentication enabled.</strong> You will need your authenticator app to complete the reset.</p>" : "")}
                <p><a href="{resetUrl}" style="background:#111827;color:#fff;padding:10px 20px;border-radius:6px;text-decoration:none;display:inline-block;">Reset my password</a></p>
                <p>This link expires in 24 hours. If you did not request this, you can ignore this email.</p>
                """;

            var textBody = $"Reset your Aethon password:\n{resetUrl}\n\nThis link expires in 24 hours.";

            await emailService.SendAsync(new EmailMessage
            {
                ToEmail = user.Email!,
                ToName = user.DisplayName,
                Subject = "Reset your Aethon password",
                HtmlBody = htmlBody,
                TextBody = textBody
            });

            return Results.Ok(new { message = "If that email is registered, a reset link has been sent." });
        });

        // GET /auth/reset-password/check — returns whether account requires MFA during reset
        group.MapGet("/reset-password/check", async (
            UserManager<ApplicationUser> userManager,
            string email) =>
        {
            var user = await userManager.FindByEmailAsync(email);
            // Return same shape whether user exists or not — just vary requiresMfa
            return Results.Ok(new { requiresMfa = user?.TwoFactorEnabled == true });
        });

        // POST /auth/reset-password — completes password reset; requires TOTP if MFA is enabled
        group.MapPost("/reset-password", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            AethonDbContext db,
            ResetPasswordRequest request,
            CancellationToken ct) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Results.BadRequest(new { code = "auth.invalid_token", message = "Invalid or expired reset link." });

            // If MFA is enabled, validate the TOTP code before resetting
            if (user.TwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(request.TotpCode))
                    return Results.BadRequest(new { code = "auth.mfa_required", message = "A verification code from your authenticator app is required." });

                var isValidTotp = await userManager.VerifyTwoFactorTokenAsync(
                    user, TokenOptions.DefaultAuthenticatorProvider, request.TotpCode);

                if (!isValidTotp)
                    return Results.BadRequest(new { code = "auth.invalid_mfa_code", message = "Invalid authenticator code." });
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return Results.ValidationProblem(errors);
            }

            // Clear the force-change flag if set
            if (user.MustChangePassword)
                await db.Users.Where(u => u.Id == user.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.MustChangePassword, false), ct);

            // Send confirmation email
            await emailService.SendAsync(new EmailMessage
            {
                ToEmail = user.Email!,
                ToName = user.DisplayName,
                Subject = "Your Aethon password has been reset",
                HtmlBody = $"""
                    <p>Hi {System.Net.WebUtility.HtmlEncode(user.DisplayName)},</p>
                    <p>Your Aethon password was successfully reset.</p>
                    <p>If you did not make this change, please contact support immediately.</p>
                    """,
                TextBody = $"Hi {user.DisplayName},\n\nYour Aethon password was successfully reset.\n\nIf you did not make this change, please contact support immediately."
            });

            return Results.Ok(new { message = "Password reset successfully." });
        });
    }

    public sealed class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RegistrationType { get; set; } = string.Empty;
        public string? OrganisationName { get; set; }
    }

    public sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AppType { get; set; } = string.Empty;
        public bool IsSuperAdmin { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSupport { get; set; }
        public bool MustChangePassword { get; set; }
        public bool MustEnableMfa { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string? TwoFactorTicket { get; set; }
        public string? OrganisationId { get; set; }
        public string? OrganisationName { get; set; }
        public string? OrganisationType { get; set; }
        public string? CompanyRole { get; set; }
        public string? RecruiterRole { get; set; }
        public bool IsOrganisationOwner { get; set; }
    }

    public sealed class VerifyTwoFactorRequest
    {
        public string TwoFactorTicket { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public sealed class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public sealed class MfaSetupRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public sealed class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? TotpCode { get; set; }
    }
}
