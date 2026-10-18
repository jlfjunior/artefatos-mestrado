# Future Implementations

This document consolidates identified bugs, performance improvements, and security hardening items for the FinancialBalance system. It is intended as a living reference for the development team to prioritize and track implementation work across sprints. Items are categorized by severity and type; each entry includes the affected file(s), a precise description of the problem, and a concrete recommended fix.

---

## Recently Fixed (via integration test suite)

The following production bugs were discovered and fixed during integration test development:

| ID | Description | Fix |
|----|-------------|-----|
| F1 | `AccountRepository.Update` used `DbSet.Update` which propagated `Modified` state to new child Transaction entities, causing `DbUpdateConcurrencyException` on InMemory and silent data loss on Postgres | Changed to `Entry(entity).State = Modified`; removed `AsNoTracking()` from `GetByIdAsync` |
| F2 | `GetTransactionQueryHandler` used `GetByIdAsync` (no Include) so `account.Transactions` was always empty → always 404 | Changed to `GetByIdWithTransactionAsync(accountId, transactionId)` |
| F3 | `CancelTransactionCommandHandler` returned HTTP 422 for an unknown transaction ID (domain exception) instead of HTTP 404 | Added explicit `NotFoundException` check before calling `account.CancelTransaction` |
| F4 | Exception handler set `Content-Type: application/problem+json` then called `WriteAsJsonAsync` which reset it to `application/json` | Changed to `JsonSerializer.SerializeToUtf8Bytes` + `Response.Body.WriteAsync` |

---

## Section 1 — Medium Severity Bugs (to fix)

These issues represent correctness defects that can cause data loss, data duplication, or incorrect query results under production conditions. They should be resolved before the system is considered production-ready.

---

### M1 — Outbox: no dead-letter / infinite retry

**File:** `src/FinancialBalance.Infrastructure/Persistence/OutboxProcessor.cs` lines 38–67

**Issue**

Messages that fail deserialization or publishing remain with `ProcessedAt = null` indefinitely. The processor retries every 2 seconds with no retry counter, no maximum-retry gate, and no dead-letter destination. A single poisoned message will block the queue and consume CPU and DB connections indefinitely without any observable signal.

**Recommended Fix**

Add `RetryCount int` and `ErrorMessage string?` fields to the `OutboxMessage` entity. On each failed attempt, increment `RetryCount` and persist the exception message to `ErrorMessage`. After reaching a configurable maximum (e.g. 5 attempts), mark the record as dead-lettered by setting a `DeadLetteredAt` timestamp instead of leaving `ProcessedAt = null`. Exclude dead-lettered records from the processor query. Log the final failure at `Error` level including the `ErrorMessage` to ensure operational visibility.

---

### M2 — Consumers not idempotent (duplicate message delivery)

**Files:**
- `src/FinancialBalance.Worker/Consumers/TransactionCreatedConsumer.cs`
- `src/FinancialBalance.Worker/Consumers/TransactionCancelledConsumer.cs`
- `src/FinancialBalance.ReportingInfrastructure/Messaging/TransactionCreatedConsumer.cs`
- `src/FinancialBalance.ReportingInfrastructure/Messaging/TransactionCancelledConsumer.cs`

**Issue**

MassTransit retries message delivery on transient network errors. If a message is redelivered after a consumer has already partially processed it, `ApplyTransaction` or `ReverseTransaction` executes a second time, doubling the applied amount. No processed-message-ID check currently exists in any of the four consumers.

**Recommended Fix**

Add a `ProcessedEvents` table with columns `(ConsumerId varchar, MessageId uuid, ProcessedAt timestamptz)` and a unique constraint on `(ConsumerId, MessageId)`. Before applying any domain mutation, query for the existence of the `(ConsumerId, MessageId)` pair. If found, short-circuit and return without re-applying. If not found, insert the pair after the mutation and within the same DB transaction as the `UpsertAsync` call, ensuring atomicity between the idempotency record and the state change.

---

### M3 — Concurrent UpsertAsync causes unique constraint violation

