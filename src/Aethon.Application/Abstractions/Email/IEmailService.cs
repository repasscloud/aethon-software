namespace Aethon.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public sealed class EmailMessage
{
    public string ToEmail { get; init; } = "";
    public string? ToName { get; init; }
    public string? ReplyToEmail { get; init; }
    public string? ReplyToName { get; init; }
    public string Subject { get; init; } = "";
    public string TextBody { get; init; } = "";
    public string HtmlBody { get; init; } = "";
    public List<EmailAttachment> Attachments { get; init; } = [];
}

public sealed class EmailAttachment
{
    public string FileName { get; init; } = "";
    public string ContentBase64 { get; init; } = "";
    public string ContentType { get; init; } = "application/octet-stream";
}
