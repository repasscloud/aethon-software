namespace Aethon.Api.Common;

public static class HttpContextExtensions
{
    public static string? GetCorrelationId(this HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationConstants.ItemKey, out var value))
        {
            return value?.ToString();
        }

        return null;
    }
}
