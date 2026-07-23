# Diseño de la API — Endpoints REST

Base URL: `/api/v1`
Autenticación: JWT Bearer (access token corto + refresh token rotativo)
Errores: formato `ProblemDetails` (RFC 7807)
Paginación estándar (recursos listables): `?page=1&pageSize=20` → `{ data, page, pageSize, totalCount, totalPages }`

---

## 1. Autenticación y cuenta

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/auth/login` | público | Autentica; devuelve `accessToken` + `refreshToken` |
| POST | `/auth/refresh-token` | refresh token válido | Rota el refresh token, emite nuevo access token |
| POST | `/auth/logout` | autenticado | Revoca el refresh token actual |
| GET | `/auth/me` | autenticado | Perfil + roles/permisos del usuario actual |
| POST | `/auth/forgot-password` | público | Genera token de recuperación y envía correo |
| POST | `/auth/reset-password` | público (token en body) | Aplica nueva contraseña |
| POST | `/account/change-email/request` | autenticado | Solicita cambio de correo; envía confirmación al correo nuevo |
| GET | `/account/change-email/confirm` | público (token en query) | Confirma y aplica el cambio de correo |

**Ejemplo — login**
```json
// Request
{ "email": "user@correo.com", "password": "••••••••" }

// Response 200
{
  "accessToken": "eyJ...",
  "refreshToken": "8f3a...",
  "expiresIn": 900,
  "user": {
    "id": "uuid",
    "name": "Juan Pérez",
    "email": "user@correo.com",
    "roles": ["Gestor de catálogo"],
    "permissions": ["products.create", "products.update", "products.delete"]
  }
}
```

Notas:
- `forgot-password` y `reset-password` siempre responden 200 aunque el correo no exista (evita enumerar usuarios).
- El token de `change-email/confirm` debe expirar (ej. 24h) y ser de un solo uso.

---

## 2. Usuarios (admin)

| Método | Ruta | Permiso | Descripción |
|---|---|---|---|
| GET | `/admin/users?search=&roleId=&isActive=&page=&pageSize=` | `users.view` | Listado con filtros |
| GET | `/admin/users/{id}` | `users.view` | Detalle |
| POST | `/admin/users` | `users.create` | Crear usuario |
| PUT | `/admin/users/{id}` | `users.update` | Editar datos |
| DELETE | `/admin/users/{id}` | `users.delete` | Desactivar (soft delete, nunca borrado físico) |
| PUT | `/admin/users/{id}/roles` | `users.manage-roles` | Body: `{ "roleIds": ["uuid", "uuid"] }` |

---

## 3. Roles y permisos (admin)

| Método | Ruta | Permiso | Descripción |
|---|---|---|---|
| GET | `/admin/roles` | `roles.view` | Listado |
| POST | `/admin/roles` | `roles.create` | Crear rol |
| PUT | `/admin/roles/{id}` | `roles.update` | Editar nombre/descripción |
| DELETE | `/admin/roles/{id}` | `roles.delete` | Bloquear si hay usuarios asignados (409), o exigir reasignación |
| GET | `/admin/permissions` | `roles.view` | Catálogo de permisos disponibles (fijo en el sistema, no editable) |
| PUT | `/admin/roles/{id}/permissions` | `roles.update` | Body: `{ "permissionIds": ["uuid", ...] }` |

---

## 4. Categorías

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/categories?tree=true` | público | Árbol o listado plano |
| GET | `/categories/{slug}` | público | Detalle |
| POST | `/admin/categories` | `categories.create` | Crear |
| PUT | `/admin/categories/{id}` | `categories.update` | Editar |
| DELETE | `/admin/categories/{id}` | `categories.delete` | Bloquear si tiene productos activos (409) |

---

