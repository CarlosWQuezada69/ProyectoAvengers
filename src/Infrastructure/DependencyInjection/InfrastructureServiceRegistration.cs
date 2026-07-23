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
            ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT")
            ?? "Host=localhost;Database=proyecto_avengers;Username=postgres;Password=postgres";

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
