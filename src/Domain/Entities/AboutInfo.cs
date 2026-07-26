namespace ProyectoAvengers.Domain.Entities;

public class AboutInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AboutGallery> Galleries { get; set; } = new List<AboutGallery>();
}