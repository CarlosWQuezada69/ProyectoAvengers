namespace ProyectoAvengers.Domain;

public static class Constants
{
    public const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    public static readonly string[] AllowedImageMimeTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];

    public const long MaxImageSizeBytes = 5 * 1024 * 1024;
}
