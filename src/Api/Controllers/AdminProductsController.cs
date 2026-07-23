using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

public class AdminProductsController : AdminBaseController
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public AdminProductsController(
        AppDbContext context,
        IFileStorage fileStorage,
        ICurrentUserService currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    [HttpGet("products")]
    [RequirePermission("products.view")]
    public async Task<ActionResult<PaginatedResponse<ProductListDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) || p.Sku.ToLower().Contains(searchTerm));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                Stock = p.Stock,
                CategoryName = p.Category != null ? p.Category.Name : null,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                PrimaryImageUrl = p.ProductImages
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Ok(new PaginatedResponse<ProductListDto>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("products/{id:guid}")]
    [RequirePermission("products.view")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.ProductRestrictions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        var dto = MapToDetailDto(product);
        dto.RowVersion = product.RowVersion != null ? Convert.ToBase64String(product.RowVersion) : null;
        return Ok(dto);
    }

    [HttpPost("products")]
    [RequirePermission("products.create")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
            return Conflict(new ProblemDetails
            {
                Title = "SKU duplicado",
                Status = 409,
                Detail = "Ya existe un producto con ese SKU."
            });

        if (await _context.Products.AnyAsync(p => p.Slug == request.Slug))
            return Conflict(new ProblemDetails
            {
                Title = "Slug duplicado",
                Status = 409,
                Detail = "Ya existe un producto con ese slug."
            });

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            CreatedByUserId = _currentUser.GetUserId(),
            RowVersion = Guid.NewGuid().ToByteArray()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var dto = MapToDetailDto(product);
        dto.RowVersion = Convert.ToBase64String(product.RowVersion!);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, dto);
    }

    [HttpPut("products/{id:guid}")]
    [RequirePermission("products.update")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        if (await _context.Products.AnyAsync(p => p.Sku == request.Sku && p.Id != id))
            return Conflict(new ProblemDetails
            {
                Title = "SKU duplicado",
                Status = 409,
                Detail = "Ya existe otro producto con ese SKU."
            });

        if (await _context.Products.AnyAsync(p => p.Slug == request.Slug && p.Id != id))
            return Conflict(new ProblemDetails
            {
                Title = "Slug duplicado",
                Status = 409,
                Detail = "Ya existe otro producto con ese slug."
            });

        if (request.RowVersion != null)
        {
            var clientVersion = Convert.FromBase64String(request.RowVersion);
            if (!product.RowVersion!.SequenceEqual(clientVersion))
                return Conflict(new ProblemDetails
                {
                    Title = "Conflicto de concurrencia",
                    Status = 409,
                    Detail = "El producto fue modificado por otro usuario. Recarga los datos e intenta de nuevo."
                });
        }

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;
        product.RowVersion = Guid.NewGuid().ToByteArray();

        await _context.SaveChangesAsync();

        var dto = MapToDetailDto(product);
        dto.RowVersion = Convert.ToBase64String(product.RowVersion!);
        return Ok(dto);
    }

    [HttpDelete("products/{id:guid}")]
    [RequirePermission("products.delete")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        product.DeletedAt = DateTime.UtcNow;
        product.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("products/{id:guid}/images")]
    [RequirePermission("products.update")]
    public async Task<ActionResult<ProductImageDto>> UploadImage(Guid id, IFormFile file)
    {
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Archivo vacío", Status = 400 });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new ProblemDetails { Title = "Tipo no válido", Status = 400, Detail = "Solo se permiten JPEG, PNG, WebP y GIF." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new ProblemDetails { Title = "Archivo muy grande", Status = 400, Detail = "El tamaño máximo es 5 MB." });

        await using var stream = file.OpenReadStream();
        var url = await _fileStorage.SaveAsync(stream, file.FileName, "products");

        var isPrimary = !product.ProductImages.Any();

        var image = new ProductImage
        {
            ProductId = id,
            Url = url,
            AltText = file.FileName,
            DisplayOrder = product.ProductImages.Count,
            IsPrimary = isPrimary
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync();

        return Ok(new ProductImageDto
        {
            Id = image.Id,
            Url = image.Url,
            AltText = image.AltText,
            DisplayOrder = image.DisplayOrder,
            IsPrimary = image.IsPrimary
        });
    }

    [HttpDelete("products/{id:guid}/images/{imageId:guid}")]
    [RequirePermission("products.update")]
    public async Task<ActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);

        if (image == null)
            return NotFound();

        await _fileStorage.DeleteAsync(image.Url);
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("products/{id:guid}/images/order")]
    [RequirePermission("products.update")]
    public async Task<ActionResult> UpdateImageOrder(Guid id, [FromBody] List<UpdateImageOrderItem> order)
    {
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        foreach (var item in order)
        {
            var image = product.ProductImages.FirstOrDefault(i => i.Id == item.ImageId);
            if (image != null)
                image.DisplayOrder = item.DisplayOrder;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("products/{id:guid}/restrictions")]
    [RequirePermission("products.manage-restrictions")]
    public async Task<ActionResult<ProductRestrictionDto>> CreateRestriction(Guid id, [FromBody] CreateRestrictionRequest request)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.Id == id);

        if (!productExists)
            return NotFound();

        var restriction = new ProductRestriction
        {
            ProductId = id,
            RestrictionType = request.RestrictionType,
            Config = request.Config,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = request.IsActive
        };

        _context.ProductRestrictions.Add(restriction);
        await _context.SaveChangesAsync();

        return Ok(new ProductRestrictionDto
        {
            Id = restriction.Id,
            RestrictionType = restriction.RestrictionType,
            Config = restriction.Config,
            StartsAt = restriction.StartsAt,
            EndsAt = restriction.EndsAt,
            IsActive = restriction.IsActive
        });
    }

    [HttpPut("products/{id:guid}/restrictions/{restrictionId:guid}")]
    [RequirePermission("products.manage-restrictions")]
    public async Task<ActionResult<ProductRestrictionDto>> UpdateRestriction(Guid id, Guid restrictionId, [FromBody] UpdateRestrictionRequest request)
    {
        var restriction = await _context.ProductRestrictions
            .FirstOrDefaultAsync(r => r.Id == restrictionId && r.ProductId == id);

        if (restriction == null)
            return NotFound();

        restriction.RestrictionType = request.RestrictionType;
        restriction.Config = request.Config;
        restriction.StartsAt = request.StartsAt;
        restriction.EndsAt = request.EndsAt;
        restriction.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new ProductRestrictionDto
        {
            Id = restriction.Id,
            RestrictionType = restriction.RestrictionType,
            Config = restriction.Config,
            StartsAt = restriction.StartsAt,
            EndsAt = restriction.EndsAt,
            IsActive = restriction.IsActive
        });
    }

    [HttpDelete("products/{id:guid}/restrictions/{restrictionId:guid}")]
    [RequirePermission("products.manage-restrictions")]
    public async Task<ActionResult> DeleteRestriction(Guid id, Guid restrictionId)
    {
        var restriction = await _context.ProductRestrictions
            .FirstOrDefaultAsync(r => r.Id == restrictionId && r.ProductId == id);

        if (restriction == null)
            return NotFound();

        _context.ProductRestrictions.Remove(restriction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ProductDto MapToDetailDto(Product product)
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
            Images = product.ProductImages?.Select(i => new ProductImageDto
            {
                Id = i.Id,
                Url = i.Url,
                AltText = i.AltText,
                DisplayOrder = i.DisplayOrder,
                IsPrimary = i.IsPrimary
            }).ToList() ?? new(),
            Restrictions = product.ProductRestrictions?.Select(r => new ProductRestrictionDto
            {
                Id = r.Id,
                RestrictionType = r.RestrictionType,
                Config = r.Config,
                StartsAt = r.StartsAt,
                EndsAt = r.EndsAt,
                IsActive = r.IsActive
            }).ToList() ?? new()
        };
    }
}
