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

        var anillos = new Category(null, "Anillos", "anillos",
            "Anillos de oro, plata y piedras preciosas", null, true, 1);
        var cadenas = new Category(null, "Cadenas", "cadenas",
            "Cadenas y gargantillas en oro 18K", null, true, 2);
        var pulseras = new Category(null, "Pulseras", "pulseras",
            "Pulseras y brazaletes de diseño exclusivo", null, true, 3);
        var collares = new Category(null, "Collares", "collares",
            "Collares y gargantillas de oro y perlas", null, true, 4);
        var pendientes = new Category(null, "Pendientes", "pendientes",
            "Pendientes y aros para toda ocasión", null, true, 5);

        _context.Categories.AddRange(anillos, cadenas, pulseras, collares, pendientes);

        var productos = new (string Sku, string Name, string Slug, string Description,
            decimal Price, decimal? CompareAtPrice, int Stock, Category Category, bool Featured,
            string[] Images)[]
        {
            ("JOY-ANI-001", "Anillo de Oro 18K Cintillo", "anillo-oro-18k-cintillo",
                "Cintillo clásico de oro 18 quilates con acabado pulido espejo. Peso aproximado de 6.5 g y cierre de comfort. Cada pieza incluye estuche de la casa y certificado de autenticidad.",
                28500, 34000, 8, anillos, true, new[] { "joya-02.jpg", "joya-13.jpg" }),
            ("JOY-ANI-002", "Anillo de Compromiso Solitario", "anillo-compromiso-solitario",
                "Solitario de oro blanco 18K con diamante central de 0.50 quilates y tallado brillante. Montura de seis garras y banda comfort de 1.8 mm.",
                145000, null, 3, anillos, true, new[] { "joya-19.jpg" }),
            ("JOY-ANI-003", "Anillo de Plata Esterlina con Circonia", "anillo-plata-estrelina-circonia",
                "Anillo de plata 925 con circonia de grado A. Hipoalergénico y libre de niquel, ideal para uso diario.",
                6750, 8500, 24, anillos, false, new[] { "joya-16.jpg" }),
            ("JOY-ANI-004", "Anillo de Oro 18K con Piedra Central", "anillo-oro-18k-piedra-central",
                "Anillo de oro 18K con topacio azul en bruto y engravado pulido. Acabado artesanal con sello de la casa en el aro.",
                34500, 42000, 5, anillos, false, new[] { "joya-12.jpg" }),
            ("JOY-CAD-001", "Cadena Veneziana Oro 18K 50cm", "cadena-veneziana-oro-18k-50cm",
                "Cadena veneziana de oro 18K de 50 cm con eslabones planos pulidos y cierre con pasador de seguridad. La pieza más versátil de la colección.",
                42000, 49500, 10, cadenas, true, new[] { "joya-10.jpg", "joya-01.jpg" }),
            ("JOY-CAD-002", "Cadena Cuban 8mm Oro 18K", "cadena-cuban-8mm-oro-18k",
                "Cadena tipo Cuban de 8 mm en oro 18K, con eslabones macizos de caja abierta. Alto brillo y peso para piezas con presencia.",
                88000, null, 4, cadenas, true, new[] { "joya-18.jpg" }),
            ("JOY-CAD-003", "Cadena de Acero Inoxidable 60cm", "cadena-acero-inoxidable-60cm",
                "Cadena de acero inoxidable 316L con baño de oro 18K. Resistente al agua y al uso diario sin perder color.",
                3200, 4500, 40, cadenas, false, new[] { "joya-17.jpg" }),
            ("JOY-PUL-001", "Pulsera Tennis Oro 18K", "pulsera-tennis-oro-18k",
                "Pulsera tennis de oro 18K con 180 piedras de 1.5 mm engastadas a mano. Cierre de doble seguridad.",
                65000, 78000, 6, pulseras, true, new[] { "joya-08.jpg", "joya-09.jpg" }),
            ("JOY-PUL-002", "Pulsera Milanesa Acero", "pulsera-milanesa-acero",
                "Pulsera milanesa de acero inoxidable con eslabones de configuración mixta. Cierre regulable de 16 a 21 cm.",
                4500, 5900, 30, pulseras, false, new[] { "joya-15.jpg" }),
            ("JOY-PUL-003", "Pulsera de Cuero con Placa Dorada", "pulsera-cuero-placa-dorada",
                "Pulsera de cuero genuino con placa dorada 18K grabada a mano. Cada pieza es única por las vetas naturales del cuero.",
                5900, null, 18, pulseras, false, new[] { "joya-14.jpg" }),
            ("JOY-COL-001", "Collar de Perla Cultivada", "collar-perla-cultivada",
                "Collar de perlas cultivadas de agua dulce con cierre dorado 18K y largo de 45 cm. Lustre natural y superficie irregular única por pieza.",
                32000, 38500, 7, collares, true, new[] { "joya-03.jpg", "joya-11.jpg" }),
            ("JOY-COL-002", "Collar Cruz de Oro 18K", "collar-cruz-oro-18k",
                "Collar con cruz de oro 18K de 3 cm sobre cadena fina de 50 cm. Detalle de micrograbado artesanal en el reverso.",
                24500, 29900, 12, collares, true, new[] { "joya-04.jpg" }),
            ("JOY-COL-003", "Gargantilla Oro 18K 45cm", "gargantilla-oro-18k-45cm",
                "Gargantilla de oro 18K con cadena fina y eslabón central. Acabado pulido y cómodo para uso diario.",
                21000, null, 15, collares, false, new[] { "joya-05.jpg" }),
            ("JOY-PEN-001", "Pendientes de Oro 18K con Circonia", "pendientes-oro-18k-circonia",
                "Pendientes de oro 18K con circonias de grado A en montura de cuatro garras. Livianos y cómodos para uso diario.",
                19500, null, 20, pendientes, true, new[] { "joya-06.jpg", "joya-07.jpg" }),
            ("JOY-PEN-002", "Aros de Oro 18K 15mm", "aros-oro-18k-15mm",
                "Aros de oro 18K de 15 mm de diámetro con cierre de resorte reforzado. La versión clásica para el día a día.",
                16900, 21500, 14, pendientes, false, new[] { "joya-20.jpg" })
        };

        foreach (var p in productos)
        {
            var product = new Product(p.Sku, p.Name, p.Slug, p.Description, p.Price,
                p.CompareAtPrice, p.Stock, p.Category.Id, true, p.Featured, null);

            for (var i = 0; i < p.Images.Length; i++)
                product.ProductImages.Add(new ProductImage(
                    product.Id, $"/uploads/products/{p.Images[i]}", p.Name, i, i == 0));

            _context.Products.Add(product);
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedDemoSettingsAsync(CancellationToken ct)
    {
        if (await _context.SiteSettings.AnyAsync(ct))
            return;

        var defaults = new (string Key, string Value)[]
        {
            ("business_name", "THE AVENGERS JOYERIA"),
            ("copyright_text", "© 2026 THE AVENGERS JOYERIA. Todos los derechos reservados."),
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
            new SliderItem("Anillos de Oro 18 Quilates", "Oro 18 quilates:", "", "/productos", 0, null, null, true, null),
            new SliderItem("Cadenas y Colgantes de Plata", "Plata 925 esterlina:", "", "/productos", 1, null, null, true, null),
            new SliderItem("Diamantes Eternos", "Certificado de calidad:", "", "/productos", 2, null, null, true, null));

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
