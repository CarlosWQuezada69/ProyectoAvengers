using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Tests.Integration;

public class PublicEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PublicEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task About_ReturnsEmptyDto_WhenNoData()
    {
        var response = await _client.GetAsync("/api/v1/about");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AboutInfoDto>();
        Assert.NotNull(content);
        Assert.Equal(string.Empty, content.Title);
    }

    [Fact]
    public async Task Settings_Public_ReturnsDictionary()
    {
        var response = await _client.GetAsync("/api/v1/settings/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Products_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Categories_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Slider_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/slider");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminCollector_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/admin/stats/overview");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