**Files:**
- `src/FinancialBalance.Worker/Infrastructure/Persistence/Repositories/DailySummaryRepository.cs` lines 36–50
- `src/FinancialBalance.ReportingInfrastructure/Persistence/Repositories/DailySummaryRepository.cs` lines 26–37
- `src/FinancialBalance.Worker/Infrastructure/Persistence/Repositories/MonthlySummaryRepository.cs` lines 28–43

**Issue**

The current upsert pattern is a read-then-write: the repository reads the existing row, and if null, calls `AddAsync`. When two consumers process events for the same `(AccountId, Date)` concurrently, both reads return null, both call `AddAsync`, and the second `SaveChangesAsync` throws a unique constraint violation. Because this exception is not handled, the second event's mutation is silently discarded, leaving the summary in an incorrect state.

**Recommended Fix**

Two viable approaches:

1. Add a `RowVersion` concurrency token (or `xmin` column in PostgreSQL) to `DailySummary` and `MonthlySummary`. Wrap `UpsertAsync` in a retry loop that catches `DbUpdateException` and `DbUpdateConcurrencyException`, clears the EF change tracker, and re-reads the row before retrying the mutation.

2. Replace the read-then-write with a PostgreSQL native `INSERT ... ON CONFLICT (AccountId, Date) DO UPDATE SET ...` via `ExecuteSqlRawAsync`. This resolves the race condition atomically at the database level and also addresses the performance improvement described in P1.

---

### M4 — Cache invalidation incomplete after monthly rollup

**Files:**
- `src/FinancialBalance.ReportingInfrastructure/Messaging/MonthlyRollupJob.cs` line 73
- `src/FinancialBalance.Worker/Jobs/MonthlyRollupJob.cs` (same pattern)

**Issue**

After upserting a monthly summary, only the key `monthly:{accountId}:{year}:{month}` is removed from Redis. Range query cache keys (e.g. `monthly-range:{accountId}:*`) are never invalidated. As a result, range-based report endpoints return stale aggregated data for up to the full cache TTL after the rollup has run.

**Recommended Fix**

After completing each account's rollup, issue a Redis `SCAN` for keys matching `monthly-range:{accountId}:*` and delete all matches in a single pipeline call. Alternatively, maintain an explicit set of range cache keys per account in Redis (using a Redis Set), and delete all members of that set on rollup completion. The explicit-set approach avoids the `SCAN` cost on large keyspaces and is preferred for high-cardinality deployments.

---

### M5 — Rollup job double-queries all daily summaries

**File:** `src/FinancialBalance.ReportingInfrastructure/Messaging/MonthlyRollupJob.cs` lines 57–62

**Issue**

The job issues a first query via `GetRangeAsync(Guid.Empty, ...)` to discover all distinct account IDs. It then issues a second per-account query to retrieve each account's daily summaries for the rollup computation. The result set from the first query, which already contains all daily summaries, is discarded — a wasted DB round-trip that scales linearly with the number of accounts.

**Recommended Fix**

Group the result of the first query in memory using LINQ `GroupBy(d => d.AccountId)`. Pass each group directly to the rollup computation function, eliminating all per-account secondary queries. This reduces the total number of DB round-trips for the rollup job from `1 + N` to `1`, regardless of account count.

---

### M6 — Monthly report returns 404 during 24-hour rollup gap

**File:** `src/FinancialBalance.Application/Reports/Queries/GetMonthlyReport/GetMonthlyReportQueryHandler.cs` ~line 30

**Issue**

If the nightly rollup has not yet run (e.g. on the first day of a new month or immediately after deployment), `GetAsync` returns null and the handler throws `NotFoundException`. The endpoint returns HTTP 404 even though all underlying daily summaries for the requested month exist. Users experience a 404 error for up to 24 hours — the full rollup cycle.

**Recommended Fix**

Add a fallback path to the query handler: if `GetAsync` returns null, call `IDailySummaryRepository.GetRangeAsync` for the requested month's date range. If daily summaries exist, compute and return a `MonthlySummary` via `MonthlySummary.ComputeFrom(dailySummaries)` without persisting the result. Throw `NotFoundException` only if no daily summaries exist for the requested period. This ensures the endpoint is always available as long as any data exists, regardless of rollup timing.

