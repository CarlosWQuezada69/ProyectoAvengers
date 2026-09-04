using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class AdminSettingsController : AdminBaseController
{
    private readonly ISettingsService _settingsService;
    private readonly ICurrentUserService _currentUser;

    public AdminSettingsController(ISettingsService settingsService, ICurrentUserService currentUser)
    {
        _settingsService = settingsService;
        _currentUser = currentUser;
    }

    [HttpGet("settings")]
    [RequirePermission("settings.view")]
    public async Task<ActionResult<List<SiteSettingDto>>> GetSettings()
        => Ok(await _settingsService.GetSettingsAsync());

    [HttpPut("settings/{key}")]
    [RequirePermission("settings.update")]
    public async Task<ActionResult<SiteSettingDto>> UpdateSetting(string key, [FromBody] UpdateSiteSettingRequest request)
        => Ok(await _settingsService.UpdateSettingAsync(key, request, _currentUser.GetUserId()));

    [HttpPost("settings/logo")]
    [RequirePermission("settings.update")]
    public async Task<ActionResult<SiteSettingDto>> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Archivo vacío", Status = 400 });

        var upload = new FileUpload
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };

        try
        {
            var dto = await _settingsService.UploadLogoAsync(upload, _currentUser.GetUserId());
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Archivo no válido",
                Status = 400,
                Detail = ex.Message
            });
        }
    }
}
