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

    public string GenerateToken(ApplicationUser user)
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

        var token = new JwtSecurityToken(
            issuer: string.IsNullOrWhiteSpace(jwtIssuer) ? null : jwtIssuer,
            audience: string.IsNullOrWhiteSpace(jwtAudience) ? null : jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
