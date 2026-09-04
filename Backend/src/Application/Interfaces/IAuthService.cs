using ProyectoAvengers.Shared.DTOs.Account;
using ProyectoAvengers.Shared.DTOs.Auth;

namespace ProyectoAvengers.Application.Interfaces;

public interface IAuthService
{
    Task<(LoginResponse? response, int? statusCode, string? title, string? detail)> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);
    Task<(RefreshTokenResponse? response, int? statusCode, string? title, string? detail)> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<(bool success, string? message, string? resetUrl)> ForgotPasswordAsync(string email, string? frontendUrl, CancellationToken ct = default);
    Task<(bool success, string? message, int? statusCode)> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);
    Task<LoginResponse?> GetMeAsync(Guid? userId, CancellationToken ct = default);
}
