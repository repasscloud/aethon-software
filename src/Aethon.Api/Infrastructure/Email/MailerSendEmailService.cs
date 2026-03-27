using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Aethon.Application.Abstractions.Email;

namespace Aethon.Api.Infrastructure.Email;

public sealed class MailerSendEmailService : IEmailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmailOptionsResolver _resolver;
    private readonly ILogger<MailerSendEmailService> _logger;

    public MailerSendEmailService(
        IHttpClientFactory httpClientFactory,
        EmailOptionsResolver resolver,
        ILogger<MailerSendEmailService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var options = await _resolver.ResolveAsync(ct);

        if (string.IsNullOrWhiteSpace(options.MailerSendApiKey))
        {
            _logger.LogWarning("Email not sent — MailerSendApiKey is not configured. To: {To}, Subject: {Subject}",
                message.ToEmail, message.Subject);
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", message.ToEmail);
        }
    }
}
