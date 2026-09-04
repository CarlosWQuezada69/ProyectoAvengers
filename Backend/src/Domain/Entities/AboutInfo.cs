namespace ProyectoAvengers.Domain.Entities;

public class AboutInfo
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string History { get; private set; } = string.Empty;
    public string? Mission { get; private set; }
    public string? Vision { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<AboutGallery> Galleries { get; private set; } = new List<AboutGallery>();

    private AboutInfo() { }

    public AboutInfo(string title, string history, string? mission, string? vision)
    {
        Id = Guid.NewGuid();
        Title = title;
        History = history;
        Mission = mission;
        Vision = vision;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string history, string? mission, string? vision)
    {
        Title = title;
        History = history;
        Mission = mission;
        Vision = vision;
        UpdatedAt = DateTime.UtcNow;
    }

    public AboutGallery AddImage(string url, string? altText, int displayOrder, string section)
    {
        var image = new AboutGallery(Id, url, altText, displayOrder, section);
        Galleries.Add(image);
        return image;
    }

    public void RemoveImage(AboutGallery image) => Galleries.Remove(image);
}