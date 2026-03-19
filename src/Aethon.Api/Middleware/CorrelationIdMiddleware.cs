using Aethon.Api.Common;

namespace Aethon.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[CorrelationConstants.ItemKey] = correlationId;
        context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;

        await _next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationConstants.HeaderName, out var values))
        {
            var value = values.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}

