using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Application.Mapping;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Infrastructure.Validation;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class AboutService : IAboutService
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;

    public AboutService(AppDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<AboutInfoDto> GetAboutAsync(CancellationToken ct = default)
    {
        var about = await _context.AboutInfos
            .AsNoTracking()
            .Include(a => a.Galleries.OrderBy(g => g.DisplayOrder))
            .FirstOrDefaultAsync(ct);

        return about == null ? new AboutInfoDto() : about.ToDto();
    }

    public async Task<AboutInfoDto> UpdateAboutAsync(UpdateAboutInfoRequest request, CancellationToken ct = default)
    {
        var about = await _context.AboutInfos
            .AsTracking()
            .Include(a => a.Galleries)
            .FirstOrDefaultAsync(ct);

        if (about == null)
        {
            about = new AboutInfo(request.Title, request.History, request.Mission, request.Vision);
            _context.AboutInfos.Add(about);
        }
        else
        {
            about.UpdateDetails(request.Title, request.History, request.Mission, request.Vision);
        }

        await _context.SaveChangesAsync(ct);

        return about.ToDto();
    }

    public async Task<AboutGalleryDto> UploadImageAsync(string section, FileUpload file, CancellationToken ct = default)
    {
        var about = await _context.AboutInfos
            .Include(a => a.Galleries)
            .FirstOrDefaultAsync(ct);

        if (about == null)
        {
            about = new AboutInfo(string.Empty, string.Empty, null, null);
            _context.AboutInfos.Add(about);
            await _context.SaveChangesAsync(ct);
        }

        if (!ImageFileValidator.IsValid(file.ContentType, file.Length, out var error))
            throw new InvalidOperationException(error ?? "Archivo no válido");

        var folder = section switch
        {
            "founder" => "about/founder",
            "employees" => "about/employees",
            "location" => "about/location",
            _ => "about"
        };

        await using var stream = file.Content;
        var url = await _fileStorage.SaveAsync(stream, file.FileName, folder);

        var image = about.AddImage(url, file.FileName, about.Galleries.Count, section);
        _context.AboutGalleries.Add(image);
        await _context.SaveChangesAsync(ct);

        return new AboutGalleryDto
        {
            Id = image.Id,
            Url = image.Url,
            AltText = image.AltText,
            DisplayOrder = image.DisplayOrder,
            Section = image.Section
        };
    }

    public async Task<bool> DeleteImageAsync(Guid id, CancellationToken ct = default)
    {
        var image = await _context.AboutGalleries
            .AsTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (image == null)
            return false;

        await _fileStorage.DeleteAsync(image.Url);
        _context.AboutGalleries.Remove(image);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task UpdateOrderAsync(List<UpdateGalleryOrderItem> order, CancellationToken ct = default)
    {
        var ids = order.Select(o => o.Id).ToList();
        var images = await _context.AboutGalleries
            .AsTracking()
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);

        foreach (var item in order)
        {
            var image = images.FirstOrDefault(i => i.Id == item.Id);
            if (image != null)
                image.UpdateOrder(item.DisplayOrder);
        }

        await _context.SaveChangesAsync(ct);
    }
}
