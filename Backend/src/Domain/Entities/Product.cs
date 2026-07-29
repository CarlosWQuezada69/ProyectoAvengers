namespace ProyectoAvengers.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int Stock { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public byte[]? RowVersion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Category? Category { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<ProductRestriction> ProductRestrictions { get; set; } = new List<ProductRestriction>();
    public ICollection<ProductStatsDaily> ProductStatsDailies { get; set; } = new List<ProductStatsDaily>();
}
