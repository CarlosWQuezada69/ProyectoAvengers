using Microsoft.Extensions.Configuration;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Services;

namespace ProyectoAvengers.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "TestSecretKey_MinLength32Chars!!!_12345678",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "15"
            })
            .Build();

        _tokenService = new TokenService(new JwtOptions(configuration));
    }

    [Fact]
    public void GenerateAccessToken_ReturnsTokenAndExpiry()
    {
        var user = new User("Test", "User", "test@test.com", "hash", null);

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
        var user = new User("Test", "User", "test@test.com", "hash", null);

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
