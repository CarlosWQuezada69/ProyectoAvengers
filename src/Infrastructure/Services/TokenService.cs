using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, int expiresIn) GenerateAccessToken(User user, List<string> roles, List<string> permissions)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? "SuperSecretKey_Dev_ChangeInProduction_MinLength32Chars!";
        var issuer = jwtSettings["Issuer"] ?? "ProyectoAvengers";
        var audience = jwtSettings["Audience"] ?? "ProyectoAvengers";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]
            ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
            ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiryMinutes * 60);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Secret"]
                ?? Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? "SuperSecretKey_Dev_ChangeInProduction_MinLength32Chars!";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "ProyectoAvengers",
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"] ?? "ProyectoAvengers",
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