---

## Section 2 — Performance Improvements (future)

The following improvements are not blocking correctness but will be required as transaction volume scales. They are listed in rough priority order based on expected impact.

---

### P1 — PostgreSQL native upsert

Replace the EF Core read-then-write pattern in all `UpsertAsync` implementations with `INSERT ... ON CONFLICT DO UPDATE` via `ExecuteSqlRawAsync`. This eliminates one DB round-trip per consumed event (the initial `SELECT` before `AddAsync`/`Update`) and resolves the race condition described in M3 atomically at the database level without requiring application-level retry loops.

---

### P2 — Batch consumer processing

Both `TransactionCreatedConsumer` and `TransactionCancelledConsumer` currently process one message at a time. Implement `IConsumer<Batch<T>>` using MassTransit's batch consumer support to accumulate messages within a configurable window and flush multiple `UpsertAsync` calls within a single DB transaction per batch. This significantly reduces the number of DB transactions opened per unit time under high throughput, lowering connection pool pressure and improving overall throughput.

---

### P3 — Read-model projection with materialized views

The `DailySummary` and `MonthlySummary` tables are updated on every consumed event. For accounts with high transaction volume, this creates write amplification that may degrade consumer throughput. Consider projecting summaries via PostgreSQL materialized views refreshed on a configurable schedule (e.g. every 5 minutes), decoupling read-model refresh from event consumption and reducing contention on summary rows.

---

### P4 — Redis pipeline for cache invalidation

When invalidating multiple cache keys (e.g. during rollup or range eviction), individual `KeyDeleteAsync` calls are issued sequentially — one network round-trip per key. Use `IDatabase.CreateBatch()` to issue all `KeyDeleteAsync` calls within a single Redis pipeline round-trip. This is particularly relevant when combined with the M4 fix, where `SCAN`-based invalidation may produce a large number of keys to delete.

---

### P5 — Outbox batch publish

The current `OutboxProcessor` publishes one outbox message at a time within a sequential loop, completing one publish before fetching the next. Group unprocessed messages into batches and issue all `IPublishEndpoint.Publish` calls in parallel using `Task.WhenAll`, respecting the configured `ConcurrentMessageLimit`. This reduces the wall-clock time per processor cycle proportionally to the batch size and improves throughput under backlog conditions.

---

### P6 — Query result streaming

`ListTransactionsAsync` materializes full pages into memory before returning. For large export operations or high page-size requests, this creates significant heap pressure. Expose a streaming endpoint using `IAsyncEnumerable<T>` and EF Core's `AsAsyncEnumerable()` to stream rows from the database directly to the HTTP response without buffering the full result set. This is particularly relevant for any future bulk-export or reporting endpoint.

---

## Section 3 — Security Improvements (future)

The following items address security gaps that must be resolved before any public or multi-tenant deployment of the system. They are listed from highest to lowest urgency.

---

### ~~S1 — Authentication and Authorization~~ ✅ IMPLEMENTED

JWT Bearer authentication is in place via `AddAuthentication().AddJwtBearer(...)` in `Program.cs`. Role-based authorization policies are defined (`CanManageAccounts`, `CanWriteTransactions`, `CanViewReports`) and enforced on all controllers via `[Authorize(Policy = "...")]`. Roles: `finance.admin`, `finance.operator`, `finance.viewer`, `service.erp`.

---

### ~~S2 — Input sanitization and validation~~ ✅ IMPLEMENTED

FluentValidation validators are registered as a MediatR `IPipelineBehavior` (`ValidationBehavior`) for all commands. Validators enforce amount ranges, string lengths, enum membership, and date constraints. Validation failures return a structured RFC 7807 `ProblemDetails` response (HTTP 400) with an `errors` extension field before the command reaches any domain or infrastructure layer.

---

### S3 — Audit logging

