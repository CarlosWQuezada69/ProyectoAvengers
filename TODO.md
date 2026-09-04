# TODO - Próxima sesión: Mejoras de calidad

> Estado actualizado tras la sesión de refactor: las tareas de calidad (OnPush, DomSanitizer, JSON.parse, confirm(), lógica de negocio en Application, encapsulación, Value Objects e integración de los mismos, centralización de DTOs y JWT en variables de entorno) están completadas. A continuación quedan las pendientes restantes.

## Frontend (Angular) — Prioridades

### Alta
- [ ] Refactorizar `ProductFormComponent` extrayendo sub-componentes (imágenes, restricciones)
- [ ] Extraer SVG icons del `LayoutComponent` a archivo separado
- [ ] Usar `TableComponent` compartido en las 6 listas o crear `PaginationComponent`
- [ ] Reemplazar emojis como iconos (✏️, 🗑️) por SVGs inline
- [ ] Crear servicios dedicados para forgot-password, reset-password, confirm-email

### Media
- [ ] Evaluar componentes con estado `loading: boolean` sin signal + `markForCheck` para evitar problemas de render con OnPush

---

## Backend (.NET) — Prioridades

### Crítica
- [ ] Integrar Value Objects (Email, Slug, Money, PhoneNumber) dentro de las entidades (usar en vez de `string`/`decimal` directos)
- [ ] Mover lógica de negocio de controladores restantes (públicos: Products, Categories, Slider, Settings, About, SEO) a capa Application

### Alta
- [ ] Implementar Repository Pattern
- [ ] Configurar CORS estricto en producción (sin AllowAnyOrigin)
- [ ] Extraer regex de slug a constante compartida (ya existe en `Constants.SlugPattern`, validar coherencia)
- [ ] Agregar validación de `IFormFile` de forma unificada para todos los controladores (ya usan `ImageFileValidator`, verificar cobertura total)
- [ ] Escribir tests de integración para el resto de controladores (Products, Users, Roles, Categories, Slider)
- [ ] Tests para autorización y audit trail

### Media
- [ ] Remover default password de `AppDbContextFactory`

---

## Notas
- Backend corre en `http://localhost:5167` (no Docker)
- Frontend apunta a `http://localhost:5167/api/v1`
- Admin: `admin@example.com` / `Admin123!`
- PostgreSQL local sin Docker
- `JWT_SECRET` se lee de variable de entorno o User Secrets (mínimo 32 caracteres), no está en appsettings
- `UseQueryTrackingBehavior(NoTracking)` global ya configurado en Infrastructure
- Tests de integración (`WebApplicationFactory` + EF Core InMemory) añadidos en `tests/ProyectoAvengers.Tests/Integration`
- `AppDbContext.SerializeChanges` ya no usa `GetColumnType()` (compatible con InMemory)
