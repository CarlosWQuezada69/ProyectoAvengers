using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

public class AdminSliderController : AdminBaseController
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public AdminSliderController(AppDbContext context, IFileStorage fileStorage, ICurrentUserService currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    [HttpGet("slider")]
    [RequirePermission("slider.view")]
    public async Task<ActionResult<List<SliderItemDto>>> GetSlider()
    {
        var items = await _context.SliderItems
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

    [HttpPost("slider")]
    [RequirePermission("slider.create")]
    public async Task<ActionResult<SliderItemDto>> CreateSlider(IFormFile? image, [FromForm] CreateSliderItemRequest request)
    {
        string? imageUrl = null;

        if (image != null)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(image.ContentType))
                return BadRequest(new ProblemDetails { Title = "Tipo no válido", Status = 400 });

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new ProblemDetails { Title = "Archivo muy grande", Status = 400 });

            await using var stream = image.OpenReadStream();
            imageUrl = await _fileStorage.SaveAsync(stream, image.FileName, "slider");
        }

        var item = new SliderItem
        {
            Title = request.Title,
            Subtitle = request.Subtitle,
            ImageUrl = imageUrl ?? string.Empty,
            LinkUrl = request.LinkUrl,
            DisplayOrder = request.DisplayOrder,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = request.IsActive,
            CreatedByUserId = _currentUser.GetUserId()
        };

        _context.SliderItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok(new SliderItemDto
        {
            Id = item.Id,
            Title = item.Title,
            Subtitle = item.Subtitle,
            ImageUrl = item.ImageUrl,
            LinkUrl = item.LinkUrl,
            DisplayOrder = item.DisplayOrder,
            StartsAt = item.StartsAt,
            EndsAt = item.EndsAt,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt
        });
    }

    [HttpPut("slider/{id:guid}")]
    [RequirePermission("slider.update")]
    public async Task<ActionResult<SliderItemDto>> UpdateSlider(Guid id, [FromBody] UpdateSliderItemRequest request)
    {
        var item = await _context.SliderItems.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
            return NotFound();

        item.Title = request.Title;
        item.Subtitle = request.Subtitle;
        item.LinkUrl = request.LinkUrl;
        item.DisplayOrder = request.DisplayOrder;
        item.StartsAt = request.StartsAt;
        item.EndsAt = request.EndsAt;
        item.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new SliderItemDto
        {
            Id = item.Id,
            Title = item.Title,
            Subtitle = item.Subtitle,
            ImageUrl = item.ImageUrl,
            LinkUrl = item.LinkUrl,
            DisplayOrder = item.DisplayOrder,
            StartsAt = item.StartsAt,
            EndsAt = item.EndsAt,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt
        });
    }

    [HttpDelete("slider/{id:guid}")]
    [RequirePermission("slider.delete")]
    public async Task<ActionResult> DeleteSlider(Guid id)
    {
        var item = await _context.SliderItems.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null)
            return NotFound();

        if (!string.IsNullOrEmpty(item.ImageUrl))
            await _fileStorage.DeleteAsync(item.ImageUrl);

        _context.SliderItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("slider/order")]
    [RequirePermission("slider.update")]
    public async Task<ActionResult> UpdateOrder([FromBody] List<UpdateSliderOrderItem> order)
    {
        var ids = order.Select(o => o.Id).ToList();
        var items = await _context.SliderItems
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();

        foreach (var item in order)
        {
            var slider = items.FirstOrDefault(s => s.Id == item.Id);
            if (slider != null)
                slider.DisplayOrder = item.DisplayOrder;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
