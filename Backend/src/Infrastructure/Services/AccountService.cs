using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Account;

namespace ProyectoAvengers.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;

    public AccountService(AppDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<ProfileResponse?> GetProfileAsync(Guid? userId, CancellationToken ct = default)
    {
        if (userId == null) return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user == null) return null;

        return BuildProfile(user, await GetRoleNamesAsync(user.Id, ct));
    }

    public async Task<ProfileResponse?> UpdateProfileAsync(Guid? userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        if (userId == null) return null;

        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user == null) return null;

        user.UpdateDetails(request.FirstName.Trim(), request.LastName.Trim(),
            string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(), user.IsActive);

        await _context.SaveChangesAsync(ct);

        return BuildProfile(user, await GetRoleNamesAsync(user.Id, ct));
    }

    public async Task<(bool success, string? message, int? statusCode)> ChangePasswordAsync(
        Guid? userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (userId == null) return (false, null, 401);

        var user = await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user == null) return (false, null, 404);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return (false, "La contraseña actual no coincide. Verifícala e inténtalo de nuevo.", 400);

        if (request.CurrentPassword == request.NewPassword)
            return (false, "La nueva contraseña debe ser diferente a la actual.", 400);

        user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var token in activeTokens)
            token.Revoke();

        await _context.SaveChangesAsync(ct);

        return (true, null, null);
    }

    public async Task<(bool success, string? message, string? confirmationUrl)> ChangeEmailRequestAsync(
        Guid? userId, ChangeEmailRequest request, CancellationToken ct = default)
    {
        if (userId == null) return (false, null, null);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return (false, null, null);

        if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail && u.Id != userId, ct))
            return (false, "El correo electrónico ya está registrado por otro usuario.", null);

        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        _context.EmailChangeRequests.Add(new Domain.Entities.EmailChangeRequest(
            user.Id, request.NewEmail, token, DateTime.UtcNow.AddHours(24)));

        await _context.SaveChangesAsync(ct);

        await _emailSender.SendAsync(request.NewEmail, "Confirmación de cambio de correo",
            $"Usa este enlace para confirmar tu nuevo correo: /api/v1/account/change-email/confirm?token={token}");

        return (true, null, $"/api/v1/account/change-email/confirm?token={token}");
    }

    public async Task<(bool success, string? message, int? statusCode)> ChangeEmailConfirmAsync(
        string token, CancellationToken ct = default)
    {
        var changeRequest = await _context.EmailChangeRequests
            .AsTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && r.ConfirmedAt == null, ct);

        if (changeRequest == null || changeRequest.ExpiresAt < DateTime.UtcNow)
            return (false, "El token de confirmación no es válido o ha expirado.", 400);

        changeRequest.Confirm();
        changeRequest.User.ChangeEmail(changeRequest.NewEmail);

        await _context.SaveChangesAsync(ct);

        return (true, null, null);
    }

    private async Task<List<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct)
        => await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync(ct);

    private static ProfileResponse BuildProfile(Domain.Entities.User user, List<string> roles) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Phone = user.Phone,
        EmailConfirmed = user.EmailConfirmed,
        Roles = roles,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
