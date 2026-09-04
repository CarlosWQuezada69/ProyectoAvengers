namespace ProyectoAvengers.Domain.Entities;

public class AboutGallery
{
    public Guid Id { get; private set; }
    public Guid AboutInfoId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }
    public string Section { get; private set; } = string.Empty;

    public AboutInfo AboutInfo { get; private set; } = null!;

    private AboutGallery() { }

    public AboutGallery(Guid aboutInfoId, string url, string? altText, int displayOrder, string section)
    {
        Id = Guid.NewGuid();
        AboutInfoId = aboutInfoId;
        Url = url;
        AltText = altText;
        DisplayOrder = displayOrder;
        Section = section;
    }

    public void UpdateOrder(int displayOrder) => DisplayOrder = displayOrder;
}