Write operations (`CreateAccount`, `RegisterTransaction`, `CancelTransaction`) leave no trace of who performed them or when. Add an `AuditLog` table with columns for command name, serialized command payload, user ID, and timestamp. Implement audit capture as a MediatR `IPipelineBehavior` that executes after successful command handling, ensuring only committed operations are logged. This provides a non-repudiable record for compliance and incident investigation.

---

### S4 — Secrets management

Connection strings and Redis URLs are currently read from `appsettings.json` and environment variables with no rotation mechanism. Integrate with a secrets manager — AWS Secrets Manager, Azure Key Vault, or HashiCorp Vault — using the corresponding .NET `IConfiguration` provider package. This allows secrets to be rotated and versioned without application redeployment, reducing the blast radius of a credential compromise.

---

### S5 — Rate limiting per authenticated user

The current sliding window rate limiter applies globally per source IP address. This allows an authenticated user to monopolize the rate limit quota from a shared IP (e.g. corporate NAT) while a different user on the same IP is throttled incorrectly. Replace the IP-keyed partition with a per-user partition using `RateLimitPartition.GetSlidingWindowLimiter` keyed on the JWT `sub` claim, falling back to IP for unauthenticated requests. This depends on S1 (authentication) being implemented first.

---

### S6 — SQL injection hardening

Any raw SQL introduced for performance improvements (see P1) must use parameterized queries exclusively. `ExecuteSqlRawAsync` accepts `SqlParameter` arguments — user-supplied values must never be interpolated directly into the SQL string. Enforce this requirement via a code review checklist item for all PRs touching raw SQL, and consider adding a Roslyn analyzer rule (e.g. via `SecurityCodeScan`) to detect string interpolation in `ExecuteSqlRawAsync` calls at compile time.

---

### S7 — HTTPS enforcement and HSTS

Neither API project currently enforces HTTPS at the application layer. Add `app.UseHsts()` and `app.UseHttpsRedirection()` in all API projects' `Program.cs`. Configure the `Strict-Transport-Security` response header with a `max-age` of at least 31536000 seconds (one year) for production deployments. Ensure HSTS preload eligibility is evaluated before any public DNS exposure.

---

### S8 — Sensitive data in logs

Account codes, transaction amounts, and user identifiers must not appear in structured log output at `Debug` or `Information` severity in production environments. Audit all log statements for PII and financial data exposure. Add a `[SensitiveData]` marker attribute or configure Serilog destructuring policies via `Destructurama.Attributed` to redact marked properties before they reach any log sink. This prevents credential and financial data leakage through centralized logging infrastructure.

---

## Summary Table

| ID | Title | Category | Priority |
|----|-------|----------|----------|
| M1 | Outbox: no dead-letter / infinite retry | Bug | Medium |
| M2 | Consumers not idempotent (duplicate message delivery) | Bug | Medium |
| M3 | Concurrent UpsertAsync causes unique constraint violation | Bug | Medium |
| M4 | Cache invalidation incomplete after monthly rollup | Bug | Medium |
| M5 | Rollup job double-queries all daily summaries | Bug | Medium |
| M6 | Monthly report returns 404 during 24-hour rollup gap | Bug | Medium |
| P1 | PostgreSQL native upsert | Performance | High |
| P2 | Batch consumer processing | Performance | High |
| P3 | Read-model projection with materialized views | Performance | Medium |
| P4 | Redis pipeline for cache invalidation | Performance | Low |
| P5 | Outbox batch publish | Performance | Medium |
| P6 | Query result streaming | Performance | Low |
| ~~S1~~ | ~~Authentication and authorization~~ | Security | ✅ Done |
| ~~S2~~ | ~~Input sanitization and validation~~ | Security | ✅ Done |
| S3 | Audit logging | Security | High |
| S4 | Secrets management | Security | High |
| S5 | Rate limiting per authenticated user | Security | Medium |
| S6 | SQL injection hardening | Security | Medium |
| S7 | HTTPS enforcement and HSTS | Security | High |
| S8 | Sensitive data in logs | Security | Medium |
