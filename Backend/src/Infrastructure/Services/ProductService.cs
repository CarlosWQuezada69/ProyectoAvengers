using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Application.Mapping;
using ProyectoAvengers.Domain;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Infrastructure.Validation;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public ProductService(AppDbContext context, IFileStorage fileStorage, ICurrentUserService currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResponse<ProductListDto>> ListAsync(string? search, Guid? categoryId, bool? isActive, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Products
            .AsNoTracking()
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
            .ToListAsync();

        return new PaginatedResponse<ProductListDto>
        {
            Data = items.Select(p => p.ToListDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
            .Include(p => p.ProductRestrictions)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product?.ToDto();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        if (await _context.Products.AnyAsync(p => p.Sku == request.Sku))
            throw new InvalidOperationException("Ya existe un producto con ese SKU.");

        if (await _context.Products.AnyAsync(p => p.Slug == request.Slug))
            throw new InvalidOperationException("Ya existe un producto con ese slug.");

        var product = new Product(
            request.Sku,
            request.Name,
            request.Slug,
            request.Description,
            request.Price,
            request.CompareAtPrice,
            request.Stock,
            request.CategoryId,
            request.IsActive,
            request.IsFeatured,
            _currentUser.GetUserId()
        );

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        if (await _context.Products.AnyAsync(p => p.Sku == request.Sku && p.Id != id))
            throw new InvalidOperationException("Ya existe otro producto con ese SKU.");

        if (await _context.Products.AnyAsync(p => p.Slug == request.Slug && p.Id != id))
            throw new InvalidOperationException("Ya existe otro producto con ese slug.");

        byte[]? clientVersion = null;
        if (request.RowVersion != null)
            clientVersion = Convert.FromBase64String(request.RowVersion);

        product.UpdateDetails(
            request.Sku,
            request.Name,
            request.Slug,
            request.Description,
            request.Price,
            request.CompareAtPrice,
            request.Stock,
            request.CategoryId,
            request.IsActive,
            request.IsFeatured,
            clientVersion
        );

        await _context.SaveChangesAsync();
        return product.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return false;

        product.SoftDelete();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductImageDto?> UploadImageAsync(Guid productId, Stream fileStream, string fileName, string contentType)
    {
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return null;

        if (fileStream.Length == 0)
            throw new InvalidOperationException("Archivo vacío.");

        if (!ImageFileValidator.IsValid(contentType, fileStream.Length, out var error))
            throw new InvalidOperationException(error);

        var url = await _fileStorage.SaveAsync(fileStream, fileName, "products");

        var image = new ProductImage(productId, url, fileName, product.ProductImages.Count, !product.ProductImages.Any());

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync();

        return image.ToDto();
    }

    public async Task<bool> DeleteImageAsync(Guid productId, Guid imageId)
    {
        var image = await _context.ProductImages
            .AsTracking()
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId);

        if (image == null) return false;

        await _fileStorage.DeleteAsync(image.Url);
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateImageOrderAsync(Guid productId, List<UpdateImageOrderItem> order)
    {
        var product = await _context.Products
            .AsTracking()
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return false;

        foreach (var item in order)
        {
            var image = product.ProductImages.FirstOrDefault(i => i.Id == item.ImageId);
            if (image != null)
                image.UpdateOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductRestrictionDto?> CreateRestrictionAsync(Guid productId, CreateRestrictionRequest request)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == productId))
            return null;

        var restriction = new ProductRestriction(productId, request.RestrictionType,
            request.Config, request.StartsAt, request.EndsAt, request.IsActive);

        _context.ProductRestrictions.Add(restriction);
        await _context.SaveChangesAsync();

        return restriction.ToDto();
    }

    public async Task<ProductRestrictionDto?> UpdateRestrictionAsync(Guid productId, Guid restrictionId, UpdateRestrictionRequest request)
    {
        var restriction = await _context.ProductRestrictions
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == restrictionId && r.ProductId == productId);

        if (restriction == null) return null;

        restriction.UpdateDetails(request.RestrictionType, request.Config,
            request.StartsAt, request.EndsAt, request.IsActive);

        await _context.SaveChangesAsync();
        return restriction.ToDto();
    }

    public async Task<bool> DeleteRestrictionAsync(Guid productId, Guid restrictionId)
    {
        var restriction = await _context.ProductRestrictions
            .AsTracking()
            .FirstOrDefaultAsync(r => r.Id == restrictionId && r.ProductId == productId);

        if (restriction == null) return false;

        _context.ProductRestrictions.Remove(restriction);
        await _context.SaveChangesAsync();
        return true;
    }
}
