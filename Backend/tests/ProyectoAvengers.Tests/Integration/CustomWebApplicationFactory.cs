using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;

namespace ProyectoAvengers.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "TestSecretKey_AtLeast_32_Characters_Long_1234567890";

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", TestJwtSecret);
        Environment.SetEnvironmentVariable("JWT_ISSUER", "ProyectoAvengers");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "ProyectoAvengers");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestJwtSecret,
                ["Jwt:Issuer"] = "ProyectoAvengers",
                ["Jwt:Audience"] = "ProyectoAvengers",
                ["Jwt:ExpiryMinutes"] = "15",
                ["App:FrontendUrl"] = "http://localhost:4200",
                ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=postgres;Password=test"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDatabaseSeeder>();
            services.AddScoped<IDatabaseSeeder, TestDatabaseSeeder>();

            services.RemoveAll<IHostedService>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));
        });
    }
}

public class TestDatabaseSeeder : IDatabaseSeeder
{
    public Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
