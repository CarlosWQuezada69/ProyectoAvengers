namespace ProyectoAvengers.Shared.DTOs;

public class FileUpload
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
    public bool HasFile => Length > 0 && Content != Stream.Null;
}
