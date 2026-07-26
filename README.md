# Proyecto Avengers

API REST para catálogo de productos con panel de administración, construida con **ASP.NET Core 8**, **Entity Framework Core** y **PostgreSQL 16**, siguiendo **Clean Architecture**.

---

## Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| Runtime | .NET 8 |
| ORM | EF Core 8 + Npgsql |
| Base de datos | PostgreSQL 16 |
| Autenticación | JWT Bearer + Refresh Tokens |
| Autorización | Policy-based con permisos granulares (28 permisos) |
| Validación | FluentValidation 11 |
| Testing | xUnit + Moq + EF Core InMemory |
| Infraestructura | Docker Compose |

---

## Arquitectura

```
src/
├── Api               → Controllers, Middleware, Authorization, Swagger
├── Application       → Interfaces de servicios, Validadores (FluentValidation)
├── Domain            → Entidades de negocio (POCO) — sin dependencias externas
├── Infrastructure    → EF Core DbContext, Migrations, Servicios, Seeder, Background Jobs
└── Shared            → DTOs compartidos (request/response)
tests/
└── ProyectoAvengers.Tests → Pruebas unitarias (xUnit)
```

Clean Architecture con dependencias hacia adentro: `Api → Application → Domain` e `Infrastructure → Application`.

---

## Inicio Rápido

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 16 (o `docker compose up -d postgres`)

### Desarrollo

```bash
# 1. Clonar y restaurar dependencias
git clone <repo-url>
dotnet restore

# 2. Configurar variables de entorno
export ADMIN_EMAIL=admin@example.com
export ADMIN_PASSWORD=Admin123!
export JWT_SECRET="UnaClaveSeguraDeAlMenos32Caracteres!"

# 3. Aplicar migraciones
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# 4. Ejecutar
dotnet run --project src/Api
```

El seed crea automáticamente 28 permisos, el rol **SuperAdmin** con todos los permisos y un usuario administrador desde las variables de entorno.

### Docker

```bash
docker compose up --build
```

| Servicio | URL |
|----------|-----|
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| PostgreSQL | localhost:5432 |

### Variables de Entorno

| Variable | Obligatorio | Por defecto | Descripción |
|----------|-------------|-------------|-------------|
| `DB_PASSWORD` | Sí | — | Contraseña de PostgreSQL |
| `JWT_SECRET` | Sí | — | Clave JWT (mín. 32 caracteres) |
| `ADMIN_EMAIL` | Sí | — | Email del admin inicial |
| `ADMIN_PASSWORD` | Sí | — | Contraseña del admin inicial |
| `ADMIN_EMAIL` | No | `admin@test.com` | Email del admin inicial |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | Entorno (Development activa Swagger) |

---

## API Endpoints

### Autenticación (`/api/v1/auth`)

| Método | Ruta | Auth | Rate Limit | Descripción |
|--------|------|------|------------|-------------|
| POST | `/auth/login` | — | 10/min | Iniciar sesión (JWT + refresh token) |
| POST | `/auth/refresh-token` | — | 10/min | Rotar refresh token |
| POST | `/auth/logout` | JWT | — | Revocar refresh token |
| GET | `/auth/me` | JWT | — | Perfil del usuario actual |
| POST | `/auth/forgot-password` | — | 10/min | Solicitar restablecimiento (siempre 200) |
| POST | `/auth/reset-password` | — | 10/min | Restablecer contraseña |

