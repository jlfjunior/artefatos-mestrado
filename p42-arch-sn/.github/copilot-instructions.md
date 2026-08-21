# Copilot Instructions

## Project Overview

**controleDeLancamentos** is a financial transaction control system that handles debits/credits (lançamentos) and generates a daily consolidated balance (saldo diário consolidado). It is based on the software architect challenge described in `desafio-arquiteto-software-out2024.pdf`.

## Tech Stack

- **.NET 10** / **C#**
- **Architecture:** Clean Architecture
- **Database:** PostgreSQL
- **API:** REST with OpenAPI (Swagger)

## Build, Test & Lint Commands

> Update this section as the project takes shape.

```sh
# Build
dotnet build

# Run tests
dotnet test

# Run a single test
dotnet test --filter "TestName~<MethodName>"

# Run the application
dotnet run --project src/<ProjectName>
```

## Architecture

### Clean Architecture Layers

```
src/
  ControleLancamentos.Domain/        # Entities, value objects, domain interfaces
  ControleLancamentos.Application/   # Use cases, DTOs, application interfaces
  ControleLancamentos.Infrastructure/# EF Core + PostgreSQL, repositories, messaging
  ControleLancamentos.Api/           # ASP.NET Core controllers, OpenAPI/Swagger config
tests/
  ControleLancamentos.Domain.Tests/
  ControleLancamentos.Application.Tests/
  ControleLancamentos.Infrastructure.Tests/
  ControleLancamentos.Api.Tests/
```

**Test framework:** NUnit (`NUnit`, `NUnit3TestAdapter`, `Microsoft.NET.Test.Sdk`)

**Dependency rule:** outer layers depend on inner layers, never the reverse. `Api` → `Application` → `Domain`. `Infrastructure` implements interfaces defined in `Application`/`Domain`.

### Bounded Contexts

- **Lançamentos** — records individual debit/credit transactions
- **Consolidado Diário** — aggregates transactions into a daily balance report

These two contexts are loosely coupled. Prefer messaging or events over direct calls between them.

### Database (PostgreSQL)

- Access via **Entity Framework Core** with PostgreSQL provider (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- Migrations live in `Infrastructure`
- Repository interfaces are defined in `Domain`/`Application`; implementations in `Infrastructure`

### API (OpenAPI)

- Use **Swashbuckle** (or **NSwag**) to generate the OpenAPI spec
- Annotate controllers and DTOs with XML doc comments and data annotations so the spec is complete
- Swagger UI is available at `/swagger` in development

## Authentication (JWT)

- Use **ASP.NET Core JWT Bearer** authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- Token validation config (issuer, audience, secret) lives in `appsettings.json` / environment variables — never hardcoded
- Protect endpoints with `[Authorize]`; public endpoints are explicitly marked `[AllowAnonymous]`
- JWT secret must be injected via environment variable (`JWT__Secret` or similar) in all environments

## Docker / Container Setup

```
docker-compose.yml          # Orchestrates api + postgres
Dockerfile                  # Multi-stage build for the API
```

- **Multi-stage Dockerfile:** `build` stage uses `mcr.microsoft.com/dotnet/sdk`, final stage uses `mcr.microsoft.com/dotnet/aspnet`
- **docker-compose** brings up:
  - `api` service (the ASP.NET Core app)
  - `db` service (`postgres:latest` or pinned version)
  - `pgadmin` service (`dpage/pgadmin4`) — accessible at `http://localhost:5050`; credentials via env vars (`PGADMIN_DEFAULT_EMAIL`, `PGADMIN_DEFAULT_PASSWORD`)
- Database connection string and JWT secret are passed as environment variables in `docker-compose.yml`; use a `.env` file locally (gitignored)
- Run locally:

```sh
docker compose up --build
```

## Key Conventions

- Project is in **Brazilian Portuguese** — domain terms (lançamento, crédito, débito, saldo, consolidado) should be kept in Portuguese in code, comments, and API contracts.
- Follow standard .NET project layout: `src/` for application code, `tests/` for test projects.
- Secrets (DB connection string, JWT secret) are always provided via environment variables or `.env` (already gitignored); never committed to source control.
