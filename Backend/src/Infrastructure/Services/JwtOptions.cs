using Microsoft.Extensions.Configuration;

namespace ProyectoAvengers.Infrastructure.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinSecretLength = 32;

    public string Secret { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int ExpiryMinutes { get; }

    public JwtOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        Secret = section["Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException(
                "JWT:Secret no está configurado. Define 'Jwt:Secret' en User Secrets o la variable de entorno 'JWT_SECRET' (mínimo 32 caracteres).");

        if (Secret.Length < MinSecretLength)
            throw new InvalidOperationException(
                $"JWT:Secret debe tener al menos {MinSecretLength} caracteres.");

        Issuer = section["Issuer"] ?? "ProyectoAvengers";
        Audience = section["Audience"] ?? "ProyectoAvengers";
        ExpiryMinutes = int.TryParse(
            section["ExpiryMinutes"] ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES"),
            out var minutes) && minutes > 0
            ? minutes
            : 15;
    }
}