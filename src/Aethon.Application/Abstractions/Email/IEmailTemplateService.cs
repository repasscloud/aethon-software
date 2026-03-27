namespace Aethon.Application.Abstractions.Email;

/// <summary>
/// Renders email templates by name. Looks up subject/html from DB first (SystemSettings),
/// falls back to built-in hardcoded defaults. Variable substitution uses {{VarName}} tokens.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Returns the resolved (subject, htmlBody) for the named template with variables substituted.
    /// </summary>
    Task<(string Subject, string HtmlBody)> RenderAsync(
        string templateName,
        Dictionary<string, string> vars,
        CancellationToken ct = default);
}
