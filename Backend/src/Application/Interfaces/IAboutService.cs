using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface IAboutService
{
    Task<AboutInfoDto> GetAboutAsync(CancellationToken ct = default);
    Task<AboutInfoDto> UpdateAboutAsync(UpdateAboutInfoRequest request, CancellationToken ct = default);
    Task<AboutGalleryDto> UploadImageAsync(string section, FileUpload file, CancellationToken ct = default);
    Task<bool> DeleteImageAsync(Guid id, CancellationToken ct = default);
    Task UpdateOrderAsync(List<UpdateGalleryOrderItem> order, CancellationToken ct = default);
}
