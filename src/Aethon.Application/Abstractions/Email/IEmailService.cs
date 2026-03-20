namespace Aethon.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public sealed class EmailMessage
{
    public string ToEmail { get; init; } = "";
    public string? ToName { get; init; }
    public string Subject { get; init; } = "";
    public string TextBody { get; init; } = "";
    public string HtmlBody { get; init; } = "";
}
