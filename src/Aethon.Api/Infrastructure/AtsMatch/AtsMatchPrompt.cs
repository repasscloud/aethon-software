namespace Aethon.Api.Infrastructure.AtsMatch;

/// <summary>
/// Shared system prompt and response-parsing helpers used by both ATS matching service implementations.
/// </summary>
internal static class AtsMatchPrompt
{
    internal const string SystemPrompt = """
        You are an expert ATS (Applicant Tracking System) evaluator. You will be given structured JSON data
        about a job posting and a candidate. Your task is to evaluate how well the candidate matches the job.

        Respond with ONLY a valid JSON object — no markdown, no explanation, no preamble. Use this exact schema:
        {
          "overall_score": <integer 0-100>,
          "dimension_scores": {
            "skills": <integer 0-100 or null>,
            "experience": <integer 0-100 or null>,
            "location": <integer 0-100 or null>,
            "salary": <integer 0-100 or null>,
            "qualifications": <integer 0-100 or null>,
            "work_rights": <integer 0-100 or null>
          },
          "recommendation": <"StrongMatch" | "GoodMatch" | "PartialMatch" | "PoorMatch" | "NotSuitable">,
          "strengths": ["<strength 1>", "<strength 2>"],
          "gaps": ["<gap 1>", "<gap 2>"],
          "summary": "<2-3 sentence plain English summary of this candidate's suitability for the role>",
          "confidence": <float 0.0-1.0>
        }

        Scoring guide:
        - overall_score 80-100 → recommendation must be "StrongMatch"
        - overall_score 60-79  → recommendation must be "GoodMatch"
        - overall_score 40-59  → recommendation must be "PartialMatch"
        - overall_score 20-39  → recommendation must be "PoorMatch"
        - overall_score 0-19   → recommendation must be "NotSuitable"

        If data is missing for a dimension, set that dimension score to null and exclude it from the
        overall score calculation. Do not penalise the candidate for missing data.
        """;
}
