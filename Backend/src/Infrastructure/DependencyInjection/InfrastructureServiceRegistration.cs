using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISliderService, SliderService>();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (environment == "Development")
        {
            services.AddScoped<IEmailSender, MockEmailSender>();
        }
        else
        {
            var smtpHost = configuration["Email:SmtpHost"] ?? Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST");
            var smtpPort = configuration["Email:SmtpPort"] ?? Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ?? "587";
            var smtpUser = configuration["Email:SmtpUser"] ?? Environment.GetEnvironmentVariable("EMAIL_SMTP_USER");
            var smtpPass = configuration["Email:SmtpPassword"] ?? Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD");
            var fromEmail = configuration["Email:FromEmail"] ?? Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "noreply@avengers.com";

            if (!string.IsNullOrWhiteSpace(smtpHost) && !string.IsNullOrWhiteSpace(smtpUser))
            {
                services.AddScoped<IEmailSender>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<SmtpEmailSender>>();
                    return new SmtpEmailSender(smtpHost, int.Parse(smtpPort), smtpUser, smtpPass ?? string.Empty, fromEmail, logger);
                });
            }
            else
            {
                services.AddScoped<IEmailSender>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<MockEmailSender>>();
                    logger.LogWarning("SMTP no configurado. Usando MockEmailSender en producción. Configura Email:SmtpHost, Email:SmtpUser, Email:SmtpPassword.");
                    return new MockEmailSender(logger);
                });
            }
        }

        var storagePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        var storageUrl = configuration["FileStorage:Url"] ?? "/uploads";
        services.AddSingleton<IFileStorage>(new LocalFileStorage(storagePath, storageUrl));

        services.AddSingleton<IViewTracker, InMemoryViewTracker>();
        services.AddHostedService<StatsFlushJob>();

        return services;
    }
}
