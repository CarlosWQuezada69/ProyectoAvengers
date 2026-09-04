using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs.Account;
using ProyectoAvengers.Shared.DTOs.Auth;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var (response, statusCode, title, detail) =
            await _authService.LoginAsync(request, _currentUser.GetIpAddress());

        if (statusCode.HasValue)
            return StatusCode(statusCode.Value, new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = detail
            });

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var (response, statusCode, title, detail) =
            await _authService.RefreshTokenAsync(request.RefreshToken, _currentUser.GetIpAddress());

        if (statusCode.HasValue)
            return StatusCode(statusCode.Value, new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = detail
            });

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var (_, _, resetUrl) = await _authService.ForgotPasswordAsync(request.Email, null);

        var message = "Si el correo existe, recibirás instrucciones para recuperar tu contraseña.";
        return resetUrl == null
            ? Ok(new { message })
            : Ok(new { message, resetUrl });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("Auth")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var (success, message, statusCode) =
            await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
            return BadRequest(new ProblemDetails
            {
                Title = "Token inválido",
                Status = statusCode,
                Detail = message
            });

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> Me()
    {
        var userId = _currentUser.GetUserId();
        var response = await _authService.GetMeAsync(userId);

        if (response == null)
            return userId == null ? Unauthorized() : NotFound();

        return Ok(response.User);
    }
}
