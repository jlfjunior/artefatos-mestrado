# 04 — API Design

## Base URLs

| Service | Local | Description |
|---|---|---|
| Transaction API | `http://localhost:5000` | Account management and transaction writes |
| Reporting API | `http://localhost:5001` | Pre-computed daily/monthly reports |

Route prefix for all endpoints: `/api/v1/...`

All responses use `application/json`. Enums are serialized as strings. `null` fields are omitted. Monetary values use `decimal`. Dates use `DateOnly` format (`2024-01-15`). Timestamps use ISO 8601 UTC (`2024-01-15T10:30:00Z`).

---

## Authentication

All endpoints require a JWT Bearer token:
```
Authorization: Bearer <token>
```

Roles: `finance.admin`, `finance.operator`, `finance.viewer`, `service.erp`

---

## Transaction API — Accounts

### `GET /api/v1/accounts`
List all accounts.

**Query params:** `isActive` (bool?), `page` (int, default 1), `pageSize` (int, default 20, max 100)

**Response `200 OK`:**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Main Operating Account",
      "code": "MAIN-001",
      "type": "Checking",
      "currency": "BRL",
      "currentBalance": 0.00,
      "isActive": true,
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### `GET /api/v1/accounts/{id}`
Get account by ID.

**Response `200 OK`:** single `AccountDto`  
**Response `404 Not Found`:** RFC 7807 problem details

### `GET /api/v1/accounts/{id}/balance`
Get current balance for account.

**Response `200 OK`:**
```json
{
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currentBalance": 15420.50,
  "currency": "BRL",
  "asOf": "2024-01-15T10:30:00Z"
}
```

### `POST /api/v1/accounts`
Create a new account. Requires `CanManageAccounts` policy (`finance.admin`).

**Request:**
```json
{
  "name": "Main Operating Account",
  "code": "MAIN-001",
  "type": "Checking",
  "currency": "BRL"
}
```

**Response `201 Created`:** `AccountDto` with `Location` header  
**Response `400 Bad Request`:** validation failure  
**Response `409 Conflict`:** duplicate account code

---

## Transaction API — Transactions

### `GET /api/v1/transactions`
List transactions with filters. Requires `accountId`.

**Query params:**
| Param | Type | Description |
|---|---|---|
| `accountId` | GUID | Required — filter by account |
| `type` | string | `Incoming` or `Outgoing` |
| `category` | string | Transaction category enum value |
| `status` | string | `Confirmed`, `Cancelled` |
| `from` | DateOnly | Start date (inclusive) |
| `to` | DateOnly | End date (inclusive) |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20, max: 100) |

**Response `200 OK`:**
```json
{
  "data": [ /* array of TransactionDto */ ],
  "totalCount": 143,
  "page": 1,
  "pageSize": 20
}
```

### `GET /api/v1/transactions/{transactionId}`
Get a single transaction. Requires `accountId` query param.

**Query params:** `accountId` (GUID, required)

**Response `200 OK`:** `TransactionDto`  
**Response `404 Not Found`:** account or transaction not found

### `POST /api/v1/transactions`
Register a new transaction. Requires `CanWriteTransactions` policy.

**Request:**
```json
{
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "Incoming",
  "amount": 5000.00,
  "description": "Client payment - Invoice #1042",
  "category": "Revenue",
  "referenceNumber": "INV-1042",
  "transactionDate": "2024-01-15"
}
```

**Response `201 Created`:**
```json
{
  "id": "7b3e2c1a-9d4f-4a2b-8e5c-1f6a3b7c9d2e",
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "Incoming",
  "amount": 5000.00,
  "description": "Client payment - Invoice #1042",
  "category": "Revenue",
  "referenceNumber": "INV-1042",
  "status": "Confirmed",
  "transactionDate": "2024-01-15",
  "createdAt": "2024-01-15T10:35:00Z"
}
```

### `PATCH /api/v1/transactions/{transactionId}/cancel`
Cancel a transaction. Requires `CanWriteTransactions` policy.

**Query params:** `accountId` (GUID, required)

**Response `204 No Content`** — no body  
**Response `404 Not Found`:** account or transaction not found  
**Response `422 Unprocessable Entity`:** transaction already cancelled

---

## Reporting API — Reports

All reporting endpoints require `CanViewReports` policy (`finance.admin`, `finance.operator`, `finance.viewer`).

### `GET /api/v1/reports/daily`
Get the daily balance summary for an account on a specific date.

**Query params:** `accountId` (GUID, required), `date` (DateOnly, required)

**Response `200 OK`:**
```json
{
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2024-01-15",
  "totalIncoming": 12500.00,
  "totalOutgoing": 3200.00,
  "netBalance": 9300.00,
  "transactionCount": 8,
  "categoryBreakdowns": [
    { "category": "Revenue", "totalIncoming": 10000.00, "totalOutgoing": 0.00 },
    { "category": "Supplier", "totalIncoming": 0.00, "totalOutgoing": 2500.00 }
  ],
  "computedAt": "2024-01-15T23:59:00Z"
}
```

**Response `404 Not Found`:** no summary exists for that date

### `GET /api/v1/reports/daily/range`
Get daily summaries for a date range (max 92 days).

**Query params:** `accountId`, `from`, `to`

**Response `200 OK`:** `IReadOnlyList<DailyReportDto>`

### `GET /api/v1/reports/monthly`
Get the full monthly balance summary.

**Query params:** `accountId`, `year`, `month` (1–12)

**Response `200 OK`:**
```json
{
  "accountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "year": 2024,
  "month": 1,
  "openingBalance": 6120.50,
  "closingBalance": 21420.50,
  "totalIncoming": 45000.00,
  "totalOutgoing": 29700.00,
  "netBalance": 15300.00,
  "transactionCount": 87,
  "categoryBreakdowns": [
    { "category": "Revenue", "totalIncoming": 40000.00, "totalOutgoing": 0.00 },
    { "category": "Payroll", "totalIncoming": 0.00, "totalOutgoing": 18000.00 }
  ],
  "computedAt": "2024-01-31T23:59:00Z"
}
```

### `GET /api/v1/reports/monthly/range`
Get multiple months summary (max 12 months).

**Query params:** `accountId`, `fromYear`, `fromMonth`, `toYear`, `toMonth`

**Response `200 OK`:** `IReadOnlyList<MonthlyReportSummaryDto>`

---

## Error Responses

All errors follow RFC 7807 Problem Details (`application/problem+json`):

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "Amount must be greater than zero.",
  "instance": "/api/v1/transactions",
  "errors": {
    "amount": ["Amount must be greater than zero."]
  }
}
```

| Status | Meaning |
|---|---|
| `400` | Validation error (FluentValidation failure) |
| `401` | Unauthorized (missing/invalid token) |
| `403` | Forbidden (insufficient role) |
| `404` | Resource not found |
| `409` | Conflict (e.g. duplicate account code) |
| `422` | Business rule violation (domain exception) |
| `429` | Rate limit exceeded |
| `500` | Internal server error |

---

## Rate Limiting

| Service | Policy | Limit |
|---|---|---|
| Transaction API | Sliding window | 5,000 req/min (queue: 100) |
| Reporting API | Sliding window | 2,000 req/min (queue: 50) |
