namespace ProyectoAvengers.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? Changes { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User? User { get; private set; }

    private AuditLog() { }

    public AuditLog(Guid? userId, string action, string entityName, Guid? entityId,
        string? changes, string? ipAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Changes = changes;
        IpAddress = ipAddress;
        CreatedAt = DateTime.UtcNow;
    }
}
