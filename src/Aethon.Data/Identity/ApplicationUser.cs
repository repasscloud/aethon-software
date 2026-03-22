using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Aethon.Data.Identity;

/// <summary>
/// Identity/account record for a system user.
/// 
/// This stores authentication, account state, high-level user classification,
/// and account-level verification flags.
/// 
/// Profile-specific and candidate-facing data should live in JobSeekerProfile.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Display name shown in the UI.
    /// This can be used across all account types.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the account is enabled and allowed to access the system.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// The high-level account type selected during onboarding.
    /// </summary>
    public UserAccountType UserType { get; set; }

    /// <summary>
    /// Indicates whether the user's identity has been verified.
    /// This can later be managed manually or via an external provider.
    /// </summary>
    public bool IsIdentityVerified { get; set; }

    /// <summary>
    /// When the user's identity was verified.
    /// </summary>
    public DateTime? IdentityVerifiedUtc { get; set; }

    /// <summary>
    /// Optional admin/internal notes for the identity verification result.
    /// </summary>
    public string? IdentityVerificationNotes { get; set; }

    /// <summary>
    /// Indicates whether the user's phone number has been verified.
    /// This is separate from Identity's built-in PhoneNumberConfirmed so the
    /// domain model stays explicit and easy to query/report on.
    /// </summary>
    public bool IsPhoneNumberVerified { get; set; }

    /// <summary>
    /// When the user's phone number was verified.
    /// </summary>
    public DateTime? PhoneNumberVerifiedUtc { get; set; }

    /// <summary>
    /// Forces the user to set a new password on next login.
    /// Set when a staff account is first created or when an admin resets the password.
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    /// Forces the user to enable TOTP-based MFA on next login.
    /// Only meaningful if TwoFactorEnabled is false.
    /// </summary>
    public bool MustEnableMfa { get; set; }
}