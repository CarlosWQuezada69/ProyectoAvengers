using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailChangeRequest> EmailChangeRequests => Set<EmailChangeRequest>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductRestriction> ProductRestrictions => Set<ProductRestriction>();
    public DbSet<ProductStatsDaily> ProductStatsDailies => Set<ProductStatsDaily>();
    public DbSet<SliderItem> SliderItems => Set<SliderItem>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<ProductRestriction>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_product_restrictions_restriction_type",
                "restriction_type IN ('AGE_MIN', 'PURCHASE_LIMIT_USER', 'PURCHASE_LIMIT_ORDER', 'AVAILABILITY_WINDOW', 'GEOGRAPHIC', 'LIMITED_STOCK')"
            ));
        });

        modelBuilder.Entity<Product>().HasQueryFilter(p => p.DeletedAt == null);
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserId();
        var ipAddress = _currentUserService.GetIpAddress();
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog)
                continue;

            var entity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    SetProperty(entity, "CreatedAt", now);
                    SetProperty(entity, "CreatedByUserId", userId);
                    break;

                case EntityState.Modified:
                    SetProperty(entity, "UpdatedAt", now);
                    SetProperty(entity, "UpdatedByUserId", userId);
                    break;

                case EntityState.Deleted:
                    if (HasProperty(entity, "DeletedAt"))
                    {
                        entry.State = EntityState.Modified;
                        SetProperty(entity, "DeletedAt", now);
                        SetProperty(entity, "DeletedByUserId", userId);
                    }
                    break;
            }
        }

        var auditLogs = GenerateAuditLogs(userId, ipAddress, now);
        if (auditLogs.Count > 0)
            AuditLogs.AddRange(auditLogs);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> GenerateAuditLogs(Guid? userId, string? ipAddress, DateTime now)
    {
        var logs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog)
                continue;

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "CREATE",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };

            if (action == null) continue;

            logs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKey(entry),
                Changes = entry.State != EntityState.Added ? SerializeChanges(entry) : null,
                IpAddress = ipAddress,
                CreatedAt = now
            });
        }

        return logs;
    }

    private static void SetProperty(object entity, string propertyName, object? value)
    {
        var prop = entity.GetType().GetProperty(propertyName);
        if (prop != null && prop.CanWrite)
            prop.SetValue(entity, value);
    }

    private static bool HasProperty(object entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName) != null;
    }

    private static Guid? GetPrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key == null) return null;

        var keyValue = key.Properties
            .Select(p => entry.Property(p.Name).CurrentValue)
            .FirstOrDefault();

        return keyValue is Guid g ? g : null;
    }

    private static string? SerializeChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var prop in entry.Properties)
        {
            if (!prop.IsModified)
                continue;

            if (prop.Metadata.Name is "RowVersion" or "ConcurrencyStamp" or "UpdatedAt" or "UpdatedByUserId")
                continue;

            if (prop.Metadata.GetColumnType() is "text" or "bytea")
            {
                changes[prop.Metadata.Name] = "[truncated]";
                continue;
            }

            changes[prop.Metadata.Name] = prop.CurrentValue;
        }

        return changes.Count > 0 ? JsonSerializer.Serialize(changes) : null;
    }
}
