using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Domain.Entities;
using ProyectoAvengers.Infrastructure.Persistence;

namespace ProyectoAvengers.Infrastructure.Seed;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedPermissionsAsync(cancellationToken);
        await SeedSuperAdminRoleAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existingCodes = (await _context.Permissions
            .AsNoTracking()
            .Select(p => p.Code)
            .ToListAsync(ct)).ToHashSet();

        if (existingCodes.Count > 0)
        {
            var missing = _permissions.Where(p => !existingCodes.Contains(p.Code)).ToList();
            if (missing.Count > 0)
            {
                _context.Permissions.AddRange(missing);
                await _context.SaveChangesAsync(ct);
            }
            return;
        }

        _context.Permissions.AddRange(_permissions);
        await _context.SaveChangesAsync(ct);
    }

    private static readonly List<Permission> _permissions =
    [
        new() { Code = "products.view",               Module = "products",    Action = "view",   Description = "Ver productos en el panel admin" },
            new() { Code = "products.create",             Module = "products",    Action = "create", Description = "Crear productos" },
            new() { Code = "products.update",             Module = "products",    Action = "update", Description = "Editar productos" },
            new() { Code = "products.delete",             Module = "products",    Action = "delete", Description = "Eliminar productos" },
            new() { Code = "products.manage-restrictions",Module = "products",    Action = "manage", Description = "Gestionar restricciones de producto" },
            new() { Code = "categories.view",               Module = "categories",  Action = "view",   Description = "Ver categorías en el panel admin" },
            new() { Code = "categories.create",            Module = "categories", Action = "create", Description = "Crear categorías" },
            new() { Code = "categories.update",            Module = "categories", Action = "update", Description = "Editar categorías" },
            new() { Code = "categories.delete",            Module = "categories", Action = "delete", Description = "Eliminar categorías" },
            new() { Code = "slider.view",                 Module = "slider",      Action = "view",   Description = "Ver ítems del carrusel" },
            new() { Code = "slider.create",               Module = "slider",      Action = "create", Description = "Crear ítems del carrusel" },
            new() { Code = "slider.update",               Module = "slider",      Action = "update", Description = "Editar ítems del carrusel" },
            new() { Code = "slider.delete",               Module = "slider",      Action = "delete", Description = "Eliminar ítems del carrusel" },
            new() { Code = "settings.view",               Module = "settings",    Action = "view",   Description = "Ver configuración del sitio" },
            new() { Code = "settings.update",             Module = "settings",    Action = "update", Description = "Editar configuración del sitio" },
            new() { Code = "users.view",                  Module = "users",       Action = "view",   Description = "Ver usuarios" },
            new() { Code = "users.create",                Module = "users",       Action = "create", Description = "Crear usuarios" },
            new() { Code = "users.update",                Module = "users",       Action = "update", Description = "Editar usuarios" },
            new() { Code = "users.delete",                Module = "users",       Action = "delete", Description = "Desactivar usuarios" },
            new() { Code = "users.manage-roles",          Module = "users",       Action = "manage", Description = "Asignar roles a usuarios" },
            new() { Code = "roles.view",                  Module = "roles",       Action = "view",   Description = "Ver roles y permisos" },
            new() { Code = "roles.create",                Module = "roles",       Action = "create", Description = "Crear roles" },
            new() { Code = "roles.update",                Module = "roles",       Action = "update", Description = "Editar roles y sus permisos" },
            new() { Code = "roles.delete",                Module = "roles",       Action = "delete", Description = "Eliminar roles" },
            new() { Code = "stats.view",                  Module = "stats",       Action = "view",   Description = "Ver estadísticas" },
            new() { Code = "audit.view",                  Module = "audit",       Action = "view",   Description = "Ver bitácora de auditoría" },
            new() { Code = "about.view",                  Module = "about",       Action = "view",   Description = "Ver información de la empresa" },
            new() { Code = "about.update",                Module = "about",       Action = "update", Description = "Editar información y galería de la empresa" },
    ];

    private async Task SeedSuperAdminRoleAsync(CancellationToken ct)
    {
        var superAdmin = await _context.Roles
            .AsTracking()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin", ct);

        if (superAdmin != null)
        {
            var changed = false;

            if (superAdmin.HierarchyLevel != 100)
            {
                superAdmin.SetHierarchyLevel(100);
                changed = true;
            }

            var assignedIds = superAdmin.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            var missingIds = await _context.Permissions
                .AsNoTracking()
                .Where(p => !assignedIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(ct);

            if (missingIds.Count > 0)
            {
                superAdmin.AssignPermissions(missingIds);
                changed = true;
            }

            if (changed)
                await _context.SaveChangesAsync(ct);
            return;
        }

        var role = new Role("SuperAdmin", "Acceso total al sistema", 100);

        var allPermissions = await _context.Permissions.ToListAsync(ct);
        role.AssignPermissions(allPermissions.Select(p => p.Id).ToList());

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedAdminUserAsync(CancellationToken ct)
    {
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        if (await _context.Users.AnyAsync(u => u.Email == adminEmail, ct))
            return;

        var superAdminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin", ct);

        if (superAdminRole == null)
            return;

        var user = new User("Admin", "Super", adminEmail,
            BCrypt.Net.BCrypt.HashPassword(adminPassword), null);
        user.ConfirmEmail();
        user.AssignRoles(new List<Guid> { superAdminRole.Id });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
    }
}
