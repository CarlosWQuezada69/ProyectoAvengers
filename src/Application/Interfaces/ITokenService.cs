using System.Security.Claims;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Application.Interfaces;

public interface ITokenService
{
    (string token, int expiresIn) GenerateAccessToken(User user, List<string> roles, List<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateRefreshToken(string token);
}
