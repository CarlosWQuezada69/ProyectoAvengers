namespace ProyectoAvengers.Domain.Entities;

public class SiteSetting
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public User? UpdatedByUser { get; set; }
}
