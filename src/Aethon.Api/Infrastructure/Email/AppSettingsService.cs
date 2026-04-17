using Aethon.Application.Abstractions.Email;
using Microsoft.Extensions.Options;

namespace Aethon.Api.Infrastructure.Email;

public sealed class AppSettingsService : IAppSettings
{
    private readonly EmailOptions _options;

    public AppSettingsService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public string WebBaseUrl => _options.WebBaseUrl;
}
