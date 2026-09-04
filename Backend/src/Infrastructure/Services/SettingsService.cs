using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Infrastructure.Validation;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;

    public SettingsService(AppDbContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<List<SiteSettingDto>> GetSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _context.SiteSettings
            .AsNoTracking()
            .OrderBy(s => s.Key)
            .ToListAsync(ct);

        return settings.Select(s => new SiteSettingDto
        {
            Key = s.Key,
            Value = s.Value,
            UpdatedAt = s.UpdatedAt
        }).ToList();
    }

    public async Task<SiteSettingDto> UpdateSettingAsync(string key, UpdateSiteSettingRequest request,
        Guid? userId, CancellationToken ct = default)
    {
        var setting = await _context.SiteSettings
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Key == key, ct);

        if (setting == null)
        {
            setting = new SiteSetting(key, request.Value, userId);
            _context.SiteSettings.Add(setting);
        }
        else
        {
            setting.UpdateValue(request.Value, userId);
        }

        await _context.SaveChangesAsync(ct);

        return new SiteSettingDto
        {
            Key = setting.Key,
            Value = setting.Value,
            UpdatedAt = setting.UpdatedAt
        };
    }

    public async Task<SiteSettingDto> UploadLogoAsync(FileUpload file, Guid? userId, CancellationToken ct = default)
    {
        if (!file.HasFile)
            throw new InvalidOperationException("Archivo vacío");

        if (!ImageFileValidator.IsValid(file.ContentType, file.Length, out var error,
                allowedTypes: ImageFileValidator.LogoMimeTypes, maxSizeBytes: 2 * 1024 * 1024))
            throw new InvalidOperationException(error ?? "Archivo no válido");

        await using var stream = file.Content;
        var url = await _fileStorage.SaveAsync(stream, file.FileName, "logo");

        var setting = await _context.SiteSettings
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Key == "logo_url", ct);

        if (setting == null)
        {
            setting = new SiteSetting("logo_url", url, userId);
            _context.SiteSettings.Add(setting);
        }
        else
        {
            if (!string.IsNullOrEmpty(setting.Value))
                await _fileStorage.DeleteAsync(setting.Value);

            setting.UpdateValue(url, userId);
        }

        await _context.SaveChangesAsync(ct);

        return new SiteSettingDto
        {
            Key = setting.Key,
            Value = setting.Value,
            UpdatedAt = setting.UpdatedAt
        };
    }
}
