using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Application.Abstractions.Settings;
using Aethon.Application.Abstractions.Syndication;
using Aethon.Data;
using Aethon.Data.Entities;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace Aethon.Api.Infrastructure.Syndication;

public sealed class GoogleIndexingService : IGoogleIndexingService
{
    private const string IndexingScope = "https://www.googleapis.com/auth/indexing";
    private const string IndexingEndpoint = "https://indexing.googleapis.com/v3/urlNotifications:publish";

    private readonly ISystemSettingsService _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AethonDbContext _db;
    private readonly ILogger<GoogleIndexingService> _logger;

    public GoogleIndexingService(
        ISystemSettingsService settings,
        IHttpClientFactory httpClientFactory,
        AethonDbContext db,
        ILogger<GoogleIndexingService> logger)
    {
        _settings = settings;
        _httpClientFactory = httpClientFactory;
        _db = db;
        _logger = logger;
    }

    public Task NotifyPublishedAsync(Guid jobId, string jobUrl, CancellationToken ct = default)
        => SendNotificationAsync(jobId, jobUrl, "URL_UPDATED", ct);

    public Task NotifyUpdatedAsync(Guid jobId, string jobUrl, CancellationToken ct = default)
        => SendNotificationAsync(jobId, jobUrl, "URL_UPDATED", ct);

    public Task NotifyRemovedAsync(Guid jobId, string jobUrl, CancellationToken ct = default)
        => SendNotificationAsync(jobId, jobUrl, "URL_DELETED", ct);

    private async Task SendNotificationAsync(Guid jobId, string jobUrl, string type, CancellationToken ct)
    {
        var enabled = await _settings.GetBoolAsync(SystemSettingKeys.GoogleIndexingEnabled, false, ct);
        if (!enabled)
            return;

        var saJson = await _settings.GetStringAsync(SystemSettingKeys.GoogleIndexingServiceAccount, ct);
        if (string.IsNullOrWhiteSpace(saJson))
        {
            _logger.LogWarning("Google Indexing is enabled but no Service Account JSON is configured.");
            return;
        }

        string? externalRef = null;
        string status;
        string? errorMessage = null;

        try
        {
            // Parse the SA JSON to extract client_email and private_key
            var keyData = JsonSerializer.Deserialize<ServiceAccountKeyData>(saJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (keyData is null || string.IsNullOrWhiteSpace(keyData.ClientEmail) || string.IsNullOrWhiteSpace(keyData.PrivateKey))
                throw new InvalidOperationException("Service Account JSON is missing client_email or private_key.");

            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(keyData.ClientEmail)
                {
                    Scopes = [IndexingScope]
                }.FromPrivateKey(keyData.PrivateKey));

            var token = await credential.GetAccessTokenForRequestAsync(
                authUri: IndexingEndpoint, cancellationToken: ct);

            var payload = JsonSerializer.Serialize(new { url = jobUrl, type });

            var client = _httpClientFactory.CreateClient("GoogleIndexing");
            using var request = new HttpRequestMessage(HttpMethod.Post, IndexingEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                status = "Success";
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("urlNotificationMetadata", out var meta) &&
                        meta.TryGetProperty("url", out var urlProp))
                        externalRef = urlProp.GetString();
                }
                catch { /* externalRef stays null */ }

                _logger.LogInformation("Google Indexing notified ({Type}) for job {JobId}", type, jobId);
            }
            else
            {
                status = "Failed";
                errorMessage = $"HTTP {(int)response.StatusCode}: {body}";
                _logger.LogWarning("Google Indexing notification failed for job {JobId}: {Error}", jobId, errorMessage);
            }
        }
        catch (Exception ex)
        {
            status = "Failed";
            errorMessage = ex.Message;
            _logger.LogError(ex, "Google Indexing exception for job {JobId}", jobId);
        }

        var now = DateTime.UtcNow;
        var record = new JobSyndicationRecord
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            Provider = "GoogleIndexing",
            Status = status,
            ExternalRef = externalRef,
            SubmittedUtc = now,
            LastAttemptUtc = now,
            LastErrorMessage = errorMessage,
            CreatedUtc = now
        };

        _db.JobSyndicationRecords.Add(record);
        await _db.SaveChangesAsync(ct);
    }

    private sealed class ServiceAccountKeyData
    {
        [JsonPropertyName("client_email")]
        public string? ClientEmail { get; set; }

        [JsonPropertyName("private_key")]
        public string? PrivateKey { get; set; }
    }
}
