# Financial Balance System

A .NET 8 financial management system built with Clean Architecture, CQRS, and an event-driven microservices approach. It handles account management, transaction processing, and financial reporting across three independent services.

---

## Architecture Overview

```
┌─────────────────────┐     ┌──────────────────────┐
│  Transaction API    │     │   Reporting API       │
│  :5000              │     │   :5001               │
│  FinancialBalance   │     │  FinancialBalance     │
│  .Api               │     │  .ReportingApi        │
└────────┬────────────┘     └──────────┬────────────┘
         │ writes                      │ reads
         ▼                             ▼
    PostgreSQL ◄──────────────── PostgreSQL
         │                        (same DB,
         │ domain events           read side)
         ▼
    RabbitMQ ──────► Reporting Worker
                     FinancialBalance.Worker
                     (consumers + rollup jobs)
                              │
                              ▼
                           Redis
                      (report cache)
```

- **Transaction API** handles account and transaction writes. Domain events are persisted to an outbox table and published to RabbitMQ via a background outbox processor.
- **Reporting Worker** consumes events and materializes `DailySummary` and `MonthlySummary` read models. Two background jobs run nightly: a monthly rollup and a daily summary cleanup.
- **Reporting API** serves report queries from the read models, with Redis caching and a Polly circuit breaker for cache resilience.

All three services share one PostgreSQL database but maintain separate DbContexts (`AppDbContext`, `WorkerDbContext`, `ReportingDbContext`).

---

## Technology Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 8 |
| Web framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 + Npgsql |
| Database | PostgreSQL 16 |
| Message broker | RabbitMQ 3.13 (MassTransit) |
| Cache | Redis 7 (StackExchange.Redis) |
| Cache resilience | Polly 8 circuit breaker |
| Mediator / CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Authentication | JWT Bearer (ASP.NET Core) |
| Structured logging | Serilog |
| Observability | OpenTelemetry (traces + metrics), Prometheus |
| Unit testing | xUnit, NSubstitute, FluentAssertions |
| Integration testing | xUnit, WebApplicationFactory, EF Core InMemory, MassTransit test harness |

---

## Project Structure

