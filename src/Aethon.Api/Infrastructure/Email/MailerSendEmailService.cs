using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Aethon.Application.Abstractions.Email;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Infrastructure.Email;

public sealed class MailerSendEmailService : IEmailService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmailOptions _options;
    private readonly ILogger<MailerSendEmailService> _logger;

    public MailerSendEmailService(
        IHttpClientFactory httpClientFactory,
        IOptions<EmailOptions> options,
        ILogger<MailerSendEmailService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.MailerSendApiKey))
        {
            _logger.LogWarning("Email not sent — MailerSendApiKey is not configured. To: {To}, Subject: {Subject}",
                message.ToEmail, message.Subject);
            return;
        }

        var payload = new
        {
            from = new { email = _options.FromEmail, name = _options.FromName },
            to = new[] { new { email = message.ToEmail, name = message.ToName } },
            subject = message.Subject,
            text = message.TextBody,
            html = message.HtmlBody
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.MailerSendApiKey);

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
