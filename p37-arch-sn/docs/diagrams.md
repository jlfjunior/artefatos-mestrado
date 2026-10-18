# Architecture Diagrams

---

## 1. System Context — Services and Infrastructure

High-level view of all running processes and how they connect to shared infrastructure.

```mermaid
graph TB
    Client(["Client / ERP System"])

    subgraph Services["Application Services"]
        TxAPI["Transaction API\n:5000\nFinancialBalance.Api"]
        RepAPI["Reporting API\n:5001\nFinancialBalance.ReportingApi"]
        Worker["Reporting Worker\nFinancialBalance.Worker"]
    end

    subgraph Infrastructure["Infrastructure"]
        PG[("PostgreSQL 16\n:5432\nfinancialbalance")]
        RMQ["RabbitMQ 3.13\n:5672 / :15672"]
        Redis["Redis 7\n:6379"]
    end

    Client -->|"HTTP — accounts\ntransactions"| TxAPI
    Client -->|"HTTP — reports"| RepAPI

    TxAPI -->|"writes: finance schema\n(accounts, transactions, outbox)"| PG
    TxAPI -->|"publishes domain events\nvia OutboxProcessor"| RMQ

    RepAPI -->|"reads: reporting schema\n(daily/monthly summaries)"| PG
    RepAPI -->|"cache read/write\nPolly circuit breaker"| Redis

    Worker -->|"reads/writes: reporting schema\n(daily/monthly summaries)"| PG
    Worker -->|"cache invalidation"| Redis
    Worker -->|"consumes: TransactionCreated\nTransactionCancelled"| RMQ
```

---

## 2. Transaction Write Flow — Command to Event

Detailed sequence from an incoming HTTP request through the domain, outbox, and message broker to the read-model projection.

```mermaid
sequenceDiagram
    participant Client
    participant TxAPI as Transaction API
    participant Handler as CreateTransaction\nCommandHandler
    participant Domain as Account\n(Aggregate)
    participant DB as PostgreSQL\n(finance schema)
    participant Outbox as OutboxProcessor\n(BackgroundService)
    participant RMQ as RabbitMQ
    participant Consumer as TransactionCreated\nConsumer (Worker)
    participant RepDB as PostgreSQL\n(reporting schema)
    participant Cache as Redis

    Client->>TxAPI: POST /api/v1/transactions
    TxAPI->>Handler: CreateTransactionCommand
    Handler->>DB: GetByIdAsync(accountId) — tracked
    DB-->>Handler: Account
    Handler->>Domain: RegisterTransaction(type, amount, category, date)
    Domain->>Domain: update CurrentBalance
    Domain->>Domain: add Transaction to _transactions
    Domain->>Domain: raise TransactionCreated event
    Domain->>Domain: raise AccountBalanceUpdated event
    Handler->>DB: Entry(account).State=Modified + SaveChangesAsync
    Note over DB: Persists Account UPDATE + Transaction INSERT\n+ OutboxMessage rows (one per event)
    DB-->>TxAPI: OK
    TxAPI-->>Client: 201 TransactionDto

    loop Every 2 seconds
        Outbox->>DB: SELECT top 500 WHERE ProcessedAt IS NULL
        DB-->>Outbox: OutboxMessage[]
        Outbox->>RMQ: Publish(TransactionCreated)
        Outbox->>RMQ: Publish(AccountBalanceUpdated)
        Outbox->>DB: SET ProcessedAt = NOW()
    end

    RMQ->>Consumer: TransactionCreated message
    Consumer->>RepDB: GetAsync(accountId, date)
    alt DailySummary exists
        RepDB-->>Consumer: DailySummary
    else first transaction of the day
        Consumer->>Consumer: DailySummary.Create(accountId, date)
    end
    Consumer->>Consumer: summary.ApplyTransaction(type, amount, category)
    Consumer->>RepDB: UpsertAsync(summary)
    Consumer->>Cache: RemoveAsync("daily:{accountId}:{date}")
```

---

## 3. Domain Model — Classes and Relationships

Class structure of the domain layer including aggregates, entities, value objects, events, and interfaces.

