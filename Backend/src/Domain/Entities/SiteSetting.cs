namespace ProyectoAvengers.Domain.Entities;

public class SiteSetting
{
    public Guid Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string? Value { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public User? UpdatedByUser { get; private set; }

    private SiteSetting() { }

    public SiteSetting(string key, string? value, Guid? updatedByUserId)
    {
        Id = Guid.NewGuid();
        Key = key;
        Value = value;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string? value, Guid? updatedByUserId)
    {
        Value = value;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }
}
