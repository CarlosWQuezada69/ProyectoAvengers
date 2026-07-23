# Prompt para generar la API y la base de datos

Copia y pega el bloque de abajo (todo el contenido dentro del bloque de código) en Claude Code, junto con los dos archivos adjuntos: `entidades-base-datos.sql` y `api-endpoints.md`. El prompt asume que ambos archivos están disponibles en el repositorio o en el contexto de la conversación.

---

```
Eres un ingeniero de software senior especializado en ASP.NET Core (C#), Entity
Framework Core, PostgreSQL y Angular. Vas a generar, de forma incremental, la
base de datos y la API REST de una aplicación de catálogo de productos con
panel administrativo.

<contexto>
La aplicación tiene dos frentes: un sitio público de catálogo (productos,
búsqueda, carrusel de noticias, productos con restricciones de venta) y un
panel administrativo (gestión de productos, categorías, carrusel, logo/nombre/
copyright del sitio, usuarios y roles con permisos granulares, estadísticas,
recuperación de contraseña y cambio de correo). El sistema debe ser escalable
y el control de acceso debe basarse en permisos, no en roles fijos en código.
</contexto>

<stack_tecnico>
- Backend: .NET 8, ASP.NET Core Web API, Entity Framework Core con proveedor
  Npgsql, FluentValidation, JWT Bearer para autenticación.
- Base de datos: PostgreSQL. El esquema ya está definido en el archivo adjunto
  `entidades-base-datos.sql` — es la fuente de verdad. No inventes tablas ni
  columnas que no estén ahí; si una funcionalidad requiere algo adicional,
  dilo explícitamente antes de crearlo y espera confirmación.
- Arquitectura: Clean Architecture / N-Layer (Api, Application, Domain,
  Infrastructure, Shared) más un proyecto de tests.
- Almacenamiento de archivos (imágenes de producto, slider, logo): detrás de
  una interfaz `IFileStorage`, con una implementación local para desarrollo y
  espacio para una implementación en la nube (S3/Azure Blob/MinIO) más adelante.
- Envío de correo (recuperación de contraseña, confirmación de cambio de
  correo): detrás de una interfaz `IEmailSender`.
</stack_tecnico>

<convenciones_api>
Sigue exactamente las rutas, métodos y permisos descritos en el archivo
adjunto `api-endpoints.md`. Además:
- Prefijo de versión: /api/v1
- Rutas de administración bajo /api/v1/admin/... protegidas por un filtro de
  autorización base que exige al menos un permiso administrativo.
- Autorización por policy basada en el código de permiso (ej. "products.create"),
  resuelta dinámicamente contra los permisos del usuario (via sus roles), no
  hardcodeada por rol.
- Paginación: query params page/pageSize (máx. 100), respuesta
  { data, page, pageSize, totalCount, totalPages }.
- Errores: formato ProblemDetails (RFC 7807), con un middleware central que
  traduzca excepciones de dominio/validación a la respuesta HTTP correcta
  (400 validación, 401 no autenticado, 403 sin permiso, 404 no encontrado,
  409 conflicto de negocio o de concurrencia).
- Eliminar = soft delete (columna deleted_at) en products y users. Nunca
  DELETE físico en esas tablas.
- Concurrencia optimista en products usando la columna row_version.
</convenciones_api>

<pasos>
Genera el proyecto siguiendo este orden. Después de cada paso, resume en 3-5
líneas lo que implementaste antes de continuar al siguiente — no avances varios
pasos a la vez sin ese resumen.

1. Crea la estructura de la solución (.sln) con los proyectos Api, Application,
   Domain, Infrastructure, Shared y Tests. Configura las referencias entre
   proyectos respetando Clean Architecture (Domain no depende de nada;
   Infrastructure implementa interfaces de Application).

2. En Infrastructure, configura EF Core con Npgsql y el DbContext. Crea las
   entidades de Domain que correspondan 1:1 con las tablas de
   `entidades-base-datos.sql` (mismos nombres de tabla/columna vía Fluent API,
   mismos tipos, mismas relaciones y restricciones).

3. Genera la migración inicial de EF Core y confírmame que el SQL que produce
   sea equivalente al script adjunto antes de aplicarla.

4. Crea un seed inicial que inserte el catálogo de permisos y el rol
   SuperAdmin (ya incluidos en el script SQL) si no existen, y que cree un
   usuario administrador inicial leyendo email/contraseña desde variables de
   entorno (nunca hardcodeado).

5. Implementa autenticación: JWT access token de corta duración + refresh
   token rotativo persistido en refresh_tokens. Endpoints: login,
   refresh-token, logout, me.

6. Implementa el sistema de autorización por permisos (policy provider
   dinámico) y aplícalo a todos los endpoints admin desde este punto en
   adelante.

7. Implementa el módulo de cuenta: forgot-password, reset-password,
   change-email/request, change-email/confirm, usando IEmailSender.

8. Implementa el módulo de usuarios, roles y permisos (admin), siguiendo
   `api-endpoints.md`.

9. Implementa el módulo de categorías (jerárquico, slugs únicos).

10. Implementa el módulo de productos: CRUD, imágenes (vía IFileStorage),
    restricciones (config JSONB), concurrencia optimista con row_version.

11. Implementa el módulo de slider/carrusel con vigencia por fechas.

12. Implementa site_settings como clave-valor, separando el endpoint público
    (solo claves seguras) del endpoint admin (todas las claves).

13. Implementa estadísticas: agregación de vistas (en memoria o Redis) con
    flush periódico a product_stats_daily vía un background job, más los
    endpoints de top-viewed, top-sellers, low-stock y overview.

14. Implementa auditoría: registra automáticamente create/update/delete de
    entidades sensibles en audit_logs.

15. Agrega validaciones con FluentValidation para cada endpoint de
    creación/edición.

16. Documenta la API con Swagger/OpenAPI, agrupada por módulo.

17. Escribe pruebas unitarias para las reglas de negocio críticas
    (evaluación de restricciones de producto, expiración/rotación de tokens,
    resolución de permisos) y al menos una prueba de integración por módulo.

18. Entrega un docker-compose.yml con PostgreSQL (y Redis si lo usaste en el
    paso 13) para desarrollo local, y un README con los pasos para levantar
    el proyecto desde cero.
</pasos>

<criterios_de_aceptacion>
- El proyecto compila y corre con `dotnet run` después de `dotnet ef database
  update` contra una PostgreSQL vacía.
- Un endpoint admin sin token devuelve 401; con token pero sin el permiso
  requerido devuelve 403.
- Swagger está disponible y documenta todos los endpoints de
  `api-endpoints.md`.
- Ningún dato sensible (contraseñas, tokens) se expone en las respuestas de
  la API ni queda en texto plano en la base de datos.
</criterios_de_aceptacion>

Empieza por el paso 1. No generes código de pasos posteriores hasta que
confirme que continuemos.
```