## 5. Productos — vista pública

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/products?search=&categoryId=&minPrice=&maxPrice=&onlyAvailable=&sort=&page=&pageSize=` | público | Catálogo con filtros y orden |
| GET | `/products/{slug}` | público | Detalle, incluye imágenes y restricciones activas resumidas (ej. "máx. 2 por cliente") |
| GET | `/products/featured` | público | Destacados para el home |
| POST | `/products/{id}/track-view` | público, con throttling por IP | Incrementa el contador de vistas del día (ver nota de rendimiento abajo) |

---

## 6. Productos — administración

| Método | Ruta | Permiso | Descripción |
|---|---|---|---|
| GET | `/admin/products?...` | `products.view` | Listado admin (incluye inactivos) |
| POST | `/admin/products` | `products.create` | Crear |
| PUT | `/admin/products/{id}` | `products.update` | Editar (usar `If-Match`/`row_version` para concurrencia optimista) |
| DELETE | `/admin/products/{id}` | `products.delete` | Soft delete |
| POST | `/admin/products/{id}/images` | `products.update` | `multipart/form-data` |
| DELETE | `/admin/products/{id}/images/{imageId}` | `products.update` | — |
| PUT | `/admin/products/{id}/images/order` | `products.update` | Body: `[{ "imageId": "uuid", "displayOrder": 1 }]` |
| POST | `/admin/products/{id}/restrictions` | `products.manage-restrictions` | Crear restricción |
| PUT | `/admin/products/{id}/restrictions/{restrictionId}` | `products.manage-restrictions` | Editar |
| DELETE | `/admin/products/{id}/restrictions/{restrictionId}` | `products.manage-restrictions` | Eliminar |

**Ejemplo — crear restricción**
```json
{
  "restrictionType": "PURCHASE_LIMIT_USER",
  "config": { "maxUnits": 2, "periodDays": 30 },
  "startsAt": "2026-08-01T00:00:00Z",
  "endsAt": "2026-09-01T00:00:00Z",
  "isActive": true
}
```

---

## 7. Slider / carrusel

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/slider` | público | Solo ítems activos y vigentes, ya ordenados |
| GET | `/admin/slider` | `slider.view` | Todos, incluidos inactivos/expirados |
| POST | `/admin/slider` | `slider.create` | `multipart/form-data` (imagen + campos) |
| PUT | `/admin/slider/{id}` | `slider.update` | Editar |
| DELETE | `/admin/slider/{id}` | `slider.delete` | Eliminar |
| PUT | `/admin/slider/order` | `slider.update` | Body: `[{ "id": "uuid", "displayOrder": 1 }]` |

---

## 8. Configuración del sitio

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/settings/public` | público | Solo claves seguras: `logo_url`, `business_name`, `copyright_text`, `contact_*`, `social_links` |
| GET | `/admin/settings` | `settings.view` | Todas las claves |
| PUT | `/admin/settings/{key}` | `settings.update` | Actualiza un valor (ej. `copyright_text`) |
| POST | `/admin/settings/logo` | `settings.update` | `multipart/form-data`, reemplaza el logo, devuelve la nueva URL |

---

## 9. Estadísticas (admin)

| Método | Ruta | Permiso | Descripción |
|---|---|---|---|
| GET | `/admin/stats/overview` | `stats.view` | Resumen para el dashboard |
| GET | `/admin/stats/products/top-viewed?from=&to=&limit=` | `stats.view` | Más vistos en el rango |
| GET | `/admin/stats/products/top-sellers?from=&to=&limit=` | `stats.view` | Más vendidos en el rango |
| GET | `/admin/stats/products/low-stock?threshold=` | `stats.view` | Alerta de stock bajo |

---

## 10. Auditoría (admin)

| Método | Ruta | Permiso | Descripción |
|---|---|---|---|
| GET | `/admin/audit-logs?userId=&entityName=&from=&to=&page=&pageSize=` | `audit.view` | Historial de cambios |

---

## 11. Notas de implementación

- **`track-view` no debe golpear la BD en cada request.** Acumula en memoria o Redis (contador por `productId` + día) y haz *flush* periódico (cada X segundos, vía background job) hacia `product_stats_daily`. Un `INSERT`/`UPDATE` por cada vista de producto no escala con tráfico real.
- **Concurrencia optimista en `PUT /admin/products/{id}`:** el cliente envía el `row_version` que recibió al leer el producto; si no coincide con el actual, el servidor responde 409 (otro admin ya lo editó).
- **Todos los `DELETE` de recursos de negocio son soft delete** (marcan `deleted_at`), y devuelven 204. El registro sigue existiendo para no romper referencias en pedidos/auditoría futuros.
- **Validaciones que generan 409 Conflict en vez de 400:** eliminar una categoría con productos activos, eliminar un rol con usuarios asignados.
- **Subida de archivos** (imágenes de producto, slider, logo): validar tipo MIME y tamaño máximo en el backend, no solo en Angular — el frontend puede ser evitado con un cliente HTTP directo.
- **`/settings/public` vs `/admin/settings`:** separarlos evita exponer accidentalmente una clave sensible que se agregue más adelante (ej. credenciales de un servicio externo guardadas ahí por error).
