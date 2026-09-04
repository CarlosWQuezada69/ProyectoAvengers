using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs.Account;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ICurrentUserService _currentUser;

    public AccountController(IAccountService accountService, ICurrentUserService currentUser)
    {
        _accountService = accountService;
        _currentUser = currentUser;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ProfileResponse>> GetProfile()
    {
        var profile = await _accountService.GetProfileAsync(_currentUser.GetUserId());
        return profile == null ? Unauthorized() : Ok(profile);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var profile = await _accountService.UpdateProfileAsync(_currentUser.GetUserId(), request);
        return profile == null ? Unauthorized() : Ok(profile);
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var (success, message, statusCode) =
            await _accountService.ChangePasswordAsync(_currentUser.GetUserId(), request);

        if (!success)
            return StatusCode(statusCode ?? 400, new ProblemDetails
            {
                Title = statusCode == 401 ? "No autorizado" : statusCode == 404 ? "No encontrado" : "Solicitud inválida",
                Status = statusCode,
                Detail = message
            });

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    [HttpPost("change-email/request")]
    [Authorize]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ChangeEmailRequest([FromBody] ChangeEmailRequest request)
    {
        var (success, message, confirmationUrl) =
            await _accountService.ChangeEmailRequestAsync(_currentUser.GetUserId(), request);

        if (!success)
            return Conflict(new ProblemDetails
            {
                Title = "Correo en uso",
                Status = 409,
                Detail = message
            });

        var responseMessage = "Se ha enviado a la nueva dirección, si el correo ya existe.";
        if (confirmationUrl == null)
            return Ok(new { message = responseMessage });

        return Ok(new
        {
            message = "Se ha enviado un correo de confirmación a la nueva dirección.",
            confirmationUrl
        });
    }

    [HttpGet("change-email/confirm")]
    public async Task<ActionResult> ChangeEmailConfirm([FromQuery] ChangeEmailConfirmRequest request)
    {
        var (success, message, statusCode) =
            await _accountService.ChangeEmailConfirmAsync(request.Token);

        if (!success)
            return BadRequest(new ProblemDetails
            {
                Title = "Token inválido",
                Status = statusCode,
                Detail = message
            });

        return Ok(new { message = "Correo electrónico actualizado correctamente." });
    }
}