```
FinancialBalance.sln
├── src/
│   ├── FinancialBalance.Domain                 # Aggregates, entities, domain events, interfaces
│   │   ├── Accounts/                           # Account aggregate, Transaction entity
│   │   │   └── Events/                         # TransactionCreated, TransactionCancelled, AccountBalanceUpdated
│   │   ├── Reporting/                          # DailySummary, MonthlySummary, CategoryBreakdown
│   │   └── Shared/                             # AggregateRoot, IDomainEvent, IRepository<T>
│   │
│   ├── FinancialBalance.Application             # Use cases (CQRS handlers, validators, DTOs)
│   │   ├── Accounts/Commands/                  # CreateAccount
│   │   ├── Accounts/Queries/                   # GetAccount, GetAccountBalance, ListAccounts
│   │   ├── Transactions/Commands/              # CreateTransaction, CancelTransaction
│   │   ├── Transactions/Queries/               # GetTransaction, ListTransactions
│   │   ├── Reports/Queries/                    # GetDailyReport, GetDailyReportRange,
│   │   │                                       # GetMonthlyReport, GetMonthlyReportRange
│   │   └── Common/                             # ValidationBehavior, ICurrentUser, IReportCache, exceptions
│   │
│   ├── FinancialBalance.Infrastructure          # Write-side infrastructure (Transaction API)
│   │   ├── Persistence/                        # AppDbContext, migrations, outbox
│   │   │   ├── Configurations/                 # AccountConfiguration, TransactionConfiguration,
│   │   │   │                                   # OutboxMessageConfiguration
│   │   │   ├── Repositories/                   # AccountRepository (bounded transaction loading)
│   │   │   ├── OutboxMessage.cs
│   │   │   └── OutboxProcessor.cs              # Background service: publishes domain events every 2s
│   │   ├── Auth/                               # CurrentUser (JWT claim extraction)
│   │   └── ServiceCollectionExtensions.cs      # EF Core (pool=50), MassTransit, outbox registration
│   │
│   ├── FinancialBalance.Api                     # Transaction API host
│   │   ├── Controllers/                        # AccountsController, TransactionsController
│   │   └── Program.cs                          # Serilog, MediatR, JWT, rate limiter (5000/min),
│   │                                           # health checks, OpenTelemetry, Swagger
│   │
│   ├── FinancialBalance.ReportingInfrastructure # Read-side infrastructure (Reporting API + Worker)
│   │   ├── Cache/                              # RedisReportCache (Polly circuit breaker)
│   │   ├── Messaging/                          # TransactionCreatedConsumer, TransactionCancelledConsumer,
│   │   │                                       # MonthlyRollupJob (BackgroundService, runs 00:05 UTC)
│   │   ├── Persistence/                        # ReportingDbContext, DailySummaryRepository,
│   │   │   └── Configurations/                 # MonthlySummaryRepository, entity configurations
│   │   ├── Auth/                               # CurrentUser
│   │   └── ServiceCollectionExtensions.cs      # EF Core, Redis (retry + timeout), MassTransit consumers
│   │
│   ├── FinancialBalance.ReportingApi            # Reporting API host
│   │   ├── Controllers/                        # ReportsController
│   │   └── Program.cs                          # Serilog, MediatR, JWT, rate limiter (2000/min),
│   │                                           # health checks, OpenTelemetry, Swagger
│   │
│   └── FinancialBalance.Worker                  # Reporting Worker host
│       ├── Consumers/                          # TransactionCreatedConsumer (limit=50),
│       │                                       # TransactionCancelledConsumer (limit=25)
│       ├── Jobs/                               # MonthlyRollupJob, DailySummaryCleanupJob
│       ├── Infrastructure/
│       │   ├── Cache/                          # RedisReportCache (Polly circuit breaker)
│       │   └── Persistence/                    # WorkerDbContext, repositories, configurations
│       └── Program.cs                          # Worker SDK host, connection string pool config
│
├── tests/
│   ├── FinancialBalance.Domain.Tests            # 43 tests — aggregates, domain rules, events
│   ├── FinancialBalance.Application.Tests       # 42 tests — command/query handlers, validators
│   ├── FinancialBalance.Worker.Tests            # 13 tests — consumers, background jobs
│   └── FinancialBalance.Api.IntegrationTests    # 44 tests — full HTTP stack via WebApplicationFactory
│
├── infra/
│   └── sql/                                    # PostgreSQL init scripts
│
├── docker-compose.yml                          # Full local stack
└── future-implementations.md                  # Backlog: medium bugs, performance, security items
```

---

## API Endpoints

### Transaction API (`localhost:5000`)

| Method | Path | Policy | Description |
|---|---|---|---|
| `POST` | `/api/v1/accounts` | `CanManageAccounts` | Create a new account |
| `GET` | `/api/v1/accounts` | `CanViewReports` | List accounts (paginated) |
| `GET` | `/api/v1/accounts/{id}` | `CanViewReports` | Get account by ID |
| `GET` | `/api/v1/accounts/{id}/balance` | `CanViewReports` | Get current balance |
| `POST` | `/api/v1/transactions` | `CanWriteTransactions` | Register a transaction |
| `GET` | `/api/v1/transactions` | `CanViewReports` | List transactions (filtered, paginated) |
| `GET` | `/api/v1/transactions/{id}` | `CanViewReports` | Get transaction by ID |
| `PATCH` | `/api/v1/transactions/{id}/cancel` | `CanWriteTransactions` | Cancel a transaction |
| `GET` | `/health/live` | — | Liveness probe |
| `GET` | `/health/ready` | — | Readiness probe (Postgres, Redis, RabbitMQ) |
| `GET` | `/metrics` | — | Prometheus scraping endpoint |

