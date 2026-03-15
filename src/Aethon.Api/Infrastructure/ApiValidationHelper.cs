using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Aethon.Api.Infrastructure;

public static class ApiValidationHelper
{
    public static Dictionary<string, string[]> Validate<T>(T model)
    {
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(model!, context, results, validateAllProperties: true);

        return results
            .SelectMany(x =>
            {
                var memberNames = x.MemberNames.Any() ? x.MemberNames : [string.Empty];
                return memberNames.Select(memberName => new
                {
                    Key = memberName,
                    Message = x.ErrorMessage ?? "Validation error."
                });
            })
            .GroupBy(x => x.Key)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.Message).Distinct().ToArray());
    }

    public static Dictionary<string, string[]> FromIdentityErrors(IEnumerable<IdentityError> errors)
    {
        return errors
            .GroupBy(x => x.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                ? "Password"
                : "Email")
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.Description).Distinct().ToArray());
    }

    public static bool IsApiRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/auth")
               || request.Path.StartsWithSegments("/login")
               || request.Path.StartsWithSegments("/register")
               || request.Path.StartsWithSegments("/manage")
               || request.Path.StartsWithSegments("/health")
               || request.Path.StartsWithSegments("/org");
    }
}