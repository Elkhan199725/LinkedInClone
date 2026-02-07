using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Security;

public sealed class JwtTokenGenerator : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(Guid userId, string email, string? userName, IEnumerable<string> roles)
    {
        var jwt = _config.GetSection("Jwt");

        var key = GetRequired(jwt, "Key", "Jwt:Key missing");
        if (key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

        var issuer = GetRequired(jwt, "Issuer", "Jwt:Issuer missing");
        var audience = GetRequired(jwt, "Audience", "Jwt:Audience missing");

        var expiresMinutes = TryGetInt(jwt["ExpiresMinutes"], fallback: 60);
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email ?? string.Empty),
            new(ClaimTypes.Name, userName ?? email ?? string.Empty)
        };

        foreach (var role in (roles ?? Array.Empty<string>()).Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Role, role));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Legacy method for backward compatibility
    public string Generate(AppUser user, IEnumerable<string> roles)
        => GenerateToken(user.Id, user.Email ?? string.Empty, user.UserName, roles);

    private static string GetRequired(IConfigurationSection section, string key, string errorMessage)
        => section[key] ?? throw new InvalidOperationException(errorMessage);

    private static int TryGetInt(string? value, int fallback)
        => int.TryParse(value, out var parsed) ? parsed : fallback;
}
