using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aethon.Application.Abstractions.AtsMatch;
using Aethon.Shared.Enums;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Infrastructure.AtsMatch;

/// <summary>
/// ATS matching service backed by a locally hosted Ollama instance.
/// Used for jobs where HasAiCandidateMatching = false (free / offline tier).
/// Calls the Ollama OpenAI-compatible /api/chat endpoint with format=json to enforce structured output.
/// </summary>
public sealed class OllamaAtsMatchingService : IAtsMatchingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaAtsMatchingService> _logger;

    public AtsMatchProvider Provider => AtsMatchProvider.Ollama;

    public OllamaAtsMatchingService(
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> options,
        ILogger<OllamaAtsMatchingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AtsMatchResponse> MatchAsync(string payloadJson, CancellationToken ct = default)
    {
        try
        {
            var url = $"{_options.BaseUrl.TrimEnd('/')}/api/chat";

            var requestBody = new
            {
                model    = _options.Model,
                format   = "json",   // Forces Ollama to return valid JSON
                stream   = false,
                messages = new[]
                {
                    new { role = "system", content = AtsMatchPrompt.SystemPrompt },
                    new { role = "user",   content = payloadJson }
                }
            };

            var json     = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            using var response = await client.PostAsync(url, content, ct);
            var responseBody   = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Ollama ATS match error. Status: {Status}, Body: {Body}",
                    response.StatusCode, responseBody);
                return new AtsMatchResponse
                {
                    Success = false,
                    Error   = $"Ollama error: {response.StatusCode}"
                };
            }

            // Ollama /api/chat response: { "message": { "content": "..." }, "done": true }
            var root = JsonNode.Parse(responseBody);
            var text = root?["message"]?["content"]?.GetValue<string>() ?? "";

            return AtsMatchResponseParser.Parse(text, _options.Model);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Ollama ATS match timed out after {Seconds}s.", _options.TimeoutSeconds);
            return new AtsMatchResponse { Success = false, Error = $"Ollama timed out after {_options.TimeoutSeconds}s." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Ollama ATS match.");
            return new AtsMatchResponse { Success = false, Error = ex.Message };
        }
    }
}
