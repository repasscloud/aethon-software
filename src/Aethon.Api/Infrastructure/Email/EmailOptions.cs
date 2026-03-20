namespace Aethon.Api.Infrastructure.Email;

public sealed class EmailOptions
{
    public string MailerSendApiKey { get; set; } = "";
    public string FromEmail { get; set; } = "do-not-reply@repasscloud.com";
    public string FromName { get; set; } = "Aethon";
    public string WebBaseUrl { get; set; } = "http://localhost:5200";
}
