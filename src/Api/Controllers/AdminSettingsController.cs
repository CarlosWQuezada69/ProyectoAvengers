using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

public class AdminSettingsController : AdminBaseController
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public AdminSettingsController(AppDbContext context, IFileStorage fileStorage, ICurrentUserService currentUser)
    {
        _context = context;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    [HttpGet("settings")]
    [RequirePermission("settings.view")]
    public async Task<ActionResult<List<SiteSettingDto>>> GetSettings()
    {
        var settings = await _context.SiteSettings
            .OrderBy(s => s.Key)
            .ToListAsync();

        return Ok(settings.Select(s => new SiteSettingDto
        {
            Key = s.Key,
            Value = s.Value,
            UpdatedAt = s.UpdatedAt
        }).ToList());
    }

    [HttpPut("settings/{key}")]
    [RequirePermission("settings.update")]
    public async Task<ActionResult<SiteSettingDto>> UpdateSetting(string key, [FromBody] UpdateSiteSettingRequest request)
    {
        var setting = await _context.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new Domain.Entities.SiteSetting
            {
                Key = key,
                Value = request.Value,
                UpdatedByUserId = _currentUser.GetUserId(),
                UpdatedAt = DateTime.UtcNow
            };
            _context.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = request.Value;
            setting.UpdatedByUserId = _currentUser.GetUserId();
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new SiteSettingDto
        {
            Key = setting.Key,
            Value = setting.Value,
            UpdatedAt = setting.UpdatedAt
        });
    }

    [HttpPost("settings/logo")]
    [RequirePermission("settings.update")]
    public async Task<ActionResult<SiteSettingDto>> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Archivo vacío", Status = 400 });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new ProblemDetails { Title = "Tipo no válido", Status = 400 });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new ProblemDetails { Title = "Archivo muy grande", Status = 400, Detail = "El tamaño máximo es 2 MB." });

        await using var stream = file.OpenReadStream();
        var url = await _fileStorage.SaveAsync(stream, file.FileName, "logo");

        var setting = await _context.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == "logo_url");

        if (setting == null)
        {
            setting = new Domain.Entities.SiteSetting
            {
                Key = "logo_url",
                Value = url,
                UpdatedByUserId = _currentUser.GetUserId(),
                UpdatedAt = DateTime.UtcNow
            };
            _context.SiteSettings.Add(setting);
        }
        else
        {
            if (!string.IsNullOrEmpty(setting.Value))
                await _fileStorage.DeleteAsync(setting.Value);

            setting.Value = url;
            setting.UpdatedByUserId = _currentUser.GetUserId();
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new SiteSettingDto
        {
            Key = setting.Key,
            Value = setting.Value,
            UpdatedAt = setting.UpdatedAt
        });
    }
}
