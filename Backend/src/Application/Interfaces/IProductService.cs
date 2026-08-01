using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface IProductService
{
    Task<PaginatedResponse<ProductListDto>> ListAsync(string? search, Guid? categoryId, bool? isActive, int page, int pageSize);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<ProductImageDto?> UploadImageAsync(Guid productId, Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteImageAsync(Guid productId, Guid imageId);
    Task<bool> UpdateImageOrderAsync(Guid productId, List<UpdateImageOrderItem> order);
    Task<ProductRestrictionDto?> CreateRestrictionAsync(Guid productId, CreateRestrictionRequest request);
    Task<ProductRestrictionDto?> UpdateRestrictionAsync(Guid productId, Guid restrictionId, UpdateRestrictionRequest request);
    Task<bool> DeleteRestrictionAsync(Guid productId, Guid restrictionId);
}
