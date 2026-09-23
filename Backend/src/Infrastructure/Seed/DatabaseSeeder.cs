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

    public async Task SeedDemoDataAsync(CancellationToken cancellationToken = default)
    {
        await SeedDemoCatalogAsync(cancellationToken);
        await SeedDemoSettingsAsync(cancellationToken);
        await SeedDemoSliderAsync(cancellationToken);
    }

    private async Task SeedDemoCatalogAsync(CancellationToken ct)
    {
        if (await _context.Categories.AnyAsync(ct) || await _context.Products.AnyAsync(ct))
            return;

        var cadenas = new Category(null, "Cadenas", "cadenas",
            "Cadenas de oro para lucir con estilo", null, true, 1);
        var pulseras = new Category(null, "Pulseras", "pulseras",
            "Pulseras elegantes para cualquier ocasión", null, true, 2);
        var anillos = new Category(null, "Anillos", "anillos",
            "Anillos de diseño exclusivo", null, true, 3);

        _context.Categories.AddRange(cadenas, pulseras, anillos);

        _context.Products.AddRange(
            new Product("DEMO-CAD-001", "Cadena de oro 18k", "cadena-de-oro-18k",
                "Cadena clásica de oro 18k, edición limitada para héroes.",
                18500, 21999, 12, cadenas.Id, true, true, null),
            new Product("DEMO-PUL-001", "Pulsera Reactor Arc", "pulsera-reactor-arc",
                "Inspirada en el reactor de Iron Man, acabado premium.",
                12400, null, 8, pulseras.Id, true, true, null),
            new Product("DEMO-ANI-001", "Anillo Gema del Infinito", "anillo-gema-del-infinito",
                "Con detalle de gema. Acero inoxidable y baño dorado.",
                9800, 12400, 15, anillos.Id, true, false, null),
            new Product("DEMO-CAD-002", "Cadena Mjolnir", "cadena-mjolnir",
                "Diseño inspirado en el martillo de Thor.",
                21000, 24500, 6, cadenas.Id, true, true, null),
            new Product("DEMO-PUL-002", "Pulsera Escudo", "pulsera-escudo",
                "Un tributo al escudo del Capitán América.",
                8900, null, 20, pulseras.Id, true, false, null));

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedDemoSettingsAsync(CancellationToken ct)
    {
        if (await _context.SiteSettings.AnyAsync(ct))
            return;

        var defaults = new (string Key, string Value)[]
        {
            ("business_name", "The Avengers Joyero"),
            ("copyright_text", "© 2026 The Avengers Joyero"),
            ("rnc", "1-01-00000-0"),
            ("address", "Santo Domingo, República Dominicana"),
            ("contact_email", "hola@avengersjoyero.com"),
            ("contact_phone", "+1 809 000 0000"),
            ("contact_whatsapp", "+18090000000"),
            ("seo_title", "The Avengers Joyero · Joyería de edición limitada"),
            ("seo_description", "Joyería inspirada en superhéroes. Piezas de edición limitada en República Dominicana."),
            ("seo_keywords", "joyería, edición limitada, avengers, colecciones")
        };

        _context.SiteSettings.AddRange(defaults.Select(d => new SiteSetting(d.Key, d.Value, null)));
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedDemoSliderAsync(CancellationToken ct)
    {
        if (await _context.SliderItems.AnyAsync(ct))
            return;

        _context.SliderItems.AddRange(
            new SliderItem("Joyas de Superhéroes", "Edición limitada", "", null, 0, null, null, true, null),
            new SliderItem("Nueva Colección 2026", "Elegancia y poder", "", null, 1, null, null, true, null),
            new SliderItem("Envío a todo el país", "República Dominicana", "", null, 2, null, null, true, null));

        await _context.SaveChangesAsync(ct);
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
        var allPermissionIds = await _context.Permissions
            .AsNoTracking()
            .Select(p => p.Id)
            .ToListAsync(ct);

        var superAdmin = await _context.Roles
            .AsTracking()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin", ct);

        if (superAdmin == null)
        {
            superAdmin = new Role("SuperAdmin", "Acceso total al sistema", 100);
            superAdmin.AssignPermissions(allPermissionIds);
            _context.Roles.Add(superAdmin);
            await _context.SaveChangesAsync(ct);
            return;
        }

        var changed = false;

        if (superAdmin.HierarchyLevel != 100)
        {
            superAdmin.SetHierarchyLevel(100);
            changed = true;
        }

        var assigned = superAdmin.RolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var missing = allPermissionIds.Except(assigned).ToList();

        if (missing.Count > 0)
        {
            foreach (var permissionId in missing)
                superAdmin.RolePermissions.Add(new RolePermission
                {
                    RoleId = superAdmin.Id,
                    PermissionId = permissionId
                });
            changed = true;
        }

        var extra = assigned.Except(allPermissionIds).ToList();
        if (extra.Count > 0)
        {
            var toRemove = superAdmin.RolePermissions
                .Where(rp => extra.Contains(rp.PermissionId))
                .ToList();
            foreach (var rp in toRemove)
                superAdmin.RolePermissions.Remove(rp);
            changed = true;
        }

        if (changed)
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
