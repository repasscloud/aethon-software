using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aethon.Application.Abstractions.AtsMatch;
using Aethon.Api.Infrastructure.ResumeAnalysis;
using Aethon.Shared.Enums;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Infrastructure.AtsMatch;

/// <summary>
/// ATS matching service backed by the Anthropic Claude API.
/// Used for jobs where HasAiCandidateMatching = true (paid tier).
/// </summary>
public sealed class ClaudeAtsMatchingService : IAtsMatchingService
{
    private const string AnthropicApiUrl  = "https://api.anthropic.com/v1/messages";
    private const string ModelId          = "claude-sonnet-4-6";
    private const string AnthropicVersion = "2023-06-01";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeAtsMatchingService> _logger;

    public AtsMatchProvider Provider => AtsMatchProvider.Claude;

    public ClaudeAtsMatchingService(
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeOptions> options,
        ILogger<ClaudeAtsMatchingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AtsMatchResponse> MatchAsync(string payloadJson, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AtsMatchResponse { Success = false, Error = "Claude API key is not configured." };
        }

        try
        {
            var requestBody = new
            {
                model      = ModelId,
                max_tokens = 2048,
                system     = AtsMatchPrompt.SystemPrompt,
                messages   = new[]
                {
                    new { role = "user", content = payloadJson }
                }
            };

            var json    = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);

            using var response = await client.PostAsync(AnthropicApiUrl, content, ct);
            var responseBody   = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude ATS match API error. Status: {Status}, Body: {Body}",
                    response.StatusCode, responseBody);
                return new AtsMatchResponse
                {
                    Success = false,
                    Error   = $"Claude API error: {response.StatusCode}"
                };
            }

            // Extract text content and token usage from the Claude response envelope
            var root       = JsonNode.Parse(responseBody);
            var text       = root?["content"]?[0]?["text"]?.GetValue<string>() ?? "";
            var inputTokens  = root?["usage"]?["input_tokens"]?.GetValue<int>() ?? 0;
            var outputTokens = root?["usage"]?["output_tokens"]?.GetValue<int>() ?? 0;
            var totalTokens  = inputTokens + outputTokens;

            return AtsMatchResponseParser.Parse(text, ModelId, totalTokens > 0 ? totalTokens : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Claude ATS match.");
            return new AtsMatchResponse { Success = false, Error = ex.Message };
        }
    }
}
