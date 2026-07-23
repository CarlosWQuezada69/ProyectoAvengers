# Proyecto Avengers

API REST para catálogo de productos con panel de administración. Construida con **ASP.NET Core 8**, **EF Core** y **PostgreSQL**.

## Stack

| Capa       | Tecnología                              |
|------------|-----------------------------------------|
| Runtime    | .NET 8                                  |
| ORM        | EF Core 8 + Npgsql                      |
| BD         | PostgreSQL 16                           |
| Auth       | JWT Bearer + Refresh Tokens             |
| Validación | FluentValidation 11                     |
| Tests      | xUnit + Moq                             |

## Arquitectura

```
src/
├── Api            → Controllers, Middleware, Authorization, Swagger
├── Application    → Interfaces, Validators
├── Domain         → Entidades de negocio (POCO)
├── Infrastructure → EF Core, Servicios (Token, Email, FileStorage, ViewTracker)
└── Shared         → DTOs compartidos
tests/
└── ProyectoAvengers.Tests → Pruebas unitarias (xUnit)
```

Clean Architecture con dependencias hacia adentro: `Api → Application → Domain` y `Infrastructure → Application`.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 16 (o `docker compose up -d`)

## Inicio rápido

```bash
# 1. Levantar PostgreSQL
docker compose up -d postgres

# 2. Configurar variables de entorno (opcional)
export ADMIN_EMAIL=admin@example.com
export ADMIN_PASSWORD=Admin123!

# 3. Aplicar migraciones
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# 4. Ejecutar
dotnet run --project src/Api
```

El seed crea automáticamente permisos, el rol **SuperAdmin** y un usuario admin (desde `ADMIN_EMAIL`/`ADMIN_PASSWORD`).

## Docker (todo incluido)

```bash
docker compose up --build
```

- API en `http://localhost:5000`
- Swagger en `http://localhost:5000/swagger`
- PostgreSQL en `localhost:5432`

## API

Endpoints públicos y administrativos. Documentación interactiva en `/swagger`.

| Prefixo                 | Auth     |
|-------------------------|----------|
| `/api/v1/auth/*`        | Público  |
| `/api/v1/account/*`     | Usuario  |
| `/api/v1/categories`    | Público  |
| `/api/v1/products`      | Público  |
| `/api/v1/slider`        | Público  |
| `/api/v1/settings/public` | Público |
| `/api/v1/admin/*`       | Admin (permisos) |

### Endpoints destacados

| Método | Ruta                                  | Permiso          |
|--------|---------------------------------------|------------------|
| POST   | `/auth/login`                         | —                |
| POST   | `/auth/refresh-token`                 | —                |
| POST   | `/auth/forgot-password`               | —                |
| POST   | `/auth/reset-password`                | —                |
| GET    | `/categories?tree=true`               | —                |
| GET    | `/products?search=&categoryId=&...`   | —                |
| POST   | `/products/{id}/track-view`           | —                |
| GET    | `/settings/public`                    | —                |
| GET    | `/admin/users`                        | `users.view`     |
| POST   | `/admin/products`                     | `products.create`|
| GET    | `/admin/stats/overview`               | `stats.view`     |
| GET    | `/admin/audit-logs`                   | `audit.view`     |

## Seed inicial

- **25 permisos** (catalogados por módulo: products, categories, users, roles, slider, settings, stats, audit)
- **Rol SuperAdmin** con todos los permisos
- **Usuario admin** desde variables de entorno (omite seed si no están definidas)

## Licencia

MIT
