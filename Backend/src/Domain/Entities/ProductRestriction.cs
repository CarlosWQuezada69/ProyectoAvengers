namespace ProyectoAvengers.Domain.Entities;

public class ProductRestriction
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string RestrictionType { get; private set; } = string.Empty;
    public string Config { get; private set; } = "{}";
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Product Product { get; private set; } = null!;

    private ProductRestriction() { }

    public ProductRestriction(Guid productId, string restrictionType, string config,
        DateTime? startsAt, DateTime? endsAt, bool isActive)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        RestrictionType = restrictionType;
        Config = config;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = isActive;
    }

    public void UpdateDetails(string restrictionType, string config,
        DateTime? startsAt, DateTime? endsAt, bool isActive)
    {
        RestrictionType = restrictionType;
        Config = config;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsActive = isActive;
    }
}
