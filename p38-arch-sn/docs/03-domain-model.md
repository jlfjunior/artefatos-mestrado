# 03 — Domain Model

## Bounded Contexts

```
┌──────────────────────────────┐   ┌──────────────────────────────┐
│     Transaction Context       │   │      Reporting Context        │
│                               │   │                               │
│  Account (aggregate root)     │   │  DailySummary (aggregate)     │
│  Transaction (entity)         │   │  MonthlySummary (aggregate)   │
│  Balance (value object)       │   │  CategoryBreakdown (entity)   │
│  TransactionCategory          │   │                               │
└──────────────────────────────┘   └──────────────────────────────┘
```

---

## Entities & Aggregates

### Account (Aggregate Root)
```csharp
public class Account : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }          // stored as UPPER; e.g. "MAIN-001"
    public AccountType Type { get; private set; }     // Checking, Savings, CostCenter, CreditCard
    public Currency Currency { get; private set; }    // BRL, USD, EUR
    public decimal CurrentBalance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public static Account Create(string name, string code, AccountType type, Currency currency);
    public Transaction RegisterTransaction(TransactionType type, decimal amount, string description,
        TransactionCategory category, DateOnly transactionDate, Guid createdBy,
        string? referenceNumber = null);
    public void CancelTransaction(Guid transactionId);
    public void Deactivate();
}
```

`RegisterTransaction` updates `CurrentBalance`, adds the entity to `_transactions`, and raises `TransactionCreated` + `AccountBalanceUpdated` domain events.

`CancelTransaction` reverses the balance effect and raises `TransactionCancelled` + `AccountBalanceUpdated`.

### Transaction (Entity)
```csharp
public class Transaction : Entity
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }       // Incoming, Outgoing
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public TransactionCategory Category { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public TransactionStatus Status { get; private set; }   // Confirmed or Cancelled
    public DateOnly TransactionDate { get; private set; }   // DateOnly — no time component
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
}
```

Transactions are always created in `Confirmed` status. `Pending` and `Failed` exist in the enum but are not used in the current write flow.

### Balance (Value Object)
```csharp
public record Balance(
    decimal TotalIncoming,
    decimal TotalOutgoing,
    decimal Net,
    DateOnly Date,
    Guid AccountId)
{
    public static Balance Calculate(IEnumerable<Transaction> transactions, DateOnly date, Guid accountId)
    {
        var confirmed = transactions.Where(t => t.Status == TransactionStatus.Confirmed);
        var incoming = confirmed.Where(t => t.Type == TransactionType.Incoming).Sum(t => t.Amount);
        var outgoing = confirmed.Where(t => t.Type == TransactionType.Outgoing).Sum(t => t.Amount);
        return new Balance(incoming, outgoing, incoming - outgoing, date, accountId);
    }
}
```

### DailySummary (Aggregate Root — Reporting Context)
```csharp
public class DailySummary : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal TotalIncoming { get; private set; }
    public decimal TotalOutgoing { get; private set; }
    public decimal NetBalance { get; private set; }
    public int TransactionCount { get; private set; }
    public DateTime ComputedAt { get; private set; }

    private readonly List<CategoryBreakdown> _categoryBreakdowns = new();
    public IReadOnlyCollection<CategoryBreakdown> CategoryBreakdowns => _categoryBreakdowns.AsReadOnly();

    public static DailySummary Create(Guid accountId, DateOnly date);
    public void ApplyTransaction(string transactionType, decimal amount, string category);
    public void ReverseTransaction(string transactionType, decimal amount, string category);
}
```

`CategoryBreakdown` is a child entity (not a separate table) — stored as JSON via EF Core owned entity.

### MonthlySummary (Aggregate Root — Reporting Context)
```csharp
public class MonthlySummary : AggregateRoot
{
    public Guid AccountId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal ClosingBalance { get; private set; }
    public decimal TotalIncoming { get; private set; }
    public decimal TotalOutgoing { get; private set; }
    public decimal NetBalance { get; private set; }
    public int TransactionCount { get; private set; }
    public DateTime ComputedAt { get; private set; }

    private readonly List<CategoryBreakdown> _categoryBreakdowns = new();
    public IReadOnlyCollection<CategoryBreakdown> CategoryBreakdowns => _categoryBreakdowns.AsReadOnly();

    public static MonthlySummary ComputeFrom(Guid accountId, int year, int month,
        decimal openingBalance, IEnumerable<DailySummary> dailySummaries);
}
```

