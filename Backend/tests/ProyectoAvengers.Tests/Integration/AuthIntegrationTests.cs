using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;
using ProyectoAvengers.Shared.DTOs.Auth;

namespace ProyectoAvengers.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedUserAsync(string email = "admin@example.com", string password = "Admin123!")
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Users.Add(new ProyectoAvengers.Domain.Entities.User(
            "Admin", "Test", email, BCrypt.Net.BCrypt.HashPassword(password), null));

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Xunit.Sdk.XunitException($"Seeding user failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        await SeedUserAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "admin@example.com",
            Password = "Admin123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(content);
        Assert.False(string.IsNullOrWhiteSpace(content.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(content.RefreshToken));
        Assert.Equal("admin@example.com", content.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        await SeedUserAsync("invalid-user@example.com");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "invalid-user@example.com",
            Password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", new RefreshTokenRequest
        {
            RefreshToken = "invalid-token"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = "invalid-token",
            newPassword = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
