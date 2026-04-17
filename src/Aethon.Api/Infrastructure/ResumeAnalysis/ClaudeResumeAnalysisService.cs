using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aethon.Application.Abstractions.ResumeAnalysis;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Infrastructure.ResumeAnalysis;

public sealed class ClaudeResumeAnalysisService : IResumeAnalysisService
{
    private const string AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
    private const string ModelId = "claude-sonnet-4-6";
    private const string AnthropicVersion = "2023-06-01";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly string ExtractionPrompt = """
        Analyse this resume/CV and respond with ONLY a valid JSON object (no markdown, no explanation) in this exact format:
        {
          "headline": "<short professional headline, max 120 chars, or null>",
          "summary": "<2-3 sentence summary of the candidate's background, or null>",
          "skills": ["skill1", "skill2"],
          "experienceLevel": "<one of: Junior, Mid, Senior, Lead, Executive, or null>",
          "yearsExperience": <integer estimate of total years of professional experience, or null>
        }
        """;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeResumeAnalysisService> _logger;

    public ClaudeResumeAnalysisService(
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeOptions> options,
        ILogger<ClaudeResumeAnalysisService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ResumeAnalysisResult> AnalyseAsync(
        string fileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new ResumeAnalysisResult
            {
                Success = false,
                Error = "Claude API key is not configured."
            };
        }

        try
        {
            object contentBlock;

            if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
            {
                contentBlock = new
                {
                    type = "document",
                    source = new
                    {
                        type = "base64",
                        media_type = "application/pdf",
                        data = Convert.ToBase64String(fileBytes)
                    }
                };
            }
            else
            {
                // Treat as plain text (handles .txt, .doc text extraction fallback)
                var text = Encoding.UTF8.GetString(fileBytes);
                contentBlock = new { type = "text", text };
            }

            var requestBody = new
            {
                model = ModelId,
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            contentBlock,
                            new { type = "text", text = ExtractionPrompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);

            using var response = await client.PostAsync(AnthropicApiUrl, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude API error. Status: {Status}, Body: {Body}",
                    response.StatusCode, responseBody);
                return new ResumeAnalysisResult { Success = false, Error = $"Claude API error: {response.StatusCode}" };
            }

            return ParseResponse(responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyse resume: {FileName}", fileName);
            return new ResumeAnalysisResult { Success = false, Error = ex.Message };
        }
    }

    private static ResumeAnalysisResult ParseResponse(string responseBody)
    {
        try
        {
            var root = JsonNode.Parse(responseBody);
            var text = root?["content"]?[0]?["text"]?.GetValue<string>() ?? "";

            // Strip markdown code fences if present
            text = text.Trim();
            if (text.StartsWith("```")) text = text[(text.IndexOf('\n') + 1)..];
            if (text.EndsWith("```")) text = text[..text.LastIndexOf("```")].TrimEnd();

            var parsed = JsonNode.Parse(text.Trim());

            var skills = new List<string>();
            var skillsNode = parsed?["skills"]?.AsArray();
            if (skillsNode is not null)
            {
                foreach (var skill in skillsNode)
                {
                    var s = skill?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(s)) skills.Add(s);
                }
            }

            return new ResumeAnalysisResult
            {
                Success = true,
                HeadlineSuggestion = parsed?["headline"]?.GetValue<string>(),
                SummaryExtract = parsed?["summary"]?.GetValue<string>(),
                Skills = skills,
                ExperienceLevel = parsed?["experienceLevel"]?.GetValue<string>(),
                YearsExperience = parsed?["yearsExperience"]?.AsValue().TryGetValue<int>(out var yrs) == true ? yrs : null
            };
        }
        catch (Exception)
        {
            return new ResumeAnalysisResult { Success = false, Error = "Failed to parse Claude response." };
        }
    }
}
