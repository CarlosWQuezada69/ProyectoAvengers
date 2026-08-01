namespace ProyectoAvengers.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    public Category? ParentCategory { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { }

    public Category(Guid? parentCategoryId, string name, string slug, string? description,
        string? imageUrl, bool isActive, int displayOrder)
    {
        Id = Guid.NewGuid();
        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(Guid? parentCategoryId, string name, string slug, string? description,
        string? imageUrl, bool isActive, int displayOrder)
    {
        ParentCategoryId = parentCategoryId;
        Name = name;
        Slug = slug;
        Description = description;
        ImageUrl = imageUrl;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    public bool HasActiveProducts() => Products.Any(p => p.DeletedAt == null);
}
