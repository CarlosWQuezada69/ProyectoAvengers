using Microsoft.Extensions.Configuration;
using Moq;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Services;

namespace ProyectoAvengers.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _configMock = new Mock<IConfiguration>();

        _configMock.Setup(x => x.GetSection("Jwt")["Secret"]).Returns("TestSecretKey_MinLength32Chars!!!_12345678");
        _configMock.Setup(x => x.GetSection("Jwt")["Issuer"]).Returns("TestIssuer");
        _configMock.Setup(x => x.GetSection("Jwt")["Audience"]).Returns("TestAudience");
        _configMock.Setup(x => x.GetSection("Jwt")["ExpiryMinutes"]).Returns("15");

        _tokenService = new TokenService(_configMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsTokenAndExpiry()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User"
        };

        var (token, expiresIn) = _tokenService.GenerateAccessToken(user, new List<string> { "Admin" }, new List<string> { "products.view" });

        Assert.NotNull(token);
        Assert.True(token.Length > 0);
        Assert.Equal(900, expiresIn);
    }

    [Fact]
    public void GenerateRefreshToken_Returns64ByteBase64String()
    {
        var token = _tokenService.GenerateRefreshToken();
        Assert.NotNull(token);
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void ValidateRefreshToken_ValidToken_ReturnsPrincipal()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User"
        };

        var (accessToken, _) = _tokenService.GenerateAccessToken(user, new List<string>(), new List<string>());
        var principal = _tokenService.ValidateRefreshToken(accessToken);

        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateRefreshToken_InvalidToken_ReturnsNull()
    {
        var principal = _tokenService.ValidateRefreshToken("invalid_token_here");
        Assert.Null(principal);
    }
}
