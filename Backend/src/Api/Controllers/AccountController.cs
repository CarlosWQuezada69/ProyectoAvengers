using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Account;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        AppDbContext context,
        ICurrentUserService currentUser,
        IEmailSender emailSender,
        IWebHostEnvironment environment)
    {
        _context = context;
        _currentUser = currentUser;
        _emailSender = emailSender;
        _environment = environment;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ProfileResponse>> GetProfile()
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

        return Ok(new ProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        if (user == null)
            return NotFound();

        user.UpdateDetails(request.FirstName.Trim(), request.LastName.Trim(),
            string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(), user.IsActive);

        await _context.SaveChangesAsync();

        return Ok(new ProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        if (user == null)
            return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new ProblemDetails
            {
                Title = "Contraseña actual incorrecta",
                Status = 400,
                Detail = "La contraseña actual no coincide. Verifícala e inténtalo de nuevo."
            });

        if (request.CurrentPassword == request.NewPassword)
            return BadRequest(new ProblemDetails
            {
                Title = "Contraseña repetida",
                Status = 400,
                Detail = "La nueva contraseña debe ser diferente a la actual."
            });

        user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();
        foreach (var token in activeTokens)
            token.Revoke();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    [HttpPost("change-email/request")]
    [Authorize]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ChangeEmailRequest([FromBody] ChangeEmailRequest request)
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail && u.Id != userId))
            return Conflict(new ProblemDetails
            {
                Title = "Correo en uso",
                Status = 409,
                Detail = "El correo electrónico ya está registrado por otro usuario."
            });

        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        _context.EmailChangeRequests.Add(new Domain.Entities.EmailChangeRequest(
            user.Id, request.NewEmail, token, DateTime.UtcNow.AddHours(24)));

        await _context.SaveChangesAsync();

        await _emailSender.SendAsync(
            request.NewEmail,
            "Confirmación de cambio de correo",
            $"Usa este enlace para confirmar tu nuevo correo: " +
            $"{Request.Scheme}://{Request.Host}/api/v1/account/change-email/confirm?token={token}");

        var message = "Se ha enviado un correo de confirmación a la nueva dirección.";

        if (_environment.IsDevelopment())
            return Ok(new { message, confirmationUrl = $"{Request.Scheme}://{Request.Host}/api/v1/account/change-email/confirm?token={token}" });

        return Ok(new { message });
    }

    [HttpGet("change-email/confirm")]
    public async Task<ActionResult> ChangeEmailConfirm([FromQuery] ChangeEmailConfirmRequest request)
    {
        var changeRequest = await _context.EmailChangeRequests
            .AsTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.Token && r.ConfirmedAt == null);

        if (changeRequest == null || changeRequest.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new ProblemDetails
            {
                Title = "Token inválido",
                Status = 400,
                Detail = "El token de confirmación no es válido o ha expirado."
            });

        changeRequest.Confirm();
        changeRequest.User.ChangeEmail(changeRequest.NewEmail);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Correo electrónico actualizado correctamente." });
    }
}
