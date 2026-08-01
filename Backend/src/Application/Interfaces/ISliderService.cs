using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface ISliderService
{
    Task<List<SliderItemDto>> ListAsync();
    Task<SliderItemDto> CreateAsync(Stream? imageStream, string? imageFileName, string? imageContentType, CreateSliderItemRequest request);
    Task<SliderItemDto?> UpdateAsync(Guid id, UpdateSliderItemRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateOrderAsync(List<UpdateSliderOrderItem> order);
}
