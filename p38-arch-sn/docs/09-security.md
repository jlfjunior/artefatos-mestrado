# 09 — Security

## Authentication

### JWT Bearer Tokens

All API endpoints require a valid JWT token issued by the Identity Provider (IdP). Both services use the same configuration:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience  = builder.Configuration["Auth:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ClockSkew                = TimeSpan.FromSeconds(30)
        };
    });
```

### Supported Flows

| Flow | Use Case |
|---|---|
| Authorization Code + PKCE | Web/SPA clients (Finance team dashboard) |
| Client Credentials | Service-to-service (ERP integration via `service.erp` role) |

---

## Authorization

Role-based access control (RBAC) enforced via JWT claims.

| Role | Permissions |
|---|---|
| `finance.admin` | Full access: manage accounts, write transactions, view all reports |
| `finance.operator` | Create/read transactions, view reports |
| `finance.viewer` | Read-only access to reports |
| `service.erp` | Create transactions only (machine account) |

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanWriteTransactions", p =>
        p.RequireRole("finance.admin", "finance.operator", "service.erp"))
    .AddPolicy("CanViewReports", p =>
        p.RequireRole("finance.admin", "finance.operator", "finance.viewer"))
    .AddPolicy("CanManageAccounts", p =>
        p.RequireRole("finance.admin"));
```

### Policy Usage

| Endpoint | Policy |
|---|---|
| `POST /api/v1/accounts` | `CanManageAccounts` |
| `POST /api/v1/transactions` | `CanWriteTransactions` |
| `PATCH /api/v1/transactions/{id}/cancel` | `CanWriteTransactions` |
| `GET /api/v1/reports/**` | `CanViewReports` |
| `GET /api/v1/accounts/**` | Authenticated (any role) |
| `GET /api/v1/transactions/**` | Authenticated (any role) |

---

## Input Validation

All incoming requests are validated via FluentValidation through a MediatR pipeline behavior before reaching the domain layer. Validation failures return `400 Bad Request` with RFC 7807 problem details and an `errors` extension containing field-level messages.

```csharp
public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .LessThanOrEqualTo(999_999_999.99m);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.TransactionDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Transaction date cannot be in the future");
    }
}
```

---

## Error Handling

A global exception handler maps exceptions to RFC 7807 `ProblemDetails` responses (`application/problem+json`):

| Exception Type | HTTP Status |
|---|---|
| `ValidationException` (FluentValidation) | 400 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `DomainException` | 422 |
| `DbUpdateConcurrencyException` | 409 |
| Unhandled | 500 |

The `Content-Type` header is explicitly set to `application/problem+json` using raw byte serialization (not `WriteAsJsonAsync`, which overrides the content type).

---

## Concurrency Control

Account rows use the PostgreSQL `xmin` system column as an optimistic concurrency token (`IsRowVersion()` in EF Core). If two concurrent requests try to update the same account row after it was changed, EF Core throws `DbUpdateConcurrencyException` → 409 Conflict.

---

## Data Protection

### Financial Data at Rest
- PostgreSQL data encrypted at rest using cloud provider disk encryption (AES-256)
- Database credentials stored in Kubernetes Secrets (or HashiCorp Vault in production)
- Connection strings injected via environment variables — never in code or images

### Financial Data in Transit
- TLS 1.2+ enforced on all ingress routes via cert-manager
- Internal service-to-service communication uses cluster-internal DNS
- RabbitMQ connections use TLS in production

### Secrets Management
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: financial-balance-secrets
type: Opaque
data:
  postgres-connection-string: <base64>
  redis-connection-string: <base64>
  rabbitmq-password: <base64>
  jwt-secret: <base64>
```

For production, secrets are managed by **AWS Secrets Manager** or **HashiCorp Vault** and injected via the External Secrets Operator.

---

## Rate Limiting

Both APIs use sliding window rate limiters:

```csharp
// Transaction API
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit       = 5000;
        limiter.Window            = TimeSpan.FromMinutes(1);
        limiter.SegmentsPerWindow = 6;
        limiter.QueueLimit        = 100;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Reporting API — uses "reports" limiter (2000/min, queue: 50)
```

---

## Security Checklist

| Control | Status |
|---|---|
| JWT authentication on all endpoints | ✅ Implemented |
| RBAC with least-privilege roles | ✅ Implemented |
| TLS 1.2+ on all ingress | Required (cert-manager) |
| Input validation (FluentValidation) | ✅ Implemented |
| SQL injection prevention (EF Core parameterized) | ✅ Built-in |
| RFC 7807 error responses | ✅ Implemented |
| Optimistic concurrency (xmin RowVersion) | ✅ Implemented |
| Secrets via Kubernetes Secrets / Vault | Required |
| Container runs as non-root user | ✅ Implemented in Dockerfile |
| Rate limiting per endpoint | ✅ Implemented |
| CORS policy (explicit allowlist) | Required |
| Security headers (HSTS, CSP, X-Frame-Options) | Required |
| Container image scanning (Trivy in CI) | Required |
| Dependency vulnerability scanning | Required |
