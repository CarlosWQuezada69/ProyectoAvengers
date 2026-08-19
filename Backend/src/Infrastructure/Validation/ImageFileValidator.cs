using ProyectoAvengers.Domain;

namespace ProyectoAvengers.Infrastructure.Validation;

public static class ImageFileValidator
{
    public static readonly string[] LogoMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    public static bool IsValid(string? contentType, long length, out string? error,
        string[]? allowedTypes = null, long? maxSizeBytes = null)
    {
        allowedTypes ??= Constants.AllowedImageMimeTypes;
        maxSizeBytes ??= Constants.MaxImageSizeBytes;

        if (string.IsNullOrWhiteSpace(contentType) || !allowedTypes.Contains(contentType))
        {
            error = $"Solo se permiten {string.Join(", ", allowedTypes.Select(t => t.Split('/').Last().ToUpperInvariant()))}.";
            return false;
        }

        if (length == 0)
        {
            error = "El archivo está vacío.";
            return false;
        }

        if (length > maxSizeBytes)
        {
            error = $"El tamaño máximo es {maxSizeBytes / (1024 * 1024)} MB.";
            return false;
        }

        error = null;
        return true;
    }
}