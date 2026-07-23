using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public AccountController(
        AppDbContext context,
        ICurrentUserService currentUser,
        IEmailSender emailSender)
    {
        _context = context;
        _currentUser = currentUser;
        _emailSender = emailSender;
    }

    [HttpPost("change-email/request")]
    [Authorize]
    public async Task<ActionResult> ChangeEmailRequest([FromBody] ChangeEmailRequest request)
    {
        var userId = _currentUser.GetUserId();
        if (userId == null)
            return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        if (user == null)
            return NotFound();

        if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail && u.Id != userId))
            return Conflict(new ProblemDetails
            {
                Title = "Correo en uso",
                Status = 409,
                Detail = "El correo electrónico ya está registrado por otro usuario."
            });

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        _context.EmailChangeRequests.Add(new Domain.Entities.EmailChangeRequest
        {
            UserId = user.Id,
            NewEmail = request.NewEmail,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });

        await _context.SaveChangesAsync();

        await _emailSender.SendAsync(
            request.NewEmail,
            "Confirmación de cambio de correo",
            $"Usa este enlace para confirmar tu nuevo correo: " +
            $"{Request.Scheme}://{Request.Host}/api/v1/account/change-email/confirm?token={token}");

        return Ok(new { message = "Se ha enviado un correo de confirmación a la nueva dirección." });
    }

    [HttpGet("change-email/confirm")]
    public async Task<ActionResult> ChangeEmailConfirm([FromQuery] ChangeEmailConfirmRequest request)
    {
        var changeRequest = await _context.EmailChangeRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.Token && r.ConfirmedAt == null);

        if (changeRequest == null || changeRequest.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new ProblemDetails
            {
                Title = "Token inválido",
                Status = 400,
                Detail = "El token de confirmación no es válido o ha expirado."
            });

        changeRequest.ConfirmedAt = DateTime.UtcNow;
        changeRequest.User.Email = changeRequest.NewEmail;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Correo electrónico actualizado correctamente." });
    }
}
