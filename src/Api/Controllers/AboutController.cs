using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/about")]
[EnableRateLimiting("Catalog")]
public class AboutController : ControllerBase
{
    private readonly AppDbContext _context;

    public AboutController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ResponseCache(Duration = 120)]
    public async Task<ActionResult<AboutInfoDto>> GetAbout()
    {
        var about = await _context.AboutInfo
            .AsNoTracking()
            .Include(a => a.Gallery.OrderBy(g => g.DisplayOrder))
            .FirstOrDefaultAsync();

        if (about == null)
            return Ok(new AboutInfoDto());

        return Ok(MapToDto(about));
    }

    private static AboutInfoDto MapToDto(Domain.Entities.AboutInfo about)
    {
        return new AboutInfoDto
        {
            Id = about.Id,
            Title = about.Title,
            History = about.History,
            Mission = about.Mission,
            Vision = about.Vision,
            CreatedAt = about.CreatedAt,
            UpdatedAt = about.UpdatedAt,
            Gallery = about.Gallery.Select(g => new AboutGalleryDto
            {
                Id = g.Id,
                Url = g.Url,
                AltText = g.AltText,
                DisplayOrder = g.DisplayOrder,
                Section = g.Section
            }).ToList()
        };
    }
}