# TODO - Próxima sesión: Mejoras de calidad

> Estado actualizado tras la sesión de refactor: las tareas de calidad (OnPush, DomSanitizer, JSON.parse, confirm(), lógica de negocio en Application, encapsulación, Value Objects e integración de los mismos, centralización de DTOs y JWT en variables de entorno) están completadas. A continuación quedan las pendientes restantes.

## Historial de sesiones

### 2026-09-22 — Integración tienda ↔ panel admin, stats de vistas y carrito-cotización
> Commit: `3838d0d`

**Backend (API)**
- Slider: uploader conectado para crear y reemplazar imágenes desde el panel (`PUT /api/v1/admin/slider/{id}` acepta `IFormFile? image` + `[FromForm]`; `SliderItem.UpdateImage`).
- Stats de vistas por página (sin ventas): entidad `PageViewDaily` + configuración + migración `AddPageViewStats` aplicada; `TrackPageView` en `IViewTracker`/`InMemoryViewTracker` con flush a `page_view_daily`; endpoint público `POST /api/v1/analytics/page-view`.
- Fix de bug pre-existente: el `CancellationToken` se pasaba como parámetro SQL, por lo que las vistas de producto nunca se persistían (corregido en los upserts).
- Stats redefinidas: eliminados `TotalOrders`, `MonthlyPurchases` y `TopSellers`; nuevas `DailyViewsStat`, `PageViewsStat`, `TodayPageViews`, `MonthlyPageViews`.
- Settings: `PublicKeys` ampliado con `rnc` y `address`. Tests: 28/28 en verde.

**Panel administrativo**
- Dashboard: gráfico donut (visitas por página, SVG) y de barras (últimos 7 días); KPIs actualizados; eliminada la tarjeta "Más vendidos".
- Slider: subir/reemplazar imagen correctamente (crear y editar) con previews.
- Pipe `assetUrl` (`core/utils/asset-url.ts` + `core/pipes/asset-url.pipe.ts`) aplicado a slider-list, product-form y product-list.
- Settings → Legal: campos **RNC** y **Dirección**.
- Sobre nosotros: módulo de administración de la página "Acerca de".

**Tienda (Vista-clientes)**
- Tracking de vistas de página por navegación (`app.ts` → `sendPageView`; `catalog.service.trackPageView`).
- Header: eliminada la barra de búsqueda (la búsqueda vive en el catálogo). Logo + carrito + menú.
- Footer: enlace discreto al panel (icono de escudo, `target=_blank`) que se oculta si `adminUrl` está vacío.
- Carrito como cotización: botón **"Imprimir cotización (PDF)"** con plantilla imprimible (`@media print`): logo, nombre de empresa, RNC, dirección, N.º de cotización, fecha, tabla de artículos (cantidad, precio unitario, total), subtotal y nota "sin valor de pago". WhatsApp queda como acción secundaria.
- Icono `printer` añadido al set de iconos.
- `PublicSettings`: nuevos campos `rnc` y `address`; environments con `adminUrl` (dev `http://localhost:4200`).
- SEO: servicio que aplica título/descripción por navegación.

**Verificación**: builds OK (backend, panel y tienda); migración aplicada y tabla `page_view_daily` verificada; stats verificadas end-to-end; push a GitHub `3838d0d`.

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
