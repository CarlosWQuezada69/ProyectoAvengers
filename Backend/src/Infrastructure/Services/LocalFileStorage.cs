using Microsoft.AspNetCore.Http;
using ProyectoAvengers.Application.Interfaces;

namespace ProyectoAvengers.Infrastructure.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorage(string basePath, string baseUrl)
    {
        _basePath = basePath;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default)
    {
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);

        var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
        var filePath = Path.Combine(dir, uniqueName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(stream, ct);

        return $"{_baseUrl}/{folder}/{uniqueName}";
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (!url.StartsWith(_baseUrl))
            return Task.CompletedTask;

        var relativePath = url[_baseUrl.Length..].TrimStart('/');
        var filePath = Path.Combine(_basePath, relativePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}
