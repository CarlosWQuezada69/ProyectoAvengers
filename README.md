# Proyecto Avengers

Sistema full-stack para catálogo de productos con panel de administración. Backend API REST en ASP.NET Core 8 + Frontend SPA en Angular 22.

## Project Structure

```
Backend/                     → API REST (.NET 8, Clean Architecture)
  src/
    Api/                     → Controllers, middleware, authorization
    Application/             → Service interfaces, validators
    Domain/                  → Business entities
    Infrastructure/          → EF Core, migrations, services, seeding
    Shared/                  → DTOs
  tests/                     → xUnit unit tests
  docker-compose.yml         → PostgreSQL + API
Frontend/
  Panel-administrativo/      → Admin SPA (Angular 22)
  Vista-clientes/            → Public catalog (coming soon)
```

## Quick Start

```bash
# Prerequisites: .NET 8 SDK, Node.js 20+, PostgreSQL 16

# Start the API
cd Backend
docker compose up -d postgres     # or use your own PostgreSQL
export JWT_SECRET="your-256-bit-secret"
export ADMIN_EMAIL=admin@example.com
export ADMIN_PASSWORD=Admin123!
dotnet run --project src/Api

# Start the admin panel (new terminal)
cd Frontend/Panel-administrativo
npm install
npx ng serve
```

| Service | URL |
|---------|-----|
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| Admin Panel | http://localhost:4200 |
| PostgreSQL | localhost:5432 |

Login: `admin@example.com` / `Admin123!`

> API documentation, endpoint listings, entity model, security details, and performance notes are maintained in [`Backend/README.md`](Backend/README.md).
