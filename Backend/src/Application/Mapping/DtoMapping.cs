using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Mapping;

public static class DtoMapping
{
    public static ProductListDto ToListDto(this Product p) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Slug = p.Slug,
        Price = p.Price,
        CompareAtPrice = p.CompareAtPrice,
        Stock = p.Stock,
        CategoryName = p.Category?.Name,
        IsActive = p.IsActive,
        IsFeatured = p.IsFeatured,
        PrimaryImageUrl = p.ProductImages
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.DisplayOrder)
            .Select(i => i.Url)
            .FirstOrDefault(),
        CreatedAt = p.CreatedAt
    };

    public static ProductDto ToDto(this Product p) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        Price = p.Price,
        CompareAtPrice = p.CompareAtPrice,
        Stock = p.Stock,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name,
        IsActive = p.IsActive,
        IsFeatured = p.IsFeatured,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        RowVersion = p.RowVersion != null ? Convert.ToBase64String(p.RowVersion) : null,
        Images = p.ProductImages?.Select(i => i.ToDto()).ToList() ?? [],
        Restrictions = p.ProductRestrictions?.Select(r => r.ToDto()).ToList() ?? []
    };

    public static ProductImageDto ToDto(this ProductImage i) => new()
    {
        Id = i.Id,
        Url = i.Url,
        AltText = i.AltText,
        DisplayOrder = i.DisplayOrder,
        IsPrimary = i.IsPrimary
    };

    public static ProductRestrictionDto ToDto(this ProductRestriction r) => new()
    {
        Id = r.Id,
        RestrictionType = r.RestrictionType,
        Config = r.Config,
        StartsAt = r.StartsAt,
        EndsAt = r.EndsAt,
        IsActive = r.IsActive
    };

    public static CategoryDto ToDto(this Category c) => new()
    {
        Id = c.Id,
        ParentCategoryId = c.ParentCategoryId,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        IsActive = c.IsActive,
        DisplayOrder = c.DisplayOrder,
        Children = c.Children?.Select(child => child.ToDto()).ToList() ?? []
    };

    public static UserDto ToDto(this User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Phone = u.Phone,
        IsActive = u.IsActive,
        EmailConfirmed = u.EmailConfirmed,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt,
        RoleIds = u.UserRoles.Select(ur => ur.RoleId).ToList(),
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
    };

    public static SliderItemDto ToDto(this SliderItem s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Subtitle = s.Subtitle,
        ImageUrl = s.ImageUrl,
        LinkUrl = s.LinkUrl,
        DisplayOrder = s.DisplayOrder,
        StartsAt = s.StartsAt,
        EndsAt = s.EndsAt,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}
