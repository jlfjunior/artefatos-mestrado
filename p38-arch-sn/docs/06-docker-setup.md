# 06 — Docker Setup

## Dockerfile — Transaction API

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FinancialBalance.Api/FinancialBalance.Api.csproj", "FinancialBalance.Api/"]
COPY ["src/FinancialBalance.Application/FinancialBalance.Application.csproj", "FinancialBalance.Application/"]
COPY ["src/FinancialBalance.Domain/FinancialBalance.Domain.csproj", "FinancialBalance.Domain/"]
COPY ["src/FinancialBalance.Infrastructure/FinancialBalance.Infrastructure.csproj", "FinancialBalance.Infrastructure/"]

RUN dotnet restore "FinancialBalance.Api/FinancialBalance.Api.csproj"

COPY src/ .
WORKDIR /src/FinancialBalance.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "FinancialBalance.Api.dll"]
```

## Dockerfile — Reporting API

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FinancialBalance.ReportingApi/FinancialBalance.ReportingApi.csproj", "FinancialBalance.ReportingApi/"]
COPY ["src/FinancialBalance.Application/FinancialBalance.Application.csproj", "FinancialBalance.Application/"]
COPY ["src/FinancialBalance.Domain/FinancialBalance.Domain.csproj", "FinancialBalance.Domain/"]
COPY ["src/FinancialBalance.ReportingInfrastructure/FinancialBalance.ReportingInfrastructure.csproj", "FinancialBalance.ReportingInfrastructure/"]

RUN dotnet restore "FinancialBalance.ReportingApi/FinancialBalance.ReportingApi.csproj"
COPY src/ .
WORKDIR /src/FinancialBalance.ReportingApi
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinancialBalance.ReportingApi.dll"]
```

## Dockerfile — Reporting Worker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/FinancialBalance.Worker/FinancialBalance.Worker.csproj", "FinancialBalance.Worker/"]
COPY ["src/FinancialBalance.Application/FinancialBalance.Application.csproj", "FinancialBalance.Application/"]
COPY ["src/FinancialBalance.Domain/FinancialBalance.Domain.csproj", "FinancialBalance.Domain/"]

RUN dotnet restore "FinancialBalance.Worker/FinancialBalance.Worker.csproj"
COPY src/ .
WORKDIR /src/FinancialBalance.Worker
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FinancialBalance.Worker.dll"]
```

---

## docker-compose.yml (Local Development)

```yaml
version: '3.9'

services:

  transaction-api:
    build:
      context: .
      dockerfile: src/FinancialBalance.Api/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__Postgres=Host=postgres;Database=financialbalance;Username=app;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=guest
      - RabbitMQ__Password=guest
      - RabbitMQ__Uri=amqp://guest:guest@rabbitmq:5672
      - Auth__Authority=https://dev-idp.example.com
      - Auth__Audience=financial-balance-api
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:8080/health/ready || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 15s
    restart: unless-stopped

  reporting-api:
    build:
      context: .
      dockerfile: src/FinancialBalance.ReportingApi/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__Postgres=Host=postgres;Database=financialbalance;Username=app;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=guest
      - RabbitMQ__Password=guest
      - RabbitMQ__Uri=amqp://guest:guest@rabbitmq:5672
      - Auth__Authority=https://dev-idp.example.com
      - Auth__Audience=financial-balance-api
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:8080/health/ready || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 15s
    restart: unless-stopped

  reporting-worker:
    build:
      context: .
      dockerfile: src/FinancialBalance.Worker/Dockerfile
    environment:
      - DOTNET_ENVIRONMENT=Development
      - ConnectionStrings__Postgres=Host=postgres;Database=financialbalance;Username=app;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=guest
      - RabbitMQ__Password=guest
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    restart: unless-stopped

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: financialbalance
      POSTGRES_USER: app
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./infra/sql/init.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U app -d financialbalance"]
      interval: 5s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5
    restart: unless-stopped

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"   # Management UI — http://localhost:15672 (guest/guest)
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:
```

---

## .dockerignore

```
**/.git
**/.vs
**/bin
**/obj
**/*.user
**/.env
**/appsettings.Development.json
```

---

## Useful Commands

```bash
# Start all services
docker compose up -d

# Rebuild and restart a specific service
docker compose up -d --build transaction-api

# Run EF Core migrations
docker compose run --rm transaction-api dotnet ef database update

# View logs
docker compose logs -f transaction-api

# Check health status of all services
docker compose ps

# Stop and remove volumes
docker compose down -v
```

---

## Service Ports

| Service | Host Port | Container Port |
|---|---|---|
| Transaction API | 5000 | 8080 |
| Reporting API | 5001 | 8080 |
| PostgreSQL | 5432 | 5432 |
| Redis | 6379 | 6379 |
| RabbitMQ AMQP | 5672 | 5672 |
| RabbitMQ Management UI | 15672 | 15672 |

---

## Image Tagging Strategy

```
financialbalance/transaction-api:latest
financialbalance/transaction-api:1.2.0
financialbalance/transaction-api:1.2.0-sha-abc1234
```

CI pipeline tags with semantic version + git SHA for full traceability.