### Cuenta (`/api/v1/account`)

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/account/change-email/request` | JWT | Solicitar cambio de email |
| GET | `/account/change-email/confirm` | — | Confirmar cambio de email |

### Catálogo Público (`/api/v1/categories`, `/products`, `/slider`, `/settings`, `/about`)

| Método | Ruta | Cache | Descripción |
|--------|------|-------|-------------|
| GET | `/categories` | 120s | Listar categorías (plano o árbol) |
| GET | `/categories/{slug}` | 120s | Categoría por slug |
| GET | `/products?search=&categoryId=&minPrice=&maxPrice=&isActive=&sortBy=&page=&pageSize=` | 60s | Listar productos con filtros y paginación |
| GET | `/products/featured` | 120s | Productos destacados |
| GET | `/products/{slug}` | — | Detalle del producto con JSON-LD |
| POST | `/products/{id}/track-view` | — | Registrar vista de producto |
| GET | `/slider` | 120s | Slider activo |
| GET | `/settings/public` | 300s | Configuración pública del sitio |
| GET | `/about` | 120s | Información de la empresa + galería |

### SEO

| Método | Ruta | Cache | Descripción |
|--------|------|-------|-------------|
| GET | `/robots.txt` | 86400s | Robots.txt |
| GET | `/sitemap.xml` | 86400s | Sitemap XML |

### Panel de Administración (`/api/v1/admin/*`)

Todos los endpoints requieren JWT + permiso específico. Rate limit: 60/min.

#### Productos
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/products` | `products.view` |
| GET | `/admin/products/{id}` | `products.view` |
| POST | `/admin/products` | `products.create` |
| PUT | `/admin/products/{id}` | `products.update` |
| DELETE | `/admin/products/{id}` | `products.delete` |
| POST | `/admin/products/{id}/images` | `products.update` |
| DELETE | `/admin/products/{id}/images/{imageId}` | `products.update` |
| PUT | `/admin/products/{id}/images/order` | `products.update` |
| POST | `/admin/products/{id}/restrictions` | `products.manage-restrictions` |
| PUT | `/admin/products/{id}/restrictions/{restrictionId}` | `products.manage-restrictions` |
| DELETE | `/admin/products/{id}/restrictions/{restrictionId}` | `products.manage-restrictions` |

#### Categorías
| Método | Ruta | Permiso |
|--------|------|---------|
| POST | `/admin/categories` | `categories.create` |
| PUT | `/admin/categories/{id}` | `categories.update` |
| DELETE | `/admin/categories/{id}` | `categories.delete` |

#### Slider
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/slider` | `slider.view` |
| POST | `/admin/slider` | `slider.create` |
| PUT | `/admin/slider/{id}` | `slider.update` |
| DELETE | `/admin/slider/{id}` | `slider.delete` |
| PUT | `/admin/slider/order` | `slider.update` |

#### Configuración
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/settings` | `settings.view` |
| PUT | `/admin/settings/{key}` | `settings.update` |
| POST | `/admin/settings/logo` | `settings.update` |

#### About Us
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/about` | `about.view` |
| PUT | `/admin/about` | `about.update` |
| POST | `/admin/about/gallery?section=` | `about.update` |
| DELETE | `/admin/about/gallery/{id}` | `about.update` |
| PUT | `/admin/about/gallery/order` | `about.update` |

#### Usuarios
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/users` | `users.view` |
| GET | `/admin/users/{id}` | `users.view` |
| POST | `/admin/users` | `users.create` |
| PUT | `/admin/users/{id}` | `users.update` |
| DELETE | `/admin/users/{id}` | `users.delete` |
| PUT | `/admin/users/{id}/roles` | `users.manage-roles` |

#### Roles y Permisos
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/roles` | `roles.view` |
| GET | `/admin/permissions` | `roles.view` |
| POST | `/admin/roles` | `roles.create` |
| PUT | `/admin/roles/{id}` | `roles.update` |
| DELETE | `/admin/roles/{id}` | `roles.delete` |
| PUT | `/admin/roles/{id}/permissions` | `roles.update` |

#### Estadísticas
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/stats/overview` | `stats.view` |
| GET | `/admin/stats/products/top-viewed` | `stats.view` |
| GET | `/admin/stats/products/top-sellers` | `stats.view` |
| GET | `/admin/stats/products/low-stock` | `stats.view` |

#### Auditoría
| Método | Ruta | Permiso |
|--------|------|---------|
| GET | `/admin/audit-logs` | `audit.view` |

---

## Seguridad

| Característica | Implementación |
|----------------|---------------|
| Autenticación | JWT Bearer con configuración de issuer/audience y zero clock skew |
| Refresh tokens | Rotación automática con detección de robo (revoca todos si se reusa uno revocado) |
| Hash de contraseñas | BCrypt |
| Bloqueo de cuenta | Tras 5 intentos fallidos, bloqueo de 15 minutos |
| Rate limiting | 3 niveles: Auth (10/min), Admin (60/min), Catálogo (120/min) |
| CSP | Content-Security-Policy con nonces para scripts y estilos |
| Headers de seguridad | X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy, Permissions-Policy |
| CORS | Configurable por orígenes permitidos |
| Autorización | Policy-based con 28 permisos granulares en 9 módulos |
| Concurrencia | Optimistic concurrency mediante RowVersion (bytea) |
| Soft delete | Todas las entidades de negocio usan `DeletedAt` |
| Validación | FluentValidation en todos los DTOs de entrada |
| Auditoría | Registro automático de CREATE/UPDATE/DELETE con cambios serializados en JSONB |

---

## Performance

| Optimización | Detalle |
|-------------|---------|
| View tracking en memoria | Las vistas de productos se acumulan en `ConcurrentDictionary` y se persisten en batch cada 30s vía raw SQL con `ON CONFLICT DO UPDATE` |
| Response caching | Endpoints públicos cacheados entre 60s y 86400s |
| Compresión | Brotli + Gzip habilitados para HTTP y HTTPS |
| Consultas | `AsNoTracking()` en todas las lecturas + proyecciones con `Select()` |
| JSONB | Columnas `config` y `changes` en JSONB con índices GIN |
| Índices filtrados | Índices parciales para consultas frecuentes |
| Caché de reflection | `ConcurrentDictionary` para evitar reflexión repetitiva en auditoría |

---

## Entidades (Base de Datos)

```
User ──1:N──> UserRole ──N:1──> Role ──1:N──> RolePermission ──N:1──> Permission
User ──1:N──> RefreshToken
User ──1:N──> PasswordResetToken
User ──1:N──> EmailChangeRequest
User ──1:N──> AuditLog (CreatedBy)

Category ──self-ref──> Category (ParentCategoryId)
Category ──1:N──> Product

Product ──1:N──> ProductImage
Product ──1:N──> ProductRestriction
Product ──1:N──> ProductStatsDaily
Product ──N:1──> User (CreatedBy)

AboutInfo ──1:N──> AboutGallery

SliderItem ──N:1──> User (CreatedBy)
SiteSetting ──N:1──> User (UpdatedBy)
```

---

## Seed Inicial

- **28 permisos** en 9 módulos: `products`, `categories`, `slider`, `settings`, `users`, `roles`, `stats`, `audit`, `about`
- **Rol SuperAdmin** con todos los permisos
- **Usuario admin** creado desde `ADMIN_EMAIL`/`ADMIN_PASSWORD` (se omite si no están definidas)

---

## Tests

18 pruebas unitarias con xUnit + Moq + EF Core InMemory + FluentValidation TestHelper:

| Clase de prueba | Pruebas |
|----------------|---------|
| LoginRequestValidatorTests | 4 |
| CreateProductRequestValidatorTests | 5 |
| CreateCategoryRequestValidatorTests | 2 |
| InMemoryViewTrackerTests | 2 |
| TokenServiceTests | 4 |
| UnitTest1 | 1 |

---

## Licencia

MIT
