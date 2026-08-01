# TODO - Próxima sesión: Mejoras de calidad

## Frontend (Angular) — Prioridades

### Alta
- [ ] Agregar `ChangeDetectionStrategy.OnPush` a todos los componentes
- [ ] Reemplazar `DomSanitizer.bypassSecurityTrustHtml()` en `Frontend/Panel-administrativo/src/app/shared/layout/layout.ts:87`
- [ ] Agregar `take(1)` a `AuthService.loadUser()` y a petición HTTP en `BrandingService`
- [ ] Proteger `JSON.parse` con try/catch en `product-form.ts:141`
- [ ] Reemplazar `confirm()` nativa por modal personalizado (5 componentes)

### Media
- [ ] Refactorizar `ProductFormComponent` extrayendo sub-componentes (imágenes, restricciones)
- [ ] Extraer SVG icons del `LayoutComponent` a archivo separado
- [ ] Usar `TableComponent` compartido en las 6 listas o crear `PaginationComponent`
- [ ] Reemplazar emojis como iconos (✏️, 🗑️) por SVGs inline
- [ ] Crear servicios dedicados para forgot-password, reset-password, confirm-email

---

## Backend (.NET) — Prioridades

### Crítica
- [ ] Mover lógica de negocio de controladores a Application (servicios de aplicación / casos de uso)
- [ ] Encapsular entidades: propiedades `{ get; private set; }`, métodos de dominio
- [ ] Crear Value Objects: `Email`, `Slug`, `Money`, `PhoneNumber`

### Alta
- [ ] Eliminar duplicación de mapeo DTO (considerar AutoMapper o extension methods)
- [ ] Centralizar validación de archivos MIME (repetido en 3 controladores)
- [ ] Mover JWT Secret de `appsettings.json` a User Secrets / variables de entorno
- [ ] Eliminar `UnitTest1.cs` placeholder
- [ ] Escribir tests de integración para controladores (`WebApplicationFactory`)
- [ ] Tests para autorización y audit trail

### Media
- [ ] Implementar Repository Pattern
- [ ] Configurar `UseQueryTrackingBehavior(NoTracking)` global
- [ ] Remover default password de `AppDbContextFactory`
- [ ] Configurar CORS estricto (no AllowAnyOrigin en producción)
- [ ] Extraer regex de slug a constante compartida

---

## Notas
- Backend corre en `http://localhost:5167` (no Docker)
- Frontend apunta a `http://localhost:5167/api/v1` (ya corregido)
- Admin: `admin@example.com` / `Admin123!`
- PostgreSQL local sin Docker
