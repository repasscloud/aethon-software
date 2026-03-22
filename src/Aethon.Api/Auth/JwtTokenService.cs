using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aethon.Data.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Aethon.Api.Auth;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user, IList<string>? roles = null)
    {
        var jwtKey = _configuration["Auth:JwtKey"] ?? "dev-secret-key-change-me-dev-secret-key-change-me";
        var jwtIssuer = _configuration["Auth:Issuer"];
        var jwtAudience = _configuration["Auth:Audience"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("name", user.DisplayName ?? string.Empty)
        };

        if (roles is not null)
        {
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: string.IsNullOrWhiteSpace(jwtIssuer) ? null : jwtIssuer,
            audience: string.IsNullOrWhiteSpace(jwtAudience) ? null : jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a short-lived (2 minute) ticket used to complete a 2FA login.
    /// Contains only the user ID and a purpose claim — no access privileges.
    /// </summary>
    public string GenerateTwoFactorTicket(Guid userId)
    {
        var jwtKey = _configuration["Auth:JwtKey"] ?? "dev-secret-key-change-me-dev-secret-key-change-me";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("purpose", "two_factor")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a two-factor ticket and returns the userId if valid, or null if invalid/expired.
    /// </summary>
    public Guid? ValidateTwoFactorTicket(string ticket)
    {
        var jwtKey = _configuration["Auth:JwtKey"] ?? "dev-secret-key-change-me-dev-secret-key-change-me";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(ticket, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out _);

            if (principal.FindFirstValue("purpose") != "two_factor")
                return null;

            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return sub != null && Guid.TryParse(sub, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
