using Aethon.Application.Abstractions.Time;

namespace Aethon.Api.Infrastructure;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}