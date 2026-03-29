using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Settings;

namespace Aethon.Api.Infrastructure.Email;

public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly ISystemSettingsService _settings;

    public EmailTemplateService(ISystemSettingsService settings)
    {
        _settings = settings;
    }

    public async Task<(string Subject, string HtmlBody)> RenderAsync(
        string templateName,
        Dictionary<string, string> vars,
        CancellationToken ct = default)
    {
        var subjectKey = $"EmailTemplate__{templateName}__Subject";
        var htmlKey    = $"EmailTemplate__{templateName}__Html";

        var dbSubject = await _settings.GetStringAsync(subjectKey, ct);
        var dbHtml    = await _settings.GetStringAsync(htmlKey, ct);

        var subject = !string.IsNullOrWhiteSpace(dbSubject) ? dbSubject : GetDefaultSubject(templateName);
        var html    = !string.IsNullOrWhiteSpace(dbHtml)    ? dbHtml    : GetDefaultHtml(templateName);

        subject = Substitute(subject, vars);
        html    = Substitute(html, vars);

        return (subject, html);
    }

    // ── Variable substitution ─────────────────────────────────────────────────

    private static string Substitute(string template, Dictionary<string, string> vars)
    {
        foreach (var (key, value) in vars)
            template = template.Replace("{{" + key + "}}", value, StringComparison.Ordinal);
        return template;
    }

    // ── Default subjects ──────────────────────────────────────────────────────

    private static string GetDefaultSubject(string templateName) => templateName switch
    {
        "Verification"         => "Verify your Aethon account",
        "PasswordReset"        => "Reset your Aethon password",
        "PasswordResetConfirm" => "Your Aethon password has been reset",
        "StaffWelcome"         => "Welcome to Aethon — your account is ready",
        "IdentityRejection"    => "Your Aethon identity verification was unsuccessful",
        _                      => "Aethon notification"
    };

    // ── Default HTML templates ────────────────────────────────────────────────

    private static string GetDefaultHtml(string templateName) => templateName switch
    {
        "Verification"         => VerificationHtml,
        "PasswordReset"        => PasswordResetHtml,
        "PasswordResetConfirm" => PasswordResetConfirmHtml,
        "StaffWelcome"         => StaffWelcomeHtml,
        "IdentityRejection"    => IdentityRejectionHtml,
        _                      => GenericHtml
    };

    // ── Layout helpers ────────────────────────────────────────────────────────

    public string WrapHtml(string body) => Wrap(body);

    private static string Wrap(string body) =>
        "<!DOCTYPE html>" +
        "<html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"></head>" +
        "<body style=\"margin:0;padding:0;background:#f4f4f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;\">" +
        "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f4f4f5;padding:40px 0;\">" +
        "<tr><td align=\"center\">" +
        "<table width=\"600\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 1px 4px rgba(0,0,0,.08);\">" +
        "<tr><td style=\"background:#111827;padding:24px 40px;\">" +
        "<table cellpadding=\"0\" cellspacing=\"0\"><tr>" +
        "<td style=\"vertical-align:middle;padding-right:12px;\">" +
        "<div style=\"width:44px;height:44px;border-radius:999px;background:#ffffff;color:#111827;font-size:1.1rem;font-weight:700;text-align:center;line-height:44px;\">A</div>" +
        "</td>" +
        "<td style=\"vertical-align:middle;\">" +
        "<div style=\"color:#ffffff;font-size:1rem;font-weight:700;line-height:1.1;\">Aethon</div>" +
        "<div style=\"color:#9ca3af;font-size:0.8rem;line-height:1.1;\">Hiring intelligence platform</div>" +
        "</td>" +
        "</tr></table>" +
        "</td></tr>" +
        "<tr><td style=\"padding:36px 40px;color:#111827;font-size:15px;line-height:1.6;\">" +
        body +
        "</td></tr>" +
        "<tr><td style=\"background:#f9fafb;padding:20px 40px;border-top:1px solid #e5e7eb;\">" +
        "<p style=\"margin:0;font-size:12px;color:#6b7280;line-height:1.5;\">You received this email because you have an Aethon account.<br>If you did not request this email, you can safely ignore it.</p>" +
        "</td></tr>" +
        "</table></td></tr></table>" +
        "</body></html>";

    private static string Btn(string url, string label) =>
        "<a href=\"" + url + "\" style=\"display:inline-block;background:#111827;color:#ffffff;text-decoration:none;padding:12px 24px;border-radius:8px;font-weight:600;font-size:14px;margin:16px 0;\">" + label + "</a>";

    // ── Templates ─────────────────────────────────────────────────────────────

    private static readonly string VerificationHtml = Wrap(
        "<p style=\"margin:0 0 16px;\">Hi <strong>{{DisplayName}}</strong>,</p>" +
        "<p style=\"margin:0 0 16px;\">Thanks for creating an Aethon account. Please verify your email address by clicking the button below.</p>" +
        Btn("{{VerificationUrl}}", "Verify my email address") +
        "<p style=\"margin:16px 0 0;font-size:13px;color:#6b7280;\">Button not working? Copy and paste this link into your browser:<br><span style=\"word-break:break-all;\">{{VerificationUrl}}</span></p>" +
        "<p style=\"margin:16px 0 0;font-size:13px;color:#6b7280;\">This link expires in 24 hours.</p>"
    );

    private static readonly string PasswordResetHtml = Wrap(
        "<p style=\"margin:0 0 16px;\">Hi <strong>{{DisplayName}}</strong>,</p>" +
        "<p style=\"margin:0 0 16px;\">We received a request to reset your Aethon password.</p>" +
        "{{MfaWarning}}" +
        Btn("{{ResetUrl}}", "Reset my password") +
        "<p style=\"margin:16px 0 0;font-size:13px;color:#6b7280;\">Button not working? Copy and paste this link into your browser:<br><span style=\"word-break:break-all;\">{{ResetUrl}}</span></p>" +
        "<p style=\"margin:16px 0 0;font-size:13px;color:#6b7280;\">This link expires in 24 hours. If you did not request a password reset, you can safely ignore this email.</p>"
    );

    private static readonly string PasswordResetConfirmHtml = Wrap(
        "<p style=\"margin:0 0 16px;\">Hi <strong>{{DisplayName}}</strong>,</p>" +
        "<p style=\"margin:0 0 16px;\">Your Aethon password has been successfully reset.</p>" +
        "<p style=\"margin:0;font-size:13px;color:#6b7280;\">If you did not make this change, please contact support immediately.</p>"
    );

    private static readonly string StaffWelcomeHtml = Wrap(
        "<p style=\"margin:0 0 16px;\">Hi <strong>{{DisplayName}}</strong>,</p>" +
        "<p style=\"margin:0 0 16px;\">Your Aethon staff account has been created.</p>" +
        "<table cellpadding=\"0\" cellspacing=\"0\" style=\"margin:0 0 20px;font-size:14px;\">" +
        "<tr><td style=\"padding:4px 12px 4px 0;color:#6b7280;\">Email</td><td><strong>{{Email}}</strong></td></tr>" +
        "<tr><td style=\"padding:4px 12px 4px 0;color:#6b7280;\">Temporary password</td><td><code style=\"background:#f3f4f6;padding:2px 6px;border-radius:4px;\">{{TempPassword}}</code></td></tr>" +
        "</table>" +
        Btn("{{LoginUrl}}", "Sign in to Aethon") +
        "<p style=\"margin:16px 0 0;font-size:13px;color:#6b7280;\">You will be prompted to change your password on first sign-in.</p>"
    );

    private static readonly string IdentityRejectionHtml = Wrap(
        "<p style=\"margin:0 0 16px;\">Hi <strong>{{DisplayName}}</strong>,</p>" +
        "<p style=\"margin:0 0 16px;\">Unfortunately we were unable to verify your identity on Aethon.</p>" +
        "<p style=\"margin:0 0 16px;\"><strong>Reason:</strong> {{RejectionReason}}</p>" +
        "<p style=\"margin:0;font-size:13px;color:#6b7280;\">If you believe this is an error, please contact support.</p>"
    );

    private static readonly string GenericHtml = Wrap(
        "<p style=\"margin:0;\">You have a new notification from Aethon.</p>"
    );
}
