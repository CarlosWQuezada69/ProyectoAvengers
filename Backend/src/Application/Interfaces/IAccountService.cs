using ProyectoAvengers.Shared.DTOs.Account;

namespace ProyectoAvengers.Application.Interfaces;

public interface IAccountService
{
    Task<ProfileResponse?> GetProfileAsync(Guid? userId, CancellationToken ct = default);
    Task<ProfileResponse?> UpdateProfileAsync(Guid? userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<(bool success, string? message, int? statusCode)> ChangePasswordAsync(Guid? userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<(bool success, string? message, string? confirmationUrl)> ChangeEmailRequestAsync(Guid? userId, ChangeEmailRequest request, CancellationToken ct = default);
    Task<(bool success, string? message, int? statusCode)> ChangeEmailConfirmAsync(string token, CancellationToken ct = default);
}
