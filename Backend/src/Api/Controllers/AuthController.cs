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

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.DeletedAt == null);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            if (user != null)
            {
                user.RecordFailedLogin();
                await _context.SaveChangesAsync();
            }
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales inválidas",
                Status = 401,
                Detail = "El correo o la contraseña no son correctos."
            });
        }

        if (user.LockedUntilUtc.HasValue && user.LockedUntilUtc > DateTime.UtcNow)
            return Unauthorized(new ProblemDetails
            {
                Title = "Cuenta bloqueada",
                Status = 401,
                Detail = "Demasiados intentos fallidos. Intenta de nuevo en 15 minutos."
            });

        user.ResetFailedLogins();
        user.RecordLogin();

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _context.RefreshTokens.Add(new RefreshToken(
            user.Id, _tokenService.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(7), _currentUser.GetIpAddress())).Entity;

        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
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
        });
    }

    [HttpPost("refresh-token")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
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
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in userTokens)
                token.Revoke();

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

            storedToken.Revoke();

        var user = storedToken.User;

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();

        var (accessToken, expiresIn) = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _context.RefreshTokens.Add(new RefreshToken(
            user.Id, _tokenService.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(7), _currentUser.GetIpAddress())).Entity;

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
        storedToken.Revoke();
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.DeletedAt == null);

        if (user != null)
        {
            var tokenBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

            _context.PasswordResetTokens.Add(new PasswordResetToken(
                user.Id, token, DateTime.UtcNow.AddHours(1)));

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

        resetToken.MarkAsUsed();
        resetToken.User.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

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
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
            return NotFound();

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync();

        return Ok(new UserInfo
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = roles,
            Permissions = permissions
        });
    }
}
