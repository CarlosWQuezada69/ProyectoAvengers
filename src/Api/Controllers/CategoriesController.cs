using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetCategories([FromQuery] bool tree = false)
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        if (tree)
        {
            var roots = categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => MapToTree(c, categories))
                .ToList();

            return Ok(roots);
        }

        return Ok(categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            ParentCategoryId = c.ParentCategoryId,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            IsActive = c.IsActive,
            DisplayOrder = c.DisplayOrder
        }).ToList());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(string slug)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        if (category == null)
            return NotFound();

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

    private static CategoryDto MapToTree(Domain.Entities.Category category, List<Domain.Entities.Category> all)
    {
        return new CategoryDto
        {
            Id = category.Id,
            ParentCategoryId = category.ParentCategoryId,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            DisplayOrder = category.DisplayOrder,
            Children = all
                .Where(c => c.ParentCategoryId == category.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => MapToTree(c, all))
                .ToList()
        };
    }
}
