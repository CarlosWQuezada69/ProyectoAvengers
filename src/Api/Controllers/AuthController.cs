using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Account;
using ProyectoAvengers.Shared.DTOs.Auth;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, FailedLoginInfo> _failedLogins = new();

    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailSender;

    public AuthController(
        AppDbContext context,
        ITokenService tokenService,
        ICurrentUserService currentUser,
        IEmailSender emailSender)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUser = currentUser;
        _emailSender = emailSender;
    }

    [HttpPost("login")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        if (IsAccountLocked(normalizedEmail))
            return Unauthorized(new ProblemDetails
            {
                Title = "Cuenta bloqueada",
                Status = 401,
                Detail = "Demasiados intentos fallidos. Intenta de nuevo en 15 minutos."
            });

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.DeletedAt == null);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            RecordFailedAttempt(normalizedEmail);
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales inválidas",
                Status = 401,
                Detail = "El correo o la contraseña no son correctos."
            });
        }

        _failedLogins.TryRemove(normalizedEmail, out _);

        if (!user.IsActive)
            return Unauthorized(new ProblemDetails
            {
                Title = "Usuario inactivo",
                Status = 401,
                Detail = "La cuenta de usuario está desactivada."
            });

        user.LastLoginAt = DateTime.UtcNow;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = _currentUser.GetIpAddress()
        }).Entity;

        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = expiresIn,
            User = new UserInfo
            {
                Id = user.Id,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Roles = roles,
                Permissions = permissions
            }
        });
    }

    [HttpPost("refresh-token")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
            return Unauthorized(new ProblemDetails
            {
                Title = "Token inválido",
                Status = 401,
                Detail = "El refresh token no es válido."
            });

        if (storedToken.RevokedAt != null)
        {
            var userId = storedToken.UserId;
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in userTokens)
                token.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Unauthorized(new ProblemDetails
            {
                Title = "Posible robo de token",
                Status = 401,
                Detail = "El refresh token ya fue usado. Todos los tokens fueron revocados por seguridad."
            });
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new ProblemDetails
            {
                Title = "Token expirado",
                Status = 401,
                Detail = "El refresh token ha expirado."
            });

        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = _currentUser.GetIpAddress()
        }).Entity;

        await _context.SaveChangesAsync();

        return Ok(new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = expiresIn
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken != null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var delay = RandomNumberGenerator.GetInt32(500, 1500);
        await Task.Delay(delay);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.DeletedAt == null);

        if (user != null)
        {
            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

            await _context.SaveChangesAsync();

            await _emailSender.SendAsync(
                user.Email,
                "Recuperación de contraseña",
                $"Usa este token para recuperar tu contraseña: {token}");
        }

        return Ok(new { message = "Si el correo existe, recibirás instrucciones para recuperar tu contraseña." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token && t.UsedAt == null);

        if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new ProblemDetails
            {
                Title = "Token inválido",
                Status = 400,
                Detail = "El token de recuperación no es válido o ha expirado."
            });

        resetToken.UsedAt = DateTime.UtcNow;
        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> Me()
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
            return NotFound();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        return Ok(new UserInfo
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Roles = roles,
            Permissions = permissions
        });
    }

    private static bool IsAccountLocked(string email)
    {
        if (_failedLogins.TryGetValue(email, out var info))
        {
            if (info.Attempts >= 5 && DateTime.UtcNow < info.LockedUntil)
                return true;

            if (DateTime.UtcNow >= info.LockedUntil)
                _failedLogins.TryRemove(email, out _);
        }

        return false;
    }

    private static void RecordFailedAttempt(string email)
    {
        _failedLogins.AddOrUpdate(email,
            _ => new FailedLoginInfo
            {
                Attempts = 1,
                LockedUntil = DateTime.UtcNow.AddMinutes(15),
                FirstAttemptAt = DateTime.UtcNow
            },
            (_, info) =>
            {
                info.Attempts++;

                if (info.Attempts >= 5)
                    info.LockedUntil = DateTime.UtcNow.AddMinutes(15);

                return info;
            });
    }

    private class FailedLoginInfo
    {
        public int Attempts { get; set; }
        public DateTime LockedUntil { get; set; }
        public DateTime FirstAttemptAt { get; set; }
    }
}
