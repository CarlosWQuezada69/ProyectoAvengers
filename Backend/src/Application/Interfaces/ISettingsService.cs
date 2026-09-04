using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface ISettingsService
{
    Task<List<SiteSettingDto>> GetSettingsAsync(CancellationToken ct = default);
    Task<SiteSettingDto> UpdateSettingAsync(string key, UpdateSiteSettingRequest request, Guid? userId, CancellationToken ct = default);
    Task<SiteSettingDto> UploadLogoAsync(FileUpload file, Guid? userId, CancellationToken ct = default);
}
