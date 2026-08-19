using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Application.Mapping;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context) => _context = context;

    public async Task<List<CategoryDto>> ListAsync(bool includeInactive)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Include(c => c.Children)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories.Where(c => c.ParentCategoryId == null)
            .Select(c => c.ToDto())
            .ToList();
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        return category?.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        if (await _context.Categories.AnyAsync(c => c.Slug == request.Slug))
            throw new InvalidOperationException("Ya existe una categoría con ese slug.");

        var category = new Category(request.ParentCategoryId, request.Name,
            request.Slug, request.Description, request.ImageUrl,
            request.IsActive, request.DisplayOrder);

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category.ToDto();
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.AsTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return null;

        if (await _context.Categories.AnyAsync(c => c.Slug == request.Slug && c.Id != id))
            throw new InvalidOperationException("Ya existe otra categoría con ese slug.");

        category.UpdateDetails(request.ParentCategoryId, request.Name,
            request.Slug, request.Description, request.ImageUrl,
            request.IsActive, request.DisplayOrder);

        await _context.SaveChangesAsync();
        return category.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _context.Categories
            .AsTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        if (category.HasActiveProducts())
            throw new InvalidOperationException("No se puede eliminar: tiene productos activos asociados.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
