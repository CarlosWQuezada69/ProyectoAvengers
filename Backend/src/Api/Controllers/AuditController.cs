using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class AuditController : AdminBaseController
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("audit-logs")]
    [RequirePermission("audit.view")]
    public async Task<ActionResult<PaginatedResponse<AuditLogDto>>> GetAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? entityName,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _auditService.GetAuditLogsAsync(userId, entityName, from, to, page, pageSize));
}
