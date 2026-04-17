namespace Aethon.Api.Infrastructure.AtsMatch;

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://ollama:11434";
    public string Model { get; set; } = "mistral:7b";
    public int TimeoutSeconds { get; set; } = 120;
}
