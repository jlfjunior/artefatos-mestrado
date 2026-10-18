# 01 — System Architecture

## Overview

The Financial Balance Management System is a cloud-native, event-driven application built on .NET 8 / C# that handles financial account management, transaction processing, and daily/monthly reporting. It is composed of three independent services sharing a single PostgreSQL database, communicating asynchronously via RabbitMQ.

---

## High-Level Architecture

```
                        ┌─────────────────────────────────────────────┐
                        │              API Gateway / Ingress            │
                        │          (NGINX Ingress / AWS ALB)            │
                        └──────────────┬──────────────┬────────────────┘
                                       │              │
                        ┌──────────────▼──┐    ┌──────▼──────────────┐
                        │  Transaction API │    │   Reporting API      │
                        │  (.NET 8)        │    │   (.NET 8)           │
                        │  :5000           │    │   :5001              │
                        │  HPA: 2–20 pods  │    │   HPA: 2–10 pods     │
                        └──────────────┬──┘    └──────┬──────────────┘
                                       │    reads      │
                              writes   │               │ reads + cache
                        ┌─────────────▼───────────────▼────────────────┐
                        │          PostgreSQL 16 (shared DB)             │
                        │    finance schema   |   reporting schema        │
                        └────────────────────┬──────────────────────────┘
                                             │
                        ┌────────────────────▼─────────────────────────┐
                        │              RabbitMQ 3.13                     │
                        │   TransactionCreated | TransactionCancelled    │
                        └────────────────────┬─────────────────────────┘
                                             │
                        ┌────────────────────▼─────────────────────────┐
                        │             Reporting Worker                   │
                        │          (.NET 8 Background Service)           │
                        │        KEDA: 1–10 pods (queue-depth)           │
                        └────────────────────┬─────────────────────────┘
                                             │
                              ┌──────────────▼──────────────┐
                              │         Redis 7              │
                              │   (daily/monthly report      │
                              │    cache + Polly breaker)    │
                              └─────────────────────────────┘
```

---

## Services

### 1. Transaction API (`FinancialBalance.Api`)
- **Responsibility**: Account management and transaction writes (create, cancel). All domain mutations go through this service.
- **Technology**: ASP.NET Core 8 Web API — `api/v1/accounts`, `api/v1/transactions`
- **Scaling**: Kubernetes HPA, CPU target 70%, 2–20 replicas
- **Persistence**: PostgreSQL `finance` schema via EF Core (`AppDbContext`)
- **Events**: Domain events raised by aggregates are persisted to an `outbox_messages` table and published to RabbitMQ by `OutboxProcessor` every 2 seconds in batches of 500
- **Auth**: JWT Bearer — roles `finance.admin`, `finance.operator`, `finance.viewer`, `service.erp`
- **Rate limit**: 5,000 req/min sliding window (queue: 100)

### 2. Reporting API (`FinancialBalance.ReportingApi`)
- **Responsibility**: Serves pre-computed daily and monthly financial reports
- **Technology**: ASP.NET Core 8 Web API — `api/v1/reports`
- **Scaling**: Kubernetes HPA, CPU target 60%, 2–10 replicas
- **Persistence**: PostgreSQL `reporting` schema (read-only) + Redis cache with Polly circuit breaker
- **Pattern**: CQRS read side — queries only, no writes
- **Rate limit**: 2,000 req/min sliding window (queue: 50)

### 3. Reporting Worker (`FinancialBalance.Worker`)
- **Responsibility**: Consumes domain events and materializes `DailySummary` and `MonthlySummary` read models; runs nightly rollup and cleanup background jobs
- **Technology**: .NET 8 Worker SDK (`IHostedService`)
- **Scaling**: KEDA ScaledObject driven by RabbitMQ queue depth, 1–10 pods
- **Consumers**: `TransactionCreatedConsumer` (limit 50), `TransactionCancelledConsumer` (limit 25) — both with MassTransit retry (5s / 15s / 30s)
- **Jobs**: `MonthlyRollupJob` (daily 00:05 UTC), `DailySummaryCleanupJob` (configurable retention)

---

## Architecture Patterns

| Pattern | Applied To | Reason |
|---|---|---|
| Clean Architecture | All services | Domain → Application → Infrastructure → API layering; testability |
| CQRS | Transaction write / Reporting read | Separate scaling, different consistency and persistence needs |
| Outbox Pattern | Transaction API | Exactly-once event delivery guarantee without distributed transactions |
| Domain Events | `Account` aggregate | Decoupled side-effect dispatch (balance updated, outbox written) |
| Event-Driven | Transaction API → Worker | Async read-model projection; no direct write coupling |
| Repository | All data access | Abstracts EF Core; enables InMemory substitution in tests |
| Mediator (CQRS) | Application layer | MediatR — all commands and queries go through `IMediator` |
| Validation Behavior | MediatR pipeline | FluentValidation runs before every command handler; returns 400 on failure |

---

## C4 Context Diagram

```
┌────────────────────────────────────────────────────────────────────┐
│                        System Context                               │
│                                                                     │
│   [Finance Team] ──────► [Financial Balance System] ◄─── [ERP]    │
│         │                         │                         │      │
│    (accounts,               (domain events,           (service.erp │
│    transactions,             reports, health)           JWT role)  │
│    reports)                                                         │
└────────────────────────────────────────────────────────────────────┘
```

