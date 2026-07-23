using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IViewTracker _viewTracker;

    public ProductsController(AppDbContext context, IViewTracker viewTracker)
    {
        _context = context;
        _viewTracker = viewTracker;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProductListDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? onlyAvailable,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.DeletedAt == null && p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Sku.Contains(search) ||
                p.Description!.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        if (onlyAvailable == true)
            query = query.Where(p => p.Stock > 0);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(p => new ProductListDto
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
            PrimaryImageUrl = p.ProductImages.FirstOrDefault(i => i.IsPrimary)?.Url
                ?? p.ProductImages.FirstOrDefault()?.Url,
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<ProductListDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("featured")]
    public async Task<ActionResult<List<ProductListDto>>> GetFeatured()
    {
        var items = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.DeletedAt == null && p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        return Ok(items.Select(p => new ProductListDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            Slug = p.Slug,
            Price = p.Price,
            CompareAtPrice = p.CompareAtPrice,
            Stock = p.Stock,
            CategoryName = p.Category?.Name,
            IsFeatured = p.IsFeatured,
            PrimaryImageUrl = p.ProductImages.FirstOrDefault(i => i.IsPrimary)?.Url
                ?? p.ProductImages.FirstOrDefault()?.Url,
            CreatedAt = p.CreatedAt
        }).ToList());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDto>> GetProduct(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.ProductRestrictions.Where(r => r.IsActive))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.DeletedAt == null && p.IsActive);

        if (product == null)
            return NotFound();

        return Ok(MapToDto(product));
    }

    [HttpPost("{id:guid}/track-view")]
    public async Task<ActionResult> TrackView(Guid id)
    {
        var product = await _context.Products
            .AnyAsync(p => p.Id == id && p.DeletedAt == null && p.IsActive);

        if (!product)
            return NotFound();

        _viewTracker.TrackView(id);
        return Ok();
    }

    private static ProductDto MapToDto(Domain.Entities.Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Images = product.ProductImages.Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                DisplayOrder = i.DisplayOrder,
                IsPrimary = i.IsPrimary
            }).ToList(),
            Restrictions = product.ProductRestrictions.Select(r => new ProductRestrictionDto
            {
                Id = r.Id,
                RestrictionType = r.RestrictionType,
                Config = r.Config,
                StartsAt = r.StartsAt,
                EndsAt = r.EndsAt,
                IsActive = r.IsActive
            }).ToList()
        };
    }
}
