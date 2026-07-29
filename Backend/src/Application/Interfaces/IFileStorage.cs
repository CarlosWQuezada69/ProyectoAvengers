namespace ProyectoAvengers.Application.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string url, CancellationToken ct = default);
}
