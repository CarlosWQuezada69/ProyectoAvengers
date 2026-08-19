using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Application.Mapping;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private async Task<int> GetCurrentUserLevelAsync()
    {
        var userId = _currentUser.GetUserId();
        if (!userId.HasValue) return 0;

        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId.Value)
            .Select(ur => ur.Role.HierarchyLevel)
            .OrderByDescending(l => l)
            .FirstOrDefaultAsync();
    }

    private static bool CanAccessUser(int currentLevel, int targetLevel) => targetLevel <= currentLevel;

    public async Task<PaginatedResponse<UserDto>> ListAsync(string? search, Guid? roleId, bool? isActive, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var currentLevel = await GetCurrentUserLevelAsync();

        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .Where(u => u.UserRoles.All(ur => ur.Role.HierarchyLevel <= currentLevel))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));

        if (roleId.HasValue)
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId.Value));

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<UserDto>
        {
            Data = items.Select(u => u.ToDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var currentLevel = await GetCurrentUserLevelAsync();

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        if (user == null) return null;
        if (!CanAccessUser(currentLevel, user.UserRoles.Max(ur => ur.Role.HierarchyLevel))) return null;

        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var currentLevel = await GetCurrentUserLevelAsync();

        if (request.RoleIds?.Count > 0)
        {
            var hasHigherRole = await _context.Roles
                .AsNoTracking()
                .AnyAsync(r => request.RoleIds.Contains(r.Id) && r.HierarchyLevel > currentLevel);

            if (hasHigherRole)
                throw new InvalidOperationException("No tienes permiso para asignar roles superiores al tuyo.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("El correo electrónico ya está registrado.");

        var user = new User(request.FirstName, request.LastName, request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password), request.Phone);

        if (request.RoleIds?.Count > 0)
            user.AssignRoles(request.RoleIds);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(user.Id))!;
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var currentLevel = await GetCurrentUserLevelAsync();

        var user = await _context.Users
            .AsTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        if (user == null) return null;
        if (!CanAccessUser(currentLevel, user.UserRoles.Max(ur => ur.Role.HierarchyLevel))) return null;

        user.UpdateDetails(request.FirstName, request.LastName, request.Phone, request.IsActive);

        await _context.SaveChangesAsync();
        return user.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var currentLevel = await GetCurrentUserLevelAsync();

        var user = await _context.Users
            .AsTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        if (user == null) return false;
        if (!CanAccessUser(currentLevel, user.UserRoles.Max(ur => ur.Role.HierarchyLevel))) return false;

        user.SoftDelete();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRolesAsync(Guid id, AssignRolesRequest request)
    {
        var currentLevel = await GetCurrentUserLevelAsync();

        var user = await _context.Users
            .AsTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);

        if (user == null) return false;
        if (!CanAccessUser(currentLevel, user.UserRoles.Max(ur => ur.Role.HierarchyLevel))) return false;

        if (request.RoleIds.Count > 0)
        {
            var hasHigherRole = await _context.Roles
                .AsNoTracking()
                .AnyAsync(r => request.RoleIds.Contains(r.Id) && r.HierarchyLevel > currentLevel);

            if (hasHigherRole)
                return false;
        }

        user.AssignRoles(request.RoleIds);

        await _context.SaveChangesAsync();
        return true;
    }
}
