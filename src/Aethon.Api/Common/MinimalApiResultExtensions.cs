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

        return Results.BadRequest(new
        {
            error = result.ErrorCode,
            message = result.ErrorMessage
        });
    }

    public static IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        if (result.Succeeded)
        {
            return Results.Ok(result.Value);
        }

        return Results.BadRequest(new
        {
            error = result.ErrorCode,
            message = result.ErrorMessage
        });
    }
}
