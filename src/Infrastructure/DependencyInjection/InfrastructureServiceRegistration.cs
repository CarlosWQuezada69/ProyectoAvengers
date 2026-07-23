using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.BackgroundJobs;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Infrastructure.Seed;
using ProyectoAvengers.Infrastructure.Services;

namespace ProyectoAvengers.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'Default' no está configurada. Define 'ConnectionStrings:Default' en appsettings o la variable de entorno 'CONNECTIONSTRINGS__DEFAULT'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailSender, MockEmailSender>();

        var storagePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        var storageUrl = configuration["FileStorage:Url"] ?? "/uploads";
        services.AddSingleton<IFileStorage>(new LocalFileStorage(storagePath, storageUrl));

        services.AddSingleton<IViewTracker, InMemoryViewTracker>();
        services.AddHostedService<StatsFlushJob>();

        return services;
    }
}
