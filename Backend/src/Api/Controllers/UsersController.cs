using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class UsersController : AdminBaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    [RequirePermission("users.view")]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] Guid? roleId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _userService.ListAsync(search, roleId, isActive, page, pageSize);
        return Ok(result);
    }

    [HttpGet("users/{id:guid}")]
    [RequirePermission("users.view")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("users")]
    [RequirePermission("users.create")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Solicitud inválida",
                Status = 400,
                Detail = ex.Message
            });
        }
    }

    [HttpPut("users/{id:guid}")]
    [RequirePermission("users.update")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpDelete("users/{id:guid}")]
    [RequirePermission("users.delete")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var deleted = await _userService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPut("users/{id:guid}/roles")]
    [RequirePermission("users.manage-roles")]
    public async Task<ActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request)
    {
        var assigned = await _userService.AssignRolesAsync(id, request);
        if (!assigned) return NotFound();
        return NoContent();
    }
}
