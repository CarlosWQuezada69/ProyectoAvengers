using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Application.Interfaces;

public interface IAuditService
{
    Task<PaginatedResponse<AuditLogDto>> GetAuditLogsAsync(Guid? userId, string? entityName, DateTime? from,
        DateTime? to, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
