namespace ProyectoAvengers.Domain.Entities;

public class AboutGallery
{
    public Guid Id { get; set; }
    public Guid AboutInfoId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public string Section { get; set; } = string.Empty;

    public AboutInfo AboutInfo { get; set; } = null!;
}