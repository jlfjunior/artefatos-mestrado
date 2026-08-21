# 05 — Database Schema

## Overview

PostgreSQL 16. All monetary values use `NUMERIC(18,2)` for financial precision. UUIDs as primary keys (`gen_random_uuid()`). Timestamps stored as `TIMESTAMPTZ` (UTC). Two schemas: `finance` (write path) and `reporting` (read path).

---

## Schema: `finance` (Transaction Context)

### `accounts`
```sql
CREATE TABLE finance.accounts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(200) NOT NULL,
    code            VARCHAR(50)  NOT NULL UNIQUE,
    type            VARCHAR(50)  NOT NULL,   -- Checking, Savings, CostCenter, CreditCard
    currency        CHAR(3)      NOT NULL DEFAULT 'BRL',
    current_balance NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    is_active       BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    xmin            xid          -- PostgreSQL system column used as RowVersion concurrency token
);

CREATE UNIQUE INDEX idx_accounts_code ON finance.accounts (code);
CREATE INDEX idx_accounts_is_active   ON finance.accounts (is_active);
```

**Concurrency control**: EF Core maps the PostgreSQL system column `xmin` (type `xid`) as a `uint` shadow property with `IsRowVersion()`. This provides optimistic concurrency — a 409 Conflict is returned if two concurrent requests modify the same account row.

### `transactions`
```sql
CREATE TABLE finance.transactions (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id       UUID          NOT NULL REFERENCES finance.accounts(id) ON DELETE CASCADE,
    type             VARCHAR(20)   NOT NULL,    -- Incoming, Outgoing
    amount           NUMERIC(18,2) NOT NULL CHECK (amount > 0),
    description      VARCHAR(500)  NOT NULL,
    category         VARCHAR(100)  NOT NULL,
    reference_number VARCHAR(100),
    status           VARCHAR(20)   NOT NULL DEFAULT 'Confirmed',
    transaction_date DATE          NOT NULL,
    created_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    created_by       UUID          NOT NULL
);

CREATE INDEX idx_transactions_account_date
    ON finance.transactions (account_id, transaction_date DESC);

CREATE INDEX idx_transactions_type
    ON finance.transactions (type, transaction_date DESC);

CREATE INDEX idx_transactions_category
    ON finance.transactions (category, transaction_date DESC);

CREATE INDEX idx_transactions_reference
    ON finance.transactions (reference_number)
    WHERE reference_number IS NOT NULL;
```

### `outbox_messages` (Transactional Outbox Pattern)
```sql
CREATE TABLE finance.outbox_messages (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    type         VARCHAR(200) NOT NULL,
    payload      JSONB        NOT NULL
);

CREATE INDEX idx_outbox_unprocessed
    ON finance.outbox_messages (created_at)
    WHERE processed_at IS NULL;
```

`OutboxProcessor` polls every 2 seconds, reads up to 500 unprocessed rows, publishes to RabbitMQ, and marks them `processed_at = NOW()`.

---

## Schema: `reporting` (Reporting Context)

### `daily_summaries`
```sql
CREATE TABLE reporting.daily_summaries (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id        UUID          NOT NULL,
    date              DATE          NOT NULL,
    total_incoming    NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    total_outgoing    NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    net_balance       NUMERIC(18,2) NOT NULL,
    transaction_count INTEGER       NOT NULL DEFAULT 0,
    category_breakdowns JSONB       NOT NULL DEFAULT '[]',  -- serialized CategoryBreakdown[]
    computed_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    UNIQUE (account_id, date)
);

CREATE INDEX idx_daily_summaries_account_date
    ON reporting.daily_summaries (account_id, date DESC);
```

`category_breakdowns` is stored as JSONB — an EF Core owned entity collection, not a separate table.

### `monthly_summaries`
```sql
CREATE TABLE reporting.monthly_summaries (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id        UUID          NOT NULL,
    year              SMALLINT      NOT NULL,
    month             SMALLINT      NOT NULL CHECK (month BETWEEN 1 AND 12),
    opening_balance   NUMERIC(18,2) NOT NULL,
    closing_balance   NUMERIC(18,2) NOT NULL,
    total_incoming    NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    total_outgoing    NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    net_balance       NUMERIC(18,2) NOT NULL,
    transaction_count INTEGER       NOT NULL DEFAULT 0,
    category_breakdowns JSONB       NOT NULL DEFAULT '[]',  -- serialized CategoryBreakdown[]
    computed_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    UNIQUE (account_id, year, month)
);

CREATE INDEX idx_monthly_summaries_account
    ON reporting.monthly_summaries (account_id, year DESC, month DESC);
```

---

## EF Core Configuration

### AccountConfiguration (write path)
```csharp
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    private readonly bool _isInMemory;
    public AccountConfiguration(bool isInMemory = false) => _isInMemory = isInMemory;

    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");  // uses schema from modelBuilder.HasDefaultSchema("finance")

        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.Currency).HasConversion<string>().HasMaxLength(3);
        builder.Property(a => a.CurrentBalance).HasColumnType("numeric(18,2)");

        // xmin concurrency token — skipped for InMemory provider (integration tests)
        if (!_isInMemory)
            builder.Property<uint>("RowVersion").IsRowVersion();

        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

The `_isInMemory` flag is set by `AppDbContext.OnModelCreating` by inspecting the registered EF Core extensions. This allows integration tests to use EF Core InMemory without crashing on the unsupported `IsRowVersion()` call.

---

## Migration Strategy

- EF Core Migrations manage schema changes
- Migrations run as a Kubernetes Job before each deployment
- Indexes created `CONCURRENTLY` in production to avoid table locks
- No table partitioning — the transactions table is a standard heap table

---

## Data Retention

| Table | Retention | Strategy |
|---|---|---|
| `transactions` | 7 years (legal) | DELETE by date range (or partition in future) |
| `daily_summaries` | Configurable | `DailySummaryCleanupJob` — retention set via config |
| `monthly_summaries` | Long-term | Keep indefinitely (small table) |
| `outbox_messages` | 30 days | DELETE processed messages older than 30 days |
