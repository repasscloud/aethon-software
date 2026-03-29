using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Aethon.Api.Common;

public static class ValidationHttpContextExtensions
{
    public static async Task<IResult?> ValidateAsync<T>(
        this HttpContext httpContext,
        T request,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var validator = httpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is null)
        {
            return null;
        }

        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken == default ? httpContext.RequestAborted : cancellationToken);

        if (validationResult.IsValid)
        {
            return null;
        }

        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.ErrorMessage).Distinct().ToArray());

        return Results.ValidationProblem(errors);
    }
}
