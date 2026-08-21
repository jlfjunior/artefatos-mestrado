# 02 — Technology Stack

## Runtime & Language

| Component | Choice | Justification |
|---|---|---|
| Runtime | .NET 8 (LTS) | Long-term support until Nov 2026, best performance in .NET history |
| Language | C# 12 | Primary patterns: records, primary constructors, collection expressions |
| Hosting | ASP.NET Core 8 | Minimal APIs + Controllers, built-in DI, Kestrel |

---

## Backend Libraries

### Core
| Library | Version | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 8.x | ORM for all data access (write path and read path) |
| `MassTransit` | 8.x | RabbitMQ abstraction, message routing, retry policies |
| `MediatR` | 12.x | CQRS mediator for commands/queries/domain events |
| `FluentValidation` | 11.x | Input validation for API requests via MediatR pipeline behavior |

### Resilience
| Library | Version | Purpose |
|---|---|---|
| `Polly` | 8.x | Circuit breaker on Redis (50% failure rate, 30s window, 15s break) |

### Auth & Security
| Library | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x | JWT token validation |

### Observability
| Library | Version | Purpose |
|---|---|---|
| `OpenTelemetry.Extensions.Hosting` | 1.x | Tracing + metrics instrumentation |
| `OpenTelemetry.Exporter.Prometheus.AspNetCore` | 1.x | Prometheus metrics endpoint at `/metrics` |
| `Serilog.AspNetCore` | 8.x | Structured logging — console sink, `ReadFrom.Configuration` |

---

## Data Layer

| Technology | Version | Role |
|---|---|---|
| PostgreSQL | 16 | Primary relational database — `finance` and `reporting` schemas |
| Redis | 7 | Report caching (daily/monthly) + Polly circuit breaker |
| RabbitMQ | 3.13 | Async messaging — `TransactionCreated` / `TransactionCancelled` events |

**PostgreSQL** was chosen over SQL Server for:
- Open-source, no licensing cost
- Excellent JSON support (`JSONB` for outbox payloads)
- Native support for `NUMERIC` type for financial precision

---

## Infrastructure

| Technology | Role |
|---|---|
| Docker | Container runtime, local dev with docker-compose |
| Kubernetes | Production orchestration (EKS / GKE / AKS) |
| Helm | Kubernetes package manager |
| KEDA | Event-driven autoscaling for Reporting Worker (queue depth) |
| NGINX Ingress | API gateway / reverse proxy |
| cert-manager | Automatic TLS certificate management |

---

## CI/CD & Tooling

| Tool | Purpose |
|---|---|
| GitHub Actions | CI/CD pipelines |
| Docker Hub / ECR | Container registry |
| SonarQube | Static code analysis |
| Trivy | Container image vulnerability scanning |
| Prometheus + Grafana | Metrics and dashboards |
| Jaeger | Distributed tracing UI |

---

## Development & Testing Tools

| Tool | Purpose |
|---|---|
| `dotnet-ef` CLI | EF Core migrations |
| `xUnit` | Unit and integration testing |
| `NSubstitute` | Mocking framework for unit tests |
| `FluentAssertions` | Expressive test assertions |
| `WebApplicationFactory` | In-process integration test host |
| `MassTransit.Testing` | MassTransit test harness (replaces RabbitMQ in tests) |
| `EF Core InMemory` | In-memory DB provider for integration tests |
| Swagger / Scalar | API documentation UI |

---

## Dependency Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        .NET 8 Application                        │
│                                                                   │
│  ASP.NET Core 8                                                   │
│    ├── MediatR (CQRS dispatch)                                    │
│    ├── FluentValidation (ValidationBehavior in pipeline)          │
│    ├── JWT Bearer Auth                                            │
│    └── OpenTelemetry (traces + metrics)                           │
│                                                                   │
│  Data Access                                                      │
│    ├── EF Core 8 → PostgreSQL finance schema (write path)         │
│    ├── EF Core 8 → PostgreSQL reporting schema (read path)        │
│    └── StackExchange.Redis → Redis (cache + Polly circuit breaker)│
│                                                                   │
│  Messaging                                                        │
│    └── MassTransit → RabbitMQ                                     │
│                                                                   │
│  Cross-cutting                                                    │
│    ├── Serilog (structured logs)                                  │
│    └── Polly (Redis circuit breaker)                              │
└─────────────────────────────────────────────────────────────────┘
```
