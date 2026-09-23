# Proyecto Avengers — The Avengers Joyero

Sistema full-stack de joyería con **tienda de clientes** y **panel de administración**. Catálogo de productos con hero de slider, carrito-cotización imprimible, estadísticas de vistas, roles jerárquicos y auditoría. Todo en español con formato de fecha de República Dominicana (`es-DO`) y moneda DOP.

## Stack

| Capa | Tecnología |
|------|------------|
| Backend | ASP.NET Core 8 — API REST, Clean Architecture, EF Core + PostgreSQL, JWT Bearer, FluentValidation |
| Panel admin | Angular 22 — standalone components, Signals, modo zoneless, `app-config` con `LOCALE_ID es-DO` |
| Tienda clientes | Angular 22 — standalone components, Signals, View Transitions API, diseño oscuro premium con acentos dorados |

## Funcionalidades

**Panel administrativo** (`:4200`)
- Autenticación JWT + refresh tokens, roles con jerarquía de nivel y permisos por acción (`RequirePermission`).
- Gestión de usuarios, roles/perfiles de permiso, productos (imágenes, restricciones, compare-at price), categorías, slider/carrusel con imágenes, página "Acerca de", ajustes del sitio (logo, RNC, dirección, contactos, SEO) y auditoría.
- Dashboard con KPIs y estadísticas de vistas (gráfico donut y de barras, formato RD).
- Animaciones fluidas (revelar al hacer scroll, cascadas con `stagger`, fade de ruta).

**Tienda de clientes** (`:4201`)
- Página de inicio con hero de slider (imágenes, textos y 8 s de lectura), colecciones destacadas y fallbacks cuando no hay datos.
- Catálogo con búsqueda, filtro por categoría, disponibilidad, ordenación y paginación.
- Detalle de producto con galería, stock, cantidad y restricciones.
- **Carrito como cotización**: imprimible en PDF (logo, negocio, RNC, dirección, N.º de cotización, tabla, subtotal y nota "sin valor de pago") y contacto por WhatsApp.
- Footer con enlace discreto al panel (se oculta si `adminUrl` está vacío).
- Tracking de vistas de página (`POST /api/v1/analytics/page-view`).

**Backend**
- Seeder robusto de permisos/rol SuperAdmin/usuario admin y **seeder de datos demo solo en `Development`** (catálogo, ajustes, slides) idempotente.
- 28 tests xUnit (unitarios + integración con `WebApplicationFactory` + EF Core InMemory).
- Validación unificada de archivos de imagen y rate limiting.

## Estructura

```
ProyectoAvengers/
├── Backend/
│   ├── src/
│   │   ├── Api/                 → Controllers, middleware, autorización
│   │   ├── Application/         → Interfaces de servicios, validadores
│   │   ├── Domain/              → Entidades
│   │   ├── Infrastructure/      → EF Core, migraciones, servicios, seeding
│   │   └── Shared/              → DTOs, permisos, constantes
│   └── tests/                   → xUnit
└── Frontend/
    ├── Panel-administrativo/    → SPA admin (Angular 22)
    └── Vista-clientes/          → Tienda pública (Angular 22)
```

## Puesta en marcha (desarrollo)

Requisitos: .NET 8 SDK, Node.js 20+, PostgreSQL 16.

```bash
# 1) Base de datos (PostgreSQL local o Docker)
docker compose -f Backend/docker-compose.yml up -d postgres

# 2) API (puerto 5167)
cd Backend/src/Api
export JWT_SECRET="<secreto-de-32-caracteres>"
export ADMIN_EMAIL=admin@example.com
export ADMIN_PASSWORD=Admin123!
export ASPNETCORE_ENVIRONMENT=Development
dotnet run

# 3) Panel admin (puerto 4200)
cd Frontend/Panel-administrativo
npm install && npx ng serve

# 4) Tienda clientes (puerto 4201)
cd Frontend/Vista-clientes
npm install && npx ng serve --port 4201
```

| Servicio | URL |
|----------|-----|
| API | http://localhost:5167 |
| Swagger | http://localhost:5167/swagger |
| Panel admin | http://localhost:4200 |
| Tienda | http://localhost:4201 |
| PostgreSQL | localhost:5432 |

Acceso admin: `admin@example.com` / `Admin123!`

## Notas
- La API lee `JWT_SECRET` de variable de entorno o User Secrets (mínimo 32 caracteres), no está en `appsettings.json`.
- El CHANGELOG mantiene el historial de sesiones por fecha.
- Detalles técnicos del backend en [`Backend/README.md`](Backend/README.md).