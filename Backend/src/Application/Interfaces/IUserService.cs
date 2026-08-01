using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface IUserService
{
    Task<PaginatedResponse<UserDto>> ListAsync(string? search, Guid? roleId, bool? isActive, int page, int pageSize);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AssignRolesAsync(Guid id, AssignRolesRequest request);
}
