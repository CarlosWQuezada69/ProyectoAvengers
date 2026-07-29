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
    private readonly SigningCredentials _signingCredentials;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenService(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET");

        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException(
                "JWT:Secret no está configurado. Define 'Jwt:Secret' en appsettings o la variable de entorno 'JWT_SECRET' (mínimo 32 caracteres).");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _issuer = jwtSettings["Issuer"] ?? "ProyectoAvengers";
        _audience = jwtSettings["Audience"] ?? "ProyectoAvengers";
        _expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]
            ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
            ?? "15");
    }

    public (string token, int expiresIn) GenerateAccessToken(User user, List<string> roles, List<string> permissions)
    {
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
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: _signingCredentials
        );

        return (_tokenHandler.WriteToken(token), _expiryMinutes * 60);
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
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingCredentials.Key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
