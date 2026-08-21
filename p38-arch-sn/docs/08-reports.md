# 08 — Report Generation

## Overview

Reports are generated asynchronously by the Reporting Worker, which consumes `TransactionCreated` and `TransactionCancelled` events from RabbitMQ. Pre-computed aggregates (`DailySummary`, `MonthlySummary`) are stored in the PostgreSQL `reporting` schema and cached in Redis.

---

## Daily Report Pipeline

```
TransactionCreated / TransactionCancelled event
        │
        ▼
Reporting Worker receives event (MassTransit consumer)
        │
        ▼
UpsertAsync DailySummary for (accountId, date)
  - DailySummary.ApplyTransaction / ReverseTransaction
  - Updates total_incoming, total_outgoing, net_balance, transaction_count
  - Updates category_breakdowns (JSONB)
        │
        ▼
Invalidate Redis cache key: daily:{accountId}:{date}
        │
        ▼
Next GET /api/v1/reports/daily → reads from DB, caches in Redis
```

### Worker Consumers

```csharp
public class TransactionCreatedConsumer : IConsumer<TransactionCreated>
{
    public async Task Consume(ConsumeContext<TransactionCreated> context)
    {
        var msg = context.Message;

        await _dailySummaryRepository.UpsertAsync(
            msg.AccountId,
            DateOnly.FromDateTime(msg.TransactionDate),
            msg.Type.ToString(),
            msg.Amount,
            msg.Category.ToString(),
            cancellationToken);

        await _cache.RemoveAsync($"daily:{msg.AccountId}:{msg.TransactionDate:yyyy-MM-dd}");
    }
}
```

`TransactionCancelledConsumer` calls `ReverseTransaction` on the `DailySummary` to subtract the cancelled transaction's contribution.

### MassTransit Retry Policy (both consumers)
- Retry intervals: 5s → 15s → 30s
- Concurrency limits: 50 (TransactionCreated), 25 (TransactionCancelled)

---

## Monthly Report Pipeline

Monthly summaries are derived from daily summaries to keep computation cheap.

### Trigger: `MonthlyRollupJob`

A background job (`IHostedService`) runs at `00:05 UTC` daily via Quartz.NET:

```
MonthlyRollupJob (00:05 UTC)
  → GetRangeAsync(all accounts, previous month date range)
  → For each account:
      → GetRangeAsync(dailySummaries for that month)
      → MonthlySummary.ComputeFrom(accountId, year, month, openingBalance, dailySummaries)
      → UpsertAsync(MonthlySummary)
      → RemoveAsync(Redis monthly cache key)
```

`openingBalance` is the closing balance of the previous month's summary (or 0 if first month).

---

## Caching Strategy

| Cache Key Pattern | TTL | Invalidated When |
|---|---|---|
| `daily:{accountId}:{date}` | 1 hour | New transaction or cancellation on that date |
| `monthly:{accountId}:{year}:{month}` | 6 hours | Daily rollup runs for that month |

Cache reads use a Polly circuit breaker. If Redis is unavailable (circuit open), queries fall through directly to PostgreSQL.

```csharp
public async Task<DailyReportDto?> GetDailyReportAsync(Guid accountId, DateOnly date, CancellationToken ct)
{
    var cacheKey = $"daily:{accountId}:{date:yyyy-MM-dd}";

    // GetAsync is wrapped in a Polly circuit breaker pipeline
    var cached = await _cache.GetAsync<DailyReportDto>(cacheKey, ct);
    if (cached is not null) return cached;

    var summary = await _repository.GetAsync(accountId, date, ct);
    if (summary is not null)
        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromHours(1), ct);

    return summary;
}
```

---

## DailySummaryCleanupJob

A configurable background job removes old daily summaries to control storage growth:

```json
{
  "Jobs": {
    "DailySummaryRetentionDays": 730
  }
}
```

The job runs on a configurable schedule and deletes `daily_summaries` rows older than the retention window.

---

## Report Read Path (Reporting API)

```
Client
  → GET /api/v1/reports/daily?accountId=...&date=...
  → JWT auth (CanViewReports policy)
  → MediatR → GetDailyReportQueryHandler
  → Redis cache (hit?) → return cached DailyReportDto
  → PostgreSQL reporting schema (miss) → cache result → return response
```

The Reporting API is purely read-only (CQRS read side). It never writes to PostgreSQL or raises domain events.

---

## Report Response Shapes

### DailyReportDto
```csharp
public record DailyReportDto(
    Guid AccountId,
    DateOnly Date,
    decimal TotalIncoming,
    decimal TotalOutgoing,
    decimal NetBalance,
    int TransactionCount,
    IReadOnlyList<CategoryBreakdownDto> CategoryBreakdowns,
    DateTime ComputedAt);
```

### MonthlyReportDto
```csharp
public record MonthlyReportDto(
    Guid AccountId,
    int Year,
    int Month,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal TotalIncoming,
    decimal TotalOutgoing,
    decimal NetBalance,
    int TransactionCount,
    IReadOnlyList<CategoryBreakdownDto> CategoryBreakdowns,
    DateTime ComputedAt);
```

### CategoryBreakdownDto
```csharp
public record CategoryBreakdownDto(
    string Category,
    decimal TotalIncoming,
    decimal TotalOutgoing);
```