### External Actors
| Actor | Role |
|---|---|
| Finance Team | Creates accounts, registers/cancels transactions, views reports via UI/API |
| ERP System | Pushes transactions via `service.erp` JWT role (service-to-service) |
| Kubernetes / Prometheus | Scrapes `/metrics` (Prometheus), calls `/health/live` and `/health/ready` |

---

## Data Flow

### Transaction Write Path
```
Client
  → API Gateway
  → Transaction API (JWT auth → FluentValidation → MediatR handler)
  → Account aggregate (RegisterTransaction → raises domain events)
  → PostgreSQL: UPDATE accounts + INSERT transactions + INSERT outbox_messages
  → OutboxProcessor (every 2s): reads outbox → publishes to RabbitMQ
  → TransactionCreatedConsumer (Worker)
      → UpsertAsync(DailySummary) in reporting schema
      → RemoveAsync(Redis cache key)
```

### Report Read Path
```
Client
  → API Gateway
  → Reporting API (JWT auth → MediatR query handler)
  → Redis cache (hit?) → return cached response
  → PostgreSQL reporting schema (miss) → cache result → return response
```

### Nightly Rollup Path
```
MonthlyRollupJob (00:05 UTC)
  → GetRangeAsync(all accounts, prev month)
  → For each account: ComputeFrom(dailySummaries)
  → UpsertAsync(MonthlySummary)
  → RemoveAsync(Redis monthly cache key)
```

---

## Project Structure

```
FinancialBalance.sln
├── src/
│   ├── FinancialBalance.Domain                     # Aggregates, domain events, interfaces
│   ├── FinancialBalance.Application                # CQRS handlers, validators, DTOs
│   ├── FinancialBalance.Infrastructure             # Write-side EF Core, outbox, MassTransit
│   ├── FinancialBalance.Api                        # Transaction API host
│   ├── FinancialBalance.ReportingInfrastructure    # Read-side EF Core, Redis, consumers
│   ├── FinancialBalance.ReportingApi               # Reporting API host
│   └── FinancialBalance.Worker                     # Worker host (consumers + jobs)
│
├── tests/
│   ├── FinancialBalance.Domain.Tests               # 43 unit tests
│   ├── FinancialBalance.Application.Tests          # 42 unit tests
│   ├── FinancialBalance.Worker.Tests               # 13 unit tests
│   └── FinancialBalance.Api.IntegrationTests       # 44 integration tests (WebApplicationFactory)
│
├── infra/sql/                                      # PostgreSQL init scripts
├── docker-compose.yml
├── README.md
├── future-implementations.md
└── docs/
    ├── 01-architecture.md      ← this file
    ├── diagrams.md             # Mermaid diagrams (context, sequence, class, dependency)
    └── ...
```

---

## Cross-Cutting Concerns

| Concern | Implementation |
|---|---|
| Authentication | JWT Bearer — `Auth:Authority` / `Auth:Audience` from config |
| Authorization | Role-based policies: `CanManageAccounts`, `CanWriteTransactions`, `CanViewReports` |
| Validation | FluentValidation + `ValidationBehavior` MediatR pipeline — returns 400 with `errors` extension |
| Error handling | Global exception handler → RFC 7807 `ProblemDetails` (`application/problem+json`) |
| Structured logging | Serilog — console sink, `ReadFrom.Configuration` |
| Distributed tracing | OpenTelemetry — ASP.NET Core + EF Core instrumentation |
| Metrics | Prometheus scraping at `/metrics` |
| Health checks | `/health/live` (always 200) · `/health/ready` (Postgres + Redis + RabbitMQ) |
| Concurrency | `RowVersion` concurrency token on `Account` (Postgres `xmin`) — 409 on conflict |
| Cache resilience | Polly circuit breaker on Redis: 50% failure rate, 30s window, 15s break |

---

## Deployment Model

- **Container runtime**: Docker / Docker Compose (local)
- **Orchestration**: Kubernetes — EKS / GKE / AKS
- **Auto-scaling**:
  - HPA for Transaction API and Reporting API pods (CPU/memory)
  - KEDA for Reporting Worker (RabbitMQ queue depth)
  - Cluster Autoscaler for node-level scaling
- **Environments**: `dev` → `staging` → `production`
- **CI/CD**: GitHub Actions → Docker Build → Helm deploy

---

## Technology Summary

| Layer | Technology |
|---|---|
| Runtime | .NET 8 / C# 12 |
| Web framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 + Npgsql |
| Database | PostgreSQL 16 |
| Cache | Redis 7 (StackExchange.Redis) |
| Cache resilience | Polly 8 circuit breaker |
| Message broker | RabbitMQ 3.13 (MassTransit 8) |
| Mediator / CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Authentication | JWT Bearer (ASP.NET Core) |
| Structured logging | Serilog |
| Observability | OpenTelemetry + Prometheus |
| Unit testing | xUnit, NSubstitute, FluentAssertions |
| Integration testing | xUnit, WebApplicationFactory, EF Core InMemory, MassTransit test harness |
| Containerization | Docker + Docker Compose |
| Orchestration | Kubernetes + Helm |
| Auto-scaling | HPA + KEDA |
