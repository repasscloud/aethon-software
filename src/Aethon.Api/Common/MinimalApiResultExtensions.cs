using Aethon.Application.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Aethon.Api.Common;

public static class MinimalApiResultExtensions
{
    public static IResult ToMinimalApiResult(this Result result)
    {
        if (result.Succeeded)
        {
            return Results.Ok();
        }

        return MapError(result.ErrorCode, result.ErrorMessage);
    }

    public static IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        if (result.Succeeded)
        {
            return Results.Ok(result.Value);
        }

        return MapError(result.ErrorCode, result.ErrorMessage);
    }

    private static IResult MapError(string? code, string? message)
    {
        var error = new ApiError
        {
            Code = code ?? "unknown",
            Message = message ?? "An error occurred."
        };

        if (code is null)
        {
            return Results.BadRequest(error);
        }

        if (code.StartsWith("auth.unauthenticated", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(error, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (code.EndsWith(".forbidden", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(error, statusCode: StatusCodes.Status403Forbidden);
        }

        if (code.EndsWith(".not_found", StringComparison.OrdinalIgnoreCase))
        {
            return Results.NotFound(error);
        }

        return Results.BadRequest(error);
    }
}
