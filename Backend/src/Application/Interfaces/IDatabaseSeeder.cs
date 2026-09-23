namespace ProyectoAvengers.Application.Interfaces;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task SeedDemoDataAsync(CancellationToken cancellationToken = default);
}