`ClosingBalance = OpeningBalance + NetBalance`. Monthly summaries are computed from daily summaries by `MonthlyRollupJob`.

---

## Enumerations

```csharp
public enum TransactionType    { Incoming, Outgoing }
public enum TransactionStatus  { Pending, Confirmed, Cancelled, Failed }  // Confirmed and Cancelled used in practice
public enum AccountType        { Checking, Savings, CostCenter, CreditCard }
public enum Currency           { BRL, USD, EUR }

public enum TransactionCategory
{
    // Incoming
    Revenue, Investment, Loan, Refund, Other,
    // Outgoing
    Payroll, Supplier, Tax, Utility, Rent, Marketing, IT, Travel
}
```

---

## Domain Events

All events implement `IDomainEvent` (with `EventId` and `OccurredAt`). They are raised by `Account` aggregate methods, collected by `AggregateRoot`, and persisted to `outbox_messages` by `AppDbContext.SaveChangesAsync`.

```csharp
public record TransactionCreated(
    Guid TransactionId,
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    DateTime TransactionDate,   // UTC DateTime converted from DateOnly
    TransactionCategory Category) : IDomainEvent;

public record TransactionCancelled(
    Guid TransactionId,
    Guid AccountId,
    decimal Amount,
    TransactionType OriginalType,
    DateTime OriginalTransactionDate,
    TransactionCategory OriginalCategory) : IDomainEvent;

public record AccountBalanceUpdated(
    Guid AccountId,
    decimal NewBalance,
    decimal PreviousBalance,
    DateTime UpdatedAt) : IDomainEvent;
```

`TransactionCreated` and `TransactionCancelled` are published to RabbitMQ via the Outbox pattern. `AccountBalanceUpdated` is persisted to the outbox but not consumed by any current worker.

---

## Invariants

| Entity | Invariant |
|---|---|
| Transaction | Amount must be > 0 |
| Transaction | `TransactionDate` cannot be in the future |
| Transaction | Already-cancelled transactions cannot be cancelled again |
| Account | `CurrentBalance` is updated atomically with each `RegisterTransaction` / `CancelTransaction` |
| Account | Cannot register transactions on an inactive account |
| Account | Cannot deactivate account with pending transactions |
| MonthlySummary | `ClosingBalance = OpeningBalance + NetBalance` |

---

## Project Structure

```
src/
├── FinancialBalance.Domain/
│   ├── Accounts/
│   │   ├── Account.cs
│   │   ├── Transaction.cs
│   │   ├── Balance.cs              ← value object
│   │   ├── AccountType.cs
│   │   ├── Currency.cs
│   │   ├── TransactionCategory.cs
│   │   ├── TransactionStatus.cs
│   │   ├── TransactionType.cs
│   │   ├── DomainException.cs
│   │   ├── IAccountRepository.cs
│   │   └── Events/
│   │       ├── TransactionCreated.cs
│   │       ├── TransactionCancelled.cs
│   │       └── AccountBalanceUpdated.cs
│   ├── Reporting/
│   │   ├── DailySummary.cs
│   │   ├── MonthlySummary.cs
│   │   ├── CategoryBreakdown.cs
│   │   ├── IDailySummaryRepository.cs
│   │   └── IMonthlySummaryRepository.cs
│   └── Shared/
│       ├── AggregateRoot.cs
│       ├── Entity.cs
│       └── IDomainEvent.cs
├── FinancialBalance.Application/
│   ├── Accounts/
│   │   ├── Commands/CreateAccount/
│   │   └── Queries/GetAccount|GetAccountBalance|ListAccounts/
│   ├── Transactions/
│   │   ├── Commands/CreateTransaction|CancelTransaction/
│   │   └── Queries/GetTransaction|ListTransactions/
│   └── Reports/
│       └── Queries/GetDailyReport|GetDailyReportRange|GetMonthlyReport|GetMonthlyReportRange/
├── FinancialBalance.Infrastructure/     ← write-side EF Core, outbox, MassTransit
├── FinancialBalance.ReportingInfrastructure/  ← read-side EF Core, Redis, consumers
├── FinancialBalance.Api/                ← Transaction API host
├── FinancialBalance.ReportingApi/       ← Reporting API host
└── FinancialBalance.Worker/             ← Worker host (consumers + jobs)
```
