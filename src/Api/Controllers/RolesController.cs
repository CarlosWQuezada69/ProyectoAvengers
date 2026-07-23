using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

public class RolesController : AdminBaseController
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("roles")]
    [RequirePermission("roles.view")]
    public async Task<ActionResult<List<RoleDto>>> GetRoles()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return Ok(roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            PermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList(),
            UserCount = r.UserRoles.Count
        }).ToList());
    }

    [HttpGet("permissions")]
    [RequirePermission("roles.view")]
    public async Task<ActionResult<List<PermissionDto>>> GetPermissions()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync();

        return Ok(permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Code = p.Code,
            Module = p.Module,
            Action = p.Action,
            Description = p.Description
        }).ToList());
    }

    [HttpPost("roles")]
    [RequirePermission("roles.create")]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == request.Name))
            return Conflict(new ProblemDetails
            {
                Title = "Rol duplicado",
                Status = 409,
                Detail = "Ya existe un rol con ese nombre."
            });

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        if (request.PermissionIds?.Count > 0)
        {
            role.RolePermissions = request.PermissionIds.Select(pid => new RolePermission
            {
                PermissionId = pid
            }).ToList();
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return Ok(new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            PermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList()
        });
    }

    [HttpPut("roles/{id:guid}")]
    [RequirePermission("roles.update")]
    public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return NotFound();

        if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.Id != id))
            return Conflict(new ProblemDetails
            {
                Title = "Rol duplicado",
                Status = 409,
                Detail = "Ya existe otro rol con ese nombre."
            });

        role.Name = request.Name;
        role.Description = request.Description;

        await _context.SaveChangesAsync();

        return Ok(new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        });
    }

    [HttpDelete("roles/{id:guid}")]
    [RequirePermission("roles.delete")]
    public async Task<ActionResult> DeleteRole(Guid id)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound();

        if (role.UserRoles.Count > 0)
            return Conflict(new ProblemDetails
            {
                Title = "Rol en uso",
                Status = 409,
                Detail = "No se puede eliminar el rol porque tiene usuarios asignados."
            });

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("roles/{id:guid}/permissions")]
    [RequirePermission("roles.update")]
    public async Task<ActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return NotFound();

        _context.RolePermissions.RemoveRange(role.RolePermissions);

        role.RolePermissions = request.PermissionIds.Select(pid => new RolePermission
        {
            RoleId = id,
            PermissionId = pid
        }).ToList();

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
