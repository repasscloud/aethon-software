namespace Aethon.Api.Infrastructure.Email;

public sealed class EmailOptions
{
    public string MailerSendApiKey   { get; set; } = "";
    public string FromEmail          { get; set; } = "";
    public string FromName           { get; set; } = "Aethon";
    public string WebBaseUrl         { get; set; } = "http://localhost:5200";
    public string FromEmailSupport   { get; set; } = "";
}
