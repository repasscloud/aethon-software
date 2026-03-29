using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Logging;

namespace Aethon.Api.Infrastructure.Email;

public sealed class MailerSendEmailService : IEmailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmailOptionsResolver _resolver;
    private readonly ILogger<MailerSendEmailService> _logger;
    private readonly ISystemLogService _systemLog;

    public MailerSendEmailService(
        IHttpClientFactory httpClientFactory,
        EmailOptionsResolver resolver,
        ILogger<MailerSendEmailService> logger,
        ISystemLogService systemLog)
    {
        _httpClientFactory = httpClientFactory;
        _resolver = resolver;
        _logger = logger;
        _systemLog = systemLog;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var options = await _resolver.ResolveAsync(ct);

        if (string.IsNullOrWhiteSpace(options.MailerSendApiKey))
        {
            const string warnMsg = "Email not sent — MailerSendApiKey is not configured.";
            _logger.LogWarning("{Msg} To: {To}, Subject: {Subject}", warnMsg, message.ToEmail, message.Subject);
            await _systemLog.WarnAsync("Email", warnMsg,
                $"To: {message.ToEmail} | Subject: {message.Subject}", ct);
            return;
        }

        object? replyTo = message.ReplyToEmail is not null
            ? new { email = message.ReplyToEmail, name = message.ReplyToName }
            : null;

        var attachments = message.Attachments.Count > 0
            ? message.Attachments.Select(a => new
            {
                content = a.ContentBase64,
                filename = a.FileName,
                disposition = "attachment"
            }).ToArray()
            : null;

        var payload = new
        {
            from = new { email = options.FromEmail, name = options.FromName },
            to = new[] { new { email = message.ToEmail, name = message.ToName } },
            reply_to = replyTo,
            subject = message.Subject,
            text = message.TextBody,
            html = message.HtmlBody,
            attachments
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.MailerSendApiKey);

        try
        {
            using var response = await client.PostAsync("https://api.mailersend.com/v1/email", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("MailerSend rejected email. Status: {Status}, Body: {Body}",
                    response.StatusCode, body);
                await _systemLog.ErrorAsync("Email",
                    $"MailerSend rejected email to {message.ToEmail} (HTTP {(int)response.StatusCode})",
                    $"Subject: {message.Subject}\nStatus: {response.StatusCode}\nResponse: {body}",
                    ct: ct);
            }
            else
            {
                await _systemLog.InfoAsync("Email",
                    $"Email sent successfully to {message.ToEmail}",
                    $"Subject: {message.Subject} | Status: {(int)response.StatusCode}", ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", message.ToEmail);
            await _systemLog.ErrorAsync("Email",
                $"Exception sending email to {message.ToEmail}",
                $"Subject: {message.Subject}",
                exception: ex,
                ct: ct);
        }
    }
}
