using Aethon.Api.Auth;
using Aethon.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace Aethon.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            RegisterRequest request) =>
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            var token = tokenService.GenerateToken(user);

            return Results.Ok(new AuthTokenResponse
            {
                Token = token
            });
        });

        group.MapPost("/login", async (
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            LoginRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid credentials." });
            }

            var valid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!valid)
            {
                return Results.BadRequest(new { code = "auth.invalid_credentials", message = "Invalid credentials." });
            }

            var token = tokenService.GenerateToken(user);

            return Results.Ok(new AuthTokenResponse
            {
                Token = token
            });
        });
    }

    public sealed class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class AuthTokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
