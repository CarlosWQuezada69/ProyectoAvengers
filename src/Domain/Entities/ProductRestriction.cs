namespace ProyectoAvengers.Domain.Entities;

public class ProductRestriction
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string RestrictionType { get; set; } = string.Empty;
    public string Config { get; set; } = "{}";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;
}
