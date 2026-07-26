namespace ProyectoAvengers.Shared.DTOs.Admin;

public class AboutInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AboutGalleryDto> Gallery { get; set; } = new();
}

public class AboutGalleryDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public string Section { get; set; } = string.Empty;
}

public class UpdateAboutInfoRequest
{
    public string Title { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string? Mission { get; set; }
    public string? Vision { get; set; }
}

public class UpdateGalleryOrderItem
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
}