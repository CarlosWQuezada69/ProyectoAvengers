namespace ProyectoAvengers.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public int Stock { get; private set; }
    public Guid? CategoryId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public byte[]? RowVersion { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public Category? Category { get; private set; }
    public User? CreatedByUser { get; private set; }
    public ICollection<ProductImage> ProductImages { get; private set; } = new List<ProductImage>();
    public ICollection<ProductRestriction> ProductRestrictions { get; private set; } = new List<ProductRestriction>();
    public ICollection<ProductStatsDaily> ProductStatsDailies { get; private set; } = new List<ProductStatsDaily>();

    private Product() { }

    public Product(string sku, string name, string slug, string? description, decimal price,
        decimal? compareAtPrice, int stock, Guid? categoryId, bool isActive, bool isFeatured, Guid? createdByUserId)
    {
        Id = Guid.NewGuid();
        Sku = sku;
        Name = name;
        Slug = slug;
        Description = description;
        Price = price;
        CompareAtPrice = compareAtPrice;
        Stock = stock;
        CategoryId = categoryId;
        IsActive = isActive;
        IsFeatured = isFeatured;
        CreatedByUserId = createdByUserId;
        RowVersion = Guid.NewGuid().ToByteArray();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string sku, string name, string slug, string? description, decimal price,
        decimal? compareAtPrice, int stock, Guid? categoryId, bool isActive, bool isFeatured, byte[]? clientRowVersion)
    {
        if (clientRowVersion != null && RowVersion != null && !RowVersion.SequenceEqual(clientRowVersion))
            throw new InvalidOperationException("El producto fue modificado por otro usuario. Recarga los datos e intenta de nuevo.");

        Sku = sku;
        Name = name;
        Slug = slug;
        Description = description;
        Price = price;
        CompareAtPrice = compareAtPrice;
        Stock = stock;
        CategoryId = categoryId;
        IsActive = isActive;
        IsFeatured = isFeatured;
        UpdatedAt = DateTime.UtcNow;
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }
}