### Reporting API (`localhost:5001`)

| Method | Path | Policy | Description |
|---|---|---|---|
| `GET` | `/api/v1/reports/{accountId}/daily/{date}` | `CanViewReports` | Daily summary for a specific date |
| `GET` | `/api/v1/reports/{accountId}/daily` | `CanViewReports` | Daily summaries for a date range |
| `GET` | `/api/v1/reports/{accountId}/monthly/{year}/{month}` | `CanViewReports` | Monthly summary |
| `GET` | `/api/v1/reports/{accountId}/monthly` | `CanViewReports` | Monthly summaries for a range |
| `GET` | `/health/live` | — | Liveness probe |
| `GET` | `/health/ready` | — | Readiness probe (Postgres, Redis, RabbitMQ) |
| `GET` | `/metrics` | — | Prometheus scraping endpoint |

---

## Authorization Roles

| Role | Permissions |
|---|---|
| `finance.admin` | Full access: manage accounts, write transactions, view reports |
| `finance.operator` | Write transactions, view reports |
| `finance.viewer` | View reports only |
| `service.erp` | Write transactions (service-to-service) |

JWT Bearer tokens are validated against the authority configured in `Auth:Authority`. Configure via environment variable or `appsettings.json`.

---

## Rate Limiting

Both APIs use a sliding window rate limiter with 6 segments per window.

| Service | Limit | Window | Queue |
|---|---|---|---|
| Transaction API | 5,000 requests | 1 minute | 100 |
| Reporting API | 2,000 requests | 1 minute | 50 |

Requests exceeding the limit return `HTTP 429 Too Many Requests`.

---

## Background Services

| Service | Host | Schedule | Description |
|---|---|---|---|
| `OutboxProcessor` | Transaction API | Every 2 seconds | Publishes unprocessed domain events from the outbox table to RabbitMQ in batches of 500 |
| `MonthlyRollupJob` | Reporting Worker | Daily at 00:05 UTC | Rolls up daily summaries into monthly summaries for all accounts |
| `MonthlyRollupJob` | Reporting API | Daily at 00:05 UTC | Fallback rollup for dev/staging (replaced by Kubernetes CronJob in production) |
| `DailySummaryCleanupJob` | Reporting Worker | Configurable | Purges old daily summaries beyond the retention window |

---

## Configuration

### Environment Variables

All three services share these environment variables:

| Variable | Example | Description |
|---|---|---|
| `ConnectionStrings__Postgres` | `Host=postgres;Database=financialbalance;Username=app;Password=...` | PostgreSQL connection string |
| `ConnectionStrings__Redis` | `redis:6379` | Redis connection string |
| `RabbitMQ__Host` | `rabbitmq` | RabbitMQ host |
| `RabbitMQ__Username` | `guest` | RabbitMQ username |
| `RabbitMQ__Password` | `guest` | RabbitMQ password |
| `RabbitMQ__Uri` | `amqp://guest:guest@rabbitmq:5672` | Full AMQP URI (used by health checks) |
| `Auth__Authority` | `https://your-idp.example.com` | OIDC authority for JWT validation |
| `Auth__Audience` | `financial-balance-api` | Expected JWT audience claim |

### Database Pool (Transaction API and Worker)

The EF Core pool is configured with:

- `MaxPoolSize = 50`
- `CommandTimeout = 10s`
- `EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: 5s)`

### Redis Resilience (Reporting API and Worker)

StackExchange.Redis is configured with:

- `ConnectTimeout = 5000ms`
- `SyncTimeout = 5000ms`
- `AbortOnConnectFail = false`
- Exponential retry: 1s–10s

A Polly circuit breaker wraps all cache operations:

- Opens after 50% failure rate over a 5-minimum-throughput, 30s sampling window
- Stays open for 15s before attempting to close

When the circuit is open, cache reads return `null` (cache miss fallback) and writes are silently dropped — the system continues serving from the database.

### Optimistic Concurrency

