using Aethon.Application.Abstractions.Settings;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Infrastructure.Email;

/// <summary>
/// Resolves email configuration with DB-first priority.
/// Resolution order: DB (SystemSettings) → IConfiguration (ENV VAR / appsettings) → null/empty (misconfigured).
/// </summary>
public sealed class EmailOptionsResolver
{
    private readonly ISystemSettingsService _settings;
    private readonly EmailOptions _configOptions;

    public EmailOptionsResolver(ISystemSettingsService settings, IOptions<EmailOptions> configOptions)
    {
        _settings = settings;
        _configOptions = configOptions.Value;
    }

    public async Task<EmailOptions> ResolveAsync(CancellationToken ct = default)
    {
        var apiKey          = await _settings.GetStringAsync(SystemSettingKeys.EmailMailerSendApiKey, ct);
        var fromEmail       = await _settings.GetStringAsync(SystemSettingKeys.EmailFromEmail, ct);
        var fromName        = await _settings.GetStringAsync(SystemSettingKeys.EmailFromName, ct);
        var webBaseUrl      = await _settings.GetStringAsync(SystemSettingKeys.EmailWebBaseUrl, ct);
        var fromEmailSupport = await _settings.GetStringAsync(SystemSettingKeys.EmailFromEmailSupport, ct);

        return new EmailOptions
        {
            MailerSendApiKey  = apiKey           ?? _configOptions.MailerSendApiKey,
            FromEmail         = fromEmail        ?? _configOptions.FromEmail,
            FromName          = fromName         ?? _configOptions.FromName,
            WebBaseUrl        = webBaseUrl       ?? _configOptions.WebBaseUrl,
            FromEmailSupport  = fromEmailSupport ?? _configOptions.FromEmailSupport,
        };
    }

    /// <summary>
    /// Returns true when both MailerSendApiKey and FromEmail are resolvable.
    /// Used by the admin dashboard health check.
    /// </summary>
    public async Task<bool> IsConfiguredAsync(CancellationToken ct = default)
    {
        var opts = await ResolveAsync(ct);
        return !string.IsNullOrWhiteSpace(opts.MailerSendApiKey)
            && !string.IsNullOrWhiteSpace(opts.FromEmail);
    }
}
