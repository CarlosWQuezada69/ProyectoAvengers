using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Account;
using ProyectoAvengers.Shared.DTOs.Auth;

namespace ProyectoAvengers.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public AuthService(AppDbContext context, ITokenService tokenService, IEmailSender emailSender,
        IConfiguration configuration, IHostEnvironment environment)
    {
        _context = context;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<(LoginResponse? response, int? statusCode, string? title, string? detail)> LoginAsync(
        LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.DeletedAt == null, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            if (user != null)
            {
                user.RecordFailedLogin();
                await _context.SaveChangesAsync(ct);
            }
            return (null, 401, "Credenciales inválidas", "El correo o la contraseña no son correctos.");
        }

        if (user.LockedUntilUtc.HasValue && user.LockedUntilUtc > DateTime.UtcNow)
            return (null, 401, "Cuenta bloqueada", "Demasiados intentos fallidos. Intenta de nuevo en 15 minutos.");

        user.ResetFailedLogins();
        user.RecordLogin();

        var roles = await GetRolesAsync(user.Id, ct);
        var permissions = await GetPermissionsAsync(user.Id, ct);

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _context.RefreshTokens.Add(new RefreshToken(
            user.Id, _tokenService.GenerateRefreshToken(), DateTime.UtcNow.AddDays(7), ipAddress)).Entity;

        await _context.SaveChangesAsync(ct);

        return (new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = expiresIn,
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions
            }
        }, null, null, null);
    }

    public async Task<(RefreshTokenResponse? response, int? statusCode, string? title, string? detail)> RefreshTokenAsync(
        string refreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var storedToken = await _context.RefreshTokens
            .AsTracking()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

        if (storedToken == null)
            return (null, 401, "Token inválido", "El refresh token no es válido.");

        if (storedToken.RevokedAt != null)
        {
            var userTokens = await _context.RefreshTokens
                .AsTracking()
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var token in userTokens)
                token.Revoke();

            await _context.SaveChangesAsync(ct);

            return (null, 401, "Posible robo de token",
                "El refresh token ya fue usado. Todos los tokens fueron revocados por seguridad.");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return (null, 401, "Token expirado", "El refresh token ha expirado.");

        storedToken.Revoke();

        var user = storedToken.User;
        var roles = await GetRolesAsync(user.Id, ct);
        var permissions = await GetPermissionsAsync(user.Id, ct);

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _context.RefreshTokens.Add(new RefreshToken(
            user.Id, _tokenService.GenerateRefreshToken(), DateTime.UtcNow.AddDays(7), ipAddress)).Entity;

        await _context.SaveChangesAsync(ct);

        return (new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = expiresIn
        }, null, null, null);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var storedToken = await _context.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);

        if (storedToken != null)
        {
            storedToken.Revoke();
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<(bool success, string? message, string? resetUrl)> ForgotPasswordAsync(
        string email, string? frontendUrl, CancellationToken ct = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);

        if (user == null)
            return (true, null, null);

        var tokenBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        _context.PasswordResetTokens.Add(new PasswordResetToken(user.Id, token, DateTime.UtcNow.AddHours(1)));
        await _context.SaveChangesAsync(ct);

        var frontend = frontendUrl ?? _configuration["App:FrontendUrl"] ?? "http://localhost:4200";
        var resetUrl = $"{frontend}/auth/reset-password?token={token}";

        await _emailSender.SendAsync(user.Email, "Recuperación de contraseña",
            $"Para restablecer tu contraseña, abre este enlace: {resetUrl}\n\nEl enlace es válido por 1 hora.");

        return (true, null, _environment.IsDevelopment() ? resetUrl : null);
    }

    public async Task<(bool success, string? message, int? statusCode)> ResetPasswordAsync(
        string token, string newPassword, CancellationToken ct = default)
    {
        var resetToken = await _context.PasswordResetTokens
            .AsTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && t.UsedAt == null, ct);

        if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
            return (false, "El token de recuperación no es válido o ha expirado.", 400);

        resetToken.MarkAsUsed();
        resetToken.User.ChangePassword(BCrypt.Net.BCrypt.HashPassword(newPassword));

        await _context.SaveChangesAsync(ct);

        return (true, null, null);
    }

    public async Task<LoginResponse?> GetMeAsync(Guid? userId, CancellationToken ct = default)
    {
        if (userId == null) return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);

        if (user == null) return null;

        var roles = await GetRolesAsync(user.Id, ct);
        var permissions = await GetPermissionsAsync(user.Id, ct);

        return new LoginResponse
        {
            AccessToken = string.Empty,
            RefreshToken = string.Empty,
            ExpiresIn = 0,
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

    private Task<List<string>> GetRolesAsync(Guid userId, CancellationToken ct)
        => _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync(ct);

    private Task<List<string>> GetPermissionsAsync(Guid userId, CancellationToken ct)
        => _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(ct);
}