```mermaid
classDiagram
    class AggregateRoot {
        +Guid Id
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +IReadOnlyList~IDomainEvent~ DomainEvents
        #RaiseDomainEvent(IDomainEvent)
        +ClearDomainEvents()
        #SetUpdated()
    }

    class Account {
        +string Name
        +string Code
        +AccountType Type
        +Currency Currency
        +decimal CurrentBalance
        +bool IsActive
        +IReadOnlyCollection~Transaction~ Transactions
        +Create(name, code, type, currency)$
        +RegisterTransaction(type, amount, desc, category, date, createdBy) Transaction
        +CancelTransaction(transactionId)
        +Deactivate()
    }

    class Transaction {
        +Guid AccountId
        +TransactionType Type
        +decimal Amount
        +string Description
        +TransactionCategory Category
        +string? ReferenceNumber
        +TransactionStatus Status
        +DateOnly TransactionDate
        +Guid CreatedBy
        +Create(accountId, type, amount, ...) Transaction$
        +Cancel()
    }

    class DailySummary {
        +Guid AccountId
        +DateOnly Date
        +decimal TotalIncoming
        +decimal TotalOutgoing
        +decimal NetBalance
        +int TransactionCount
        +DateTime ComputedAt
        +IReadOnlyCollection~CategoryBreakdown~ CategoryBreakdowns
        +Create(accountId, date)$
        +ApplyTransaction(type, amount, category)
        +ReverseTransaction(type, amount, category)
    }

    class MonthlySummary {
        +Guid AccountId
        +int Year
        +int Month
        +decimal OpeningBalance
        +decimal ClosingBalance
        +decimal TotalIncoming
        +decimal TotalOutgoing
        +decimal NetBalance
        +int TransactionCount
        +DateTime ComputedAt
        +IReadOnlyCollection~CategoryBreakdown~ CategoryBreakdowns
        +ComputeFrom(accountId, year, month, openingBalance, dailies)$
    }

    class CategoryBreakdown {
        +string Category
        +decimal TotalIncoming
        +decimal TotalOutgoing
    }

    class TransactionCreated {
        +Guid TransactionId
        +Guid AccountId
        +TransactionType Type
        +decimal Amount
        +DateTime TransactionDate
        +TransactionCategory Category
    }

    class TransactionCancelled {
        +Guid TransactionId
        +Guid AccountId
        +decimal Amount
        +TransactionType OriginalType
        +DateTime OriginalTransactionDate
        +TransactionCategory OriginalCategory
    }

    class AccountBalanceUpdated {
        +Guid AccountId
        +decimal NewBalance
        +decimal PreviousBalance
        +DateTime UpdatedAt
    }

    class IDomainEvent {
        <<interface>>
    }

    AggregateRoot <|-- Account
    AggregateRoot <|-- DailySummary
    AggregateRoot <|-- MonthlySummary
    Account "1" *-- "0..*" Transaction : owns
    DailySummary "1" *-- "0..*" CategoryBreakdown : breakdown
    MonthlySummary "1" *-- "0..*" CategoryBreakdown : breakdown
    IDomainEvent <|.. TransactionCreated
    IDomainEvent <|.. TransactionCancelled
    IDomainEvent <|.. AccountBalanceUpdated
    Account ..> TransactionCreated : raises
    Account ..> TransactionCancelled : raises
    Account ..> AccountBalanceUpdated : raises
```

---

## 4. Layered Architecture — Project Dependencies

Clean Architecture dependency flow across all projects in the solution.

```mermaid
graph TB
    subgraph Domain["Domain Layer"]
        D["FinancialBalance.Domain\n\nAggregates, Entities\nDomain Events\nIRepository&lt;T&gt;\nIDailySummaryRepository\nIMonthlySummaryRepository"]
    end

    subgraph Application["Application Layer"]
        A["FinancialBalance.Application\n\nCQRS Handlers\nFluentValidation validators\nValidationBehavior pipeline\nDTOs, ICurrentUser, IReportCache\nNotFoundException, DomainException"]
    end

    subgraph Infrastructure["Infrastructure Layer"]
        I["FinancialBalance.Infrastructure\n\nAppDbContext (EF Core)\nAccountRepository\nOutboxProcessor\nCurrentUser (JWT)\nMassTransit (publisher)\nServiceCollectionExtensions"]

        RI["FinancialBalance.ReportingInfrastructure\n\nReportingDbContext (EF Core)\nDailySummaryRepository\nMonthlySummaryRepository\nRedisReportCache + Polly\nTransactionCreated/CancelledConsumer\nMonthlyRollupJob\nServiceCollectionExtensions"]

        WI["FinancialBalance.Worker (infra)\n\nWorkerDbContext (EF Core)\nDailySummaryRepository\nMonthlySummaryRepository\nRedisReportCache + Polly\nTransactionCreated/CancelledConsumer\nMonthlyRollupJob\nDailySummaryCleanupJob"]
    end

    subgraph Hosts["Host / Presentation Layer"]
        H1["FinancialBalance.Api\n\nAccountsController\nTransactionsController\nJWT auth, rate limiter\nOpenTelemetry, Serilog\nSwagger, health checks"]

        H2["FinancialBalance.ReportingApi\n\nReportsController\nJWT auth, rate limiter\nOpenTelemetry, Serilog\nSwagger, health checks"]

        H3["FinancialBalance.Worker\n\nWorker SDK host\nProgram.cs wiring\nOpenTelemetry, Serilog"]
    end

    subgraph Tests["Test Projects"]
        T1["Domain.Tests\n36 tests"]
        T2["Application.Tests\n39 tests"]
        T3["Worker.Tests\n13 tests"]
        T4["Api.IntegrationTests\n44 tests\nWebApplicationFactory\nEF InMemory + MassTransit harness"]
    end

    A --> D
    I --> A
    I --> D
    RI --> A
    RI --> D
    WI --> A
    WI --> D

    H1 --> I
    H1 --> A
    H2 --> RI
    H2 --> A
    H3 --> WI

    T1 --> D
    T2 --> A
    T2 --> D
    T3 --> WI
    T3 --> D
    T4 --> H1
    T4 --> I
    T4 --> A
```

