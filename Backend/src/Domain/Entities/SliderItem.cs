namespace ProyectoAvengers.Domain.Entities;

public class SliderItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Subtitle { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string? LinkUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User? CreatedByUser { get; private set; }

    private SliderItem() { }

    public SliderItem(string title, string? subtitle, string imageUrl, string? linkUrl,
        int displayOrder, DateTime? startsAt, DateTime? endsAt, bool isActive, Guid? createdByUserId)
    {
        Id = Guid.NewGuid();
        Title = title;
        Subtitle = subtitle;
        ImageUrl = imageUrl;
        LinkUrl = linkUrl;
        DisplayOrder = displayOrder;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? subtitle, string? linkUrl,
        int displayOrder, DateTime? startsAt, DateTime? endsAt, bool isActive)
    {
        Title = title;
        Subtitle = subtitle;
        LinkUrl = linkUrl;
        DisplayOrder = displayOrder;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = isActive;
    }

    public void UpdateOrder(int displayOrder) => DisplayOrder = displayOrder;
}