The `Account` aggregate uses a `RowVersion` (PostgreSQL `xmin`) concurrency token configured in `AccountConfiguration`. Concurrent writes to the same account raise `DbUpdateConcurrencyException` and are rejected with `HTTP 409 Conflict`.

---

## Running Locally

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK (for running tests or building outside Docker)

### Start the full stack

```bash
docker-compose up --build
```

Services available after startup:

| Service | URL |
|---|---|
| Transaction API | http://localhost:5000 |
| Transaction API Swagger | http://localhost:5000/swagger |
| Reporting API | http://localhost:5001 |
| Reporting API Swagger | http://localhost:5001/swagger |
| RabbitMQ Management | http://localhost:15672 (guest / guest) |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |

### Run tests

```bash
export PATH="$PATH:$HOME/.dotnet"
dotnet test FinancialBalance.sln
```

Current test results: **142 tests, 0 failures** across 4 test projects.

| Project | Tests | Description |
|---|---|---|
| `FinancialBalance.Domain.Tests` | 43 | Aggregate rules, domain events, value objects |
| `FinancialBalance.Application.Tests` | 42 | CQRS handlers, validators, pipeline behaviors |
| `FinancialBalance.Worker.Tests` | 13 | Consumers, rollup jobs, summary projections |
| `FinancialBalance.Api.IntegrationTests` | 44 | Full HTTP stack via `WebApplicationFactory` — accounts, transactions, outbox, domain rules |

Integration tests use EF Core InMemory (replacing Postgres), MassTransit test harness (replacing RabbitMQ), and a header-based `TestAuthHandler` (replacing JWT). No external services are required to run the integration tests.

---

## Domain Model

### Account

- `AccountType`: `Checking`, `Savings`, `Investment`, `Credit`
- `Currency`: `BRL`, `USD`, `EUR`
- `IsActive`: inactive accounts reject new transactions
- `CurrentBalance`: updated on every `RegisterTransaction` and `CancelTransaction`

### Transaction

- `TransactionType`: `Incoming`, `Outgoing`
- `TransactionCategory`: `Revenue`, `Expense`, `Transfer`, `Supplier`, `Tax`, `Payroll`, `Other`
- `TransactionStatus`: `Confirmed`, `Cancelled`
- Amount validated > 0 at creation

### DailySummary

Materialized per `(AccountId, Date)` by the Reporting Worker. Holds `TotalIncoming`, `TotalOutcoming`, `NetFlow`, `ClosingBalance`, and a `CategoryBreakdowns` collection.

### MonthlySummary

Rolled up from daily summaries once per night. Holds `TotalIncoming`, `TotalOutcoming`, `NetFlow`, `OpeningBalance`, `ClosingBalance`, and `CategoryBreakdowns`.

---

## Event Flow

```
CreateTransaction command
        │
        ▼
Account.RegisterTransaction()
        │ raises
        ▼
TransactionCreated (domain event)
        │ persisted to outbox table
        ▼
OutboxProcessor (every 2s)
        │ publishes to RabbitMQ
        ▼
TransactionCreatedConsumer (Worker)
        │
        ├── GetAsync(accountId, date)  ← load or create DailySummary
        ├── ApplyTransaction(type, amount, category)
        └── UpsertAsync(summary)  ← persist updated summary
```

Cancellation follows the same path via `TransactionCancelled`, carrying the original transaction date and category to ensure the correct `DailySummary` row is reversed.

---

## Observability

- **Traces**: ASP.NET Core + EF Core instrumentation via OpenTelemetry
- **Metrics**: ASP.NET Core instrumentation exported to Prometheus at `/metrics`
- **Logs**: Serilog structured logging, console sink, configurable via `appsettings.json`
- **Health checks**: `/health/live` (always 200) and `/health/ready` (checks Postgres, Redis, RabbitMQ)

---

## Repository

GitHub: https://github.com/josecaferreira/financial-balance-system

---

## Author

Jose Carlos Ferreira — Software Architect