---

## 5. Reporting Pipeline — Nightly Rollup

How daily summaries are rolled up into monthly summaries by the background job.

```mermaid
sequenceDiagram
    participant Job as MonthlyRollupJob\n(00:05 UTC daily)
    participant DailyRepo as IDailySummaryRepository
    participant MonthlyRepo as IMonthlySummaryRepository
    participant Cache as IReportCache (Redis)

    Note over Job: Wakes up, computes yesterday's month
    Job->>DailyRepo: GetRangeAsync(Guid.Empty, from, to)
    Note over DailyRepo: Guid.Empty = wildcard: all accounts
    DailyRepo-->>Job: All DailySummary rows for the month

    Job->>Job: Distinct AccountIds from result

    loop For each AccountId
        Job->>DailyRepo: GetRangeAsync(accountId, from, to)
        DailyRepo-->>Job: DailySummary[] for account

        Job->>MonthlyRepo: GetAsync(accountId, prevYear, prevMonth)
        MonthlyRepo-->>Job: prev MonthlySummary (or null)
        Job->>Job: openingBalance = prev.ClosingBalance ?? 0

        Job->>Job: MonthlySummary.ComputeFrom(accountId, year, month,\n  openingBalance, dailies)

        Job->>MonthlyRepo: UpsertAsync(monthly)
        Job->>Cache: RemoveAsync("monthly:{accountId}:{year}:{month}")
    end
```

---

## 6. Infrastructure Configuration Summary

Key configuration values wired at startup across all three services.

```mermaid
graph LR
    subgraph TxAPI_Config["Transaction API — Program.cs"]
        RL1["Rate Limiter\nSlidingWindow\n5000 req/min\n6 segments\nqueue: 100"]
        Auth1["JWT Bearer\nAuth__Authority\nAuth__Audience\nClockSkew: 30s"]
        Roles1["Policies\nCanManageAccounts → finance.admin\nCanWriteTransactions → admin, operator, erp\nCanViewReports → admin, operator, viewer"]
        OT1["OpenTelemetry\nASP.NET Core traces\nEF Core traces\nPrometheus /metrics"]
        HC1["Health Checks\n/health/live\n/health/ready\n(Postgres, Redis, RabbitMQ)"]
    end

    subgraph RepAPI_Config["Reporting API — Program.cs"]
        RL2["Rate Limiter\nSlidingWindow\n2000 req/min\n6 segments\nqueue: 50"]
        Auth2["JWT Bearer\nAuth__Authority\nAuth__Audience\nClockSkew: 30s"]
        Roles2["Policies\nCanViewReports → admin, operator, viewer"]
        OT2["OpenTelemetry\nASP.NET Core traces\nEF Core traces\nPrometheus /metrics"]
    end

    subgraph Shared_Config["Shared Infrastructure Config"]
        PGPool["PostgreSQL Pool\nMaxPoolSize: 50\nCommandTimeout: 10s\nRetry: 3x / 5s backoff"]
        RedisConf["Redis\nConnectTimeout: 5000ms\nSyncTimeout: 5000ms\nAbortOnConnectFail: false\nExponentialRetry: 1s–10s"]
        Polly["Polly Circuit Breaker\nFailureRatio: 50%\nMinThroughput: 5\nSamplingDuration: 30s\nBreakDuration: 15s"]
        Outbox["OutboxProcessor\nInterval: 2s\nBatch: 500 messages"]
        Consumers["MassTransit Consumers\nTransactionCreated: limit 50\nTransactionCancelled: limit 25\nRetry: 5s, 15s, 30s"]
    end
```
