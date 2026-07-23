using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

public class AdminCategoriesController : AdminBaseController
{
    private readonly AppDbContext _context;

    public AdminCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("categories")]
    [RequirePermission("categories.create")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (await _context.Categories.AnyAsync(c => c.Slug == request.Slug))
            return Conflict(new ProblemDetails
            {
                Title = "Slug duplicado",
                Status = 409,
                Detail = "Ya existe una categoría con ese slug."
            });

        var category = new Domain.Entities.Category
        {
            ParentCategoryId = request.ParentCategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            ParentCategoryId = category.ParentCategoryId,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder
        });
    }

    [HttpPut("categories/{id:guid}")]
    [RequirePermission("categories.update")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound();

        if (await _context.Categories.AnyAsync(c => c.Slug == request.Slug && c.Id != id))
            return Conflict(new ProblemDetails
            {
                Title = "Slug duplicado",
                Status = 409,
                Detail = "Ya existe otra categoría con ese slug."
            });

        category.ParentCategoryId = request.ParentCategoryId;
        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IsActive = request.IsActive;
        category.DisplayOrder = request.DisplayOrder;

        await _context.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            ParentCategoryId = category.ParentCategoryId,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder
        });
    }

    [HttpDelete("categories/{id:guid}")]
    [RequirePermission("categories.delete")]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        if (category.Products.Any(p => p.DeletedAt == null))
            return Conflict(new ProblemDetails
            {
                Title = "Categoría en uso",
                Status = 409,
                Detail = "No se puede eliminar la categoría porque tiene productos activos asociados."
            });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
