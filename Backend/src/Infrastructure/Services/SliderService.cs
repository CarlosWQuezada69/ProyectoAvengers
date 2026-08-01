using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class SliderService : ISliderService
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public SliderService(AppDbContext context, IFileStorage fileStorage, ICurrentUserService currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<List<SliderItemDto>> ListAsync()
    {
        var items = await _context.SliderItems
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<SliderItemDto> CreateAsync(Stream? imageStream, string? imageFileName, string? imageContentType, CreateSliderItemRequest request)
    {
        string? imageUrl = null;

        if (imageStream != null && imageFileName != null && imageContentType != null)
        {
            var allowedTypes = Constants.AllowedImageMimeTypes;
            if (!allowedTypes.Contains(imageContentType))
                throw new InvalidOperationException("Tipo de imagen no válido.");

            if (imageStream.Length > Constants.MaxImageSizeBytes)
                throw new InvalidOperationException("La imagen supera el tamaño máximo permitido.");

            imageUrl = await _fileStorage.SaveAsync(imageStream, imageFileName, "slider");
        }

        var item = new SliderItem(request.Title, request.Subtitle, imageUrl ?? string.Empty,
            request.LinkUrl, request.DisplayOrder, request.StartsAt,
            request.EndsAt, request.IsActive, _currentUser.GetUserId());

        _context.SliderItems.Add(item);
        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<SliderItemDto?> UpdateAsync(Guid id, UpdateSliderItemRequest request)
    {
        var item = await _context.SliderItems.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return null;

        item.UpdateDetails(request.Title, request.Subtitle, request.LinkUrl,
            request.DisplayOrder, request.StartsAt, request.EndsAt, request.IsActive);

        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _context.SliderItems.FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return false;

        if (!string.IsNullOrEmpty(item.ImageUrl))
            await _fileStorage.DeleteAsync(item.ImageUrl);

        _context.SliderItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateOrderAsync(List<UpdateSliderOrderItem> order)
    {
        var ids = order.Select(o => o.Id).ToList();
        var items = await _context.SliderItems
            .Where(s => ids.Contains(s.Id))
            .ToListAsync();

        if (items.Count == 0) return false;

        foreach (var item in order)
        {
            var slider = items.FirstOrDefault(s => s.Id == item.Id);
            if (slider != null)
                slider.UpdateOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static SliderItemDto MapToDto(SliderItem item) => new()
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
    };
}
