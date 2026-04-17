using System.Text.Json.Nodes;
using Aethon.Application.Abstractions.AtsMatch;
using Aethon.Shared.Enums;

namespace Aethon.Api.Infrastructure.AtsMatch;

/// <summary>
/// Parses the raw LLM JSON response into an AtsMatchResponse.
/// Shared by both ClaudeAtsMatchingService and OllamaAtsMatchingService.
/// </summary>
internal static class AtsMatchResponseParser
{
    internal static AtsMatchResponse Parse(string rawJson, string modelUsed, int? tokensUsed = null)
    {
        try
        {
            var text = rawJson.Trim();

            // Strip markdown code fences if present
            if (text.StartsWith("```"))
            {
                text = text[(text.IndexOf('\n') + 1)..];
                if (text.EndsWith("```")) text = text[..text.LastIndexOf("```")].TrimEnd();
            }

            var node = JsonNode.Parse(text.Trim());
            if (node is null)
                return Failure("Parsed JSON was null.", rawJson);

            var overall  = node["overall_score"]?.GetValue<int>() ?? 0;
            var dimNode  = node["dimension_scores"];
            var rec      = node["recommendation"]?.GetValue<string>() ?? "";
            var summary  = node["summary"]?.GetValue<string>();
            var confidence = TryFloat(node["confidence"]);

            var strengths = ParseStringArray(node["strengths"]);
            var gaps      = ParseStringArray(node["gaps"]);

            var recommendation = rec switch
            {
                "StrongMatch"  => AtsMatchRecommendation.StrongMatch,
                "GoodMatch"    => AtsMatchRecommendation.GoodMatch,
                "PartialMatch" => AtsMatchRecommendation.PartialMatch,
                "PoorMatch"    => AtsMatchRecommendation.PoorMatch,
                _              => AtsMatchRecommendation.NotSuitable
            };

            // Derive recommendation from score if the model didn't return a valid one
            if (rec is not ("StrongMatch" or "GoodMatch" or "PartialMatch" or "PoorMatch" or "NotSuitable"))
            {
                recommendation = overall switch
                {
                    >= 80 => AtsMatchRecommendation.StrongMatch,
                    >= 60 => AtsMatchRecommendation.GoodMatch,
                    >= 40 => AtsMatchRecommendation.PartialMatch,
                    >= 20 => AtsMatchRecommendation.PoorMatch,
                    _     => AtsMatchRecommendation.NotSuitable
                };
            }

            return new AtsMatchResponse
            {
                Success             = true,
                ModelUsed           = modelUsed,
                OverallScore        = Math.Clamp(overall, 0, 100),
                SkillsScore         = TryInt(dimNode?["skills"]),
                ExperienceScore     = TryInt(dimNode?["experience"]),
                LocationScore       = TryInt(dimNode?["location"]),
                SalaryScore         = TryInt(dimNode?["salary"]),
                QualificationsScore = TryInt(dimNode?["qualifications"]),
                WorkRightsScore     = TryInt(dimNode?["work_rights"]),
                Recommendation      = recommendation,
                Strengths           = strengths,
                Gaps                = gaps,
                Summary             = summary,
                Confidence          = confidence,
                RawResponseJson     = rawJson,
                TokensUsed          = tokensUsed
            };
        }
        catch (Exception ex)
        {
            return Failure($"Failed to parse LLM response: {ex.Message}", rawJson);
        }
    }

    private static AtsMatchResponse Failure(string error, string rawJson) => new()
    {
        Success         = false,
        Error           = error,
        RawResponseJson = rawJson
    };

    private static int? TryInt(JsonNode? node)
    {
        if (node is null) return null;
        try { return node.GetValue<int>(); } catch { return null; }
    }

    private static float? TryFloat(JsonNode? node)
    {
        if (node is null) return null;
        try { return node.GetValue<float>(); } catch { return null; }
    }

    private static List<string> ParseStringArray(JsonNode? node)
    {
        var result = new List<string>();
        if (node is not JsonArray arr) return result;
        foreach (var item in arr)
        {
            var s = item?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(s)) result.Add(s);
        }
        return result;
    }
}
