namespace ProyectoAvengers.Shared.DTOs.Admin;

public class SliderItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSliderItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSliderItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSliderOrderItem
{
    public Guid Id { get; set; }
    public int DisplayOrder { get; set; }
}
