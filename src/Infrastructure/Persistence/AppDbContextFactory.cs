using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProyectoAvengers.Application.Interfaces;

namespace ProyectoAvengers.Infrastructure.Persistence;

internal class DesignTimeCurrentUserService : ICurrentUserService
{
    public Guid? GetUserId() => null;
    public string? GetIpAddress() => null;
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT")
            ?? "Host=localhost;Database=proyecto_avengers;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }
}
