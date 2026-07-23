using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/slider")]
public class SliderController : ControllerBase
{
    private readonly AppDbContext _context;

    public SliderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<SliderItemDto>>> GetSlider()
    {
        var now = DateTime.UtcNow;

        var items = await _context.SliderItems
            .Where(s => s.IsActive &&
                (s.StartsAt == null || s.StartsAt <= now) &&
                (s.EndsAt == null || s.EndsAt >= now))
            .OrderBy(s => s.DisplayOrder)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(items.Select(s => new SliderItemDto
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
        }).ToList());
    }
}
