using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class AdminAboutController : AdminBaseController
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;

    public AdminAboutController(AppDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    [HttpGet("about")]
    [RequirePermission("about.view")]
    public async Task<ActionResult<AboutInfoDto>> GetAbout()
    {
        var about = await _context.AboutInfos
            .AsNoTracking()
            .Include(a => a.Galleries.OrderBy(g => g.DisplayOrder))
            .FirstOrDefaultAsync();

        if (about == null)
            return Ok(new AboutInfoDto());

        return Ok(MapToDto(about));
    }

    [HttpPut("about")]
    [RequirePermission("about.update")]
    public async Task<ActionResult<AboutInfoDto>> UpdateAbout([FromBody] UpdateAboutInfoRequest request)
    {
        var about = await _context.AboutInfos
            .Include(a => a.Galleries)
            .FirstOrDefaultAsync();

        if (about == null)
        {
            about = new AboutInfo
            {
                Title = request.Title,
                History = request.History,
                Mission = request.Mission,
                Vision = request.Vision
            };
            _context.AboutInfos.Add(about);
        }
        else
        {
            about.Title = request.Title;
            about.History = request.History;
            about.Mission = request.Mission;
            about.Vision = request.Vision;
            about.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var dto = MapToDto(about);
        return Ok(dto);
    }

    [HttpPost("about/gallery")]
    [RequirePermission("about.update")]
    public async Task<ActionResult<AboutGalleryDto>> UploadImage(
        [FromQuery] string section, IFormFile file)
    {
        var about = await _context.AboutInfos
            .Include(a => a.Galleries)
            .FirstOrDefaultAsync();

        if (about == null)
        {
            about = new AboutInfo();
            _context.AboutInfos.Add(about);
            await _context.SaveChangesAsync();
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new ProblemDetails
            {
                Title = "Tipo no válido",
                Status = 400,
                Detail = "Solo se permiten JPEG, PNG, WebP y GIF."
            });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new ProblemDetails
            {
                Title = "Archivo muy grande",
                Status = 400,
                Detail = "El tamaño máximo es 5 MB."
            });

        var folder = section switch
        {
            "founder" => "about/founder",
            "employees" => "about/employees",
            "location" => "about/location",
            _ => "about"
        };

        await using var stream = file.OpenReadStream();
        var url = await _fileStorage.SaveAsync(stream, file.FileName, folder);

        var image = new AboutGallery
        {
            AboutInfoId = about.Id,
            Url = url,
            AltText = file.FileName,
            DisplayOrder = about.Galleries.Count,
            Section = section
        };

        _context.AboutGalleries.Add(image);
        await _context.SaveChangesAsync();

        return Ok(new AboutGalleryDto
        {
            Id = image.Id,
            Url = image.Url,
            AltText = image.AltText,
            DisplayOrder = image.DisplayOrder,
            Section = image.Section
        });
    }

    [HttpDelete("about/gallery/{id:guid}")]
    [RequirePermission("about.update")]
    public async Task<ActionResult> DeleteImage(Guid id)
    {
        var image = await _context.AboutGalleries
            .FirstOrDefaultAsync(i => i.Id == id);

        if (image == null)
            return NotFound();

        await _fileStorage.DeleteAsync(image.Url);
        _context.AboutGalleries.Remove(image);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("about/gallery/order")]
    [RequirePermission("about.update")]
    public async Task<ActionResult> UpdateOrder([FromBody] List<UpdateGalleryOrderItem> order)
    {
        var ids = order.Select(o => o.Id).ToList();
        var images = await _context.AboutGalleries
            .Where(i => ids.Contains(i.Id))
            .ToListAsync();

        foreach (var item in order)
        {
            var image = images.FirstOrDefault(i => i.Id == item.Id);
            if (image != null)
                image.DisplayOrder = item.DisplayOrder;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static AboutInfoDto MapToDto(AboutInfo about)
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
            Gallery = about.Galleries
                .OrderBy(g => g.DisplayOrder)
                .Select(g => new AboutGalleryDto
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