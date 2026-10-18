# Arquitetura - Controle de Fluxo de Caixa

## 1. Visão Geral

Sistema escalável e resiliente para controle de lançamentos (débitos/créditos) e consolidação diária de saldo por lojista, desenvolvido em C# .NET 8 com padrões de arquitetura modernos.

```
┌─────────────────────────────────────────────────────────────┐
│                     CLIENT (API REST)                        │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
   ┌────▼──────────────┐   ┌──────▼──────────────┐
   │  Transactions API │   │  Consolidation API  │
   │   (Port 5001)     │   │   (Port 5002)       │
   └────┬──────────────┘   └──────┬──────────────┘
        │                         │
        │ Publica                 │ Consome
        │ TransactionCreatedEvent │ TransactionCreatedEvent
        │ (MassTransit)           │ (MassTransit)
        │                         │ Publica
        │                         │ ConsolidationUpdatedEvent
        │                         │
        ├───────────┬─────────────┘
        │           │
   ┌────▼──┐   ┌────▼─────┐
   │  SQL  │   │ RabbitMQ │
   │Server │   │          │
   └───────┘   └──────────┘
```

## 2. Conceito de Negócio: Lojista

O sistema gerencia o fluxo de caixa de 4 lojas independentes:

| Código | Enum         | Nome        |
|--------|--------------|-------------|
| 0001   | LojaNorte=1  | Loja Norte  |
| 0002   | LojaSul=2    | Loja Sul    |
| 0003   | LojaLeste=3  | Loja Leste  |
| 0004   | LojaOeste=4  | Loja Oeste  |

Toda transação pertence a um lojista. A consolidação diária é calculada separadamente por lojista — cada loja tem seu próprio saldo do dia (índice único em `ConsolidationDate + Lojista`).

## 3. Padrões Arquiteturais

### 3.1 Microsserviços com Event-Driven

- **Transactions API**: Persiste lançamentos e publica eventos
- **Consolidation API**: Consome eventos e calcula saldos por lojista por dia
- **Desacoplamento**: Via eventos assíncronos (RabbitMQ + MassTransit)

### 3.2 Por que essa abordagem?

1. **Escalabilidade**: Consolidado escala independentemente (50 req/s alvo)
2. **Resiliência**: Falha no Consolidado não afeta gravação de transações
3. **Flexibilidade**: Novos consumidores (Auditoria, Analytics) sem alterar produtores
4. **Tolerância a falhas**: Retry automático (5×, 2 s) + Dead Letter Queue

## 4. Componentes

### 4.1 Transactions API (Port 5001)

**Responsabilidades:**
- Criar, listar e detalhar lançamentos (débito/crédito) por lojista
- Persistir em SQL Server
- Publicar `TransactionCreatedEvent` via MassTransit

**Endpoints:**
```
POST  /api/transactions        - Criar lançamento
GET   /api/transactions        - Listar (paginado, filtro por data)
GET   /api/transactions/{id}   - Detalhe
GET   /health                  - Health check
```

**Stack:**
- .NET 8 (C#)
- Entity Framework Core 8 (SQL Server)
- MassTransit 8 + RabbitMQ
- Serilog (console + arquivo rotativo diário)

### 4.2 Consolidation API (Port 5002)

**Responsabilidades:**
- Consumir `TransactionCreatedEvent` e atualizar saldo por lojista por dia
- Publicar `ConsolidationUpdatedEvent` após cada atualização
- Retornar consolidados por data ou período

**Endpoints:**
```
GET   /api/consolidations        - Listar (paginado, filtro por período)
GET   /api/consolidations/{date} - Consolidado da data
GET   /health                    - Health check
```

**Stack:**
- .NET 8 (C#)
- Entity Framework Core 8 (SQL Server)
- MassTransit 8 + RabbitMQ (consumer)
- Serilog

### 4.3 Shared (biblioteca comum)

| Namespace | Conteúdo |
|-----------|----------|
| `Shared.Contracts` | `TransactionCreatedEvent`, `ConsolidationUpdatedEvent` |
| `Shared.DTOs` | `TransactionDto`, `ConsolidationDto`, `CreateTransactionRequest`, `PaginationResponse<T>` |
| `Shared.Enums` | `Lojista` (LojaNorte, LojaSul, LojaLeste, LojaOeste) |

## 5. Estrutura de Pastas

```
services/
├── Shared/
│   ├── Contracts/          # Eventos de integração
│   ├── DTOs/               # Objetos de transferência de dados
│   └── Enums/              # Lojista
│
├── Transactions.API/
│   └── src/
│       ├── Domain/         # Transaction entity, ITransactionRepository, IUnitOfWork
│       ├── Application/    # TransactionService, ITransactionService
│       ├── Infrastructure/ # TransactionDbContext, Repository, UnitOfWork
│       └── EntryPoint/     # TransactionsController
│
└── Consolidation.API/
    └── src/
        ├── Domain/         # Consolidation entity, IConsolidationRepository, IUnitOfWork
        ├── Application/    # ConsolidationService, IConsolidationService
        ├── Infrastructure/ # ConsolidationDbContext, Repository, UnitOfWork,
        │                   # TransactionCreatedEventConsumer
        └── EntryPoint/     # ConsolidationsController
```

## 6. Fluxo de Dados

### 6.1 Criar Lançamento

```
Client
  ↓
POST /api/transactions  {lojista, amount, type, transactionDate}
  ↓
TransactionService.CreateAsync()
  ↓
SQL Server — persiste Transaction (com Lojista)
  ↓
PublishAsync(TransactionCreatedEvent)  →  RabbitMQ
  ↓
TransactionCreatedEventConsumer (Consolidation.API)
  ↓
ConsolidationService.ProcessTransactionAsync()
  ↓
SQL Server — cria/atualiza Consolidation (Lojista + Data)
  ↓
PublishAsync(ConsolidationUpdatedEvent)  →  RabbitMQ
```

### 6.2 Obter Consolidado

```
Client
  ↓
GET /api/consolidations/{date}
  ↓
ConsolidationService.GetByDateAsync(date)
  ↓
SQL Server
  ↓
Retorna ConsolidationDto com saldos do dia
```

## 7. Banco de Dados

### 7.1 Schema

```sql
-- Transações (gerenciado por Transactions.API)
CREATE TABLE Transactions (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Lojista         INT NOT NULL,             -- 1=LojaNorte, 2=LojaSul, 3=LojaLeste, 4=LojaOeste
    Amount          DECIMAL(18,2) NOT NULL,
    Type            NVARCHAR(50) NOT NULL,    -- 'Debit' ou 'Credit'
    Description     NVARCHAR(500) NULL,
    TransactionDate DATETIME2 NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsProcessed     BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Transactions_TransactionDate ON Transactions(TransactionDate);
CREATE INDEX IX_Transactions_CreatedAt ON Transactions(CreatedAt);

-- Consolidações diárias por lojista (gerenciado por Consolidation.API)
CREATE TABLE Consolidations (
    Id                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Lojista           INT NOT NULL,           -- 1=LojaNorte, 2=LojaSul, 3=LojaLeste, 4=LojaOeste
    ConsolidationDate DATETIME2 NOT NULL,
    DebitTotal        DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditTotal       DECIMAL(18,2) NOT NULL DEFAULT 0,
    DailyBalance      DECIMAL(18,2) NOT NULL DEFAULT 0,   -- CreditTotal - DebitTotal
    ProcessedCount    INT NOT NULL DEFAULT 0,
    LastUpdatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE UNIQUE INDEX IX_Consolidations_Date_Lojista ON Consolidations(ConsolidationDate, Lojista);
```

**Inicialização:** O banco é criado via `SQL_CREATE_DATABASE.sql`, executado automaticamente pelo container `sql-init` no docker-compose. Não utiliza EF Core migrations.

## 8. Tecnologias

| Camada | Tecnologia |
|--------|------------|
| **API** | ASP.NET Core 8 |
| **Linguagem** | C# 12 |
| **Banco** | SQL Server 2022 |
| **ORM** | Entity Framework Core 8 |
| **Mensageria** | RabbitMQ 3.12 via MassTransit 8 |
| **Logs** | Serilog 4 (console + arquivo rotativo) |
| **Health Checks** | SQL Server + RabbitMQ probes |
| **Testes** | xUnit + Moq + FluentAssertions + Testcontainers |
| **Container** | Docker + Compose |
| **CI/CD** | GitHub Actions (build, testes, Docker push, Trivy scan) |

## 9. Requisitos Não-Funcionais

### 9.1 Escalabilidade

- **Horizontal scaling**: Consolidation API pode ser escalada com `--scale consolidation-api=N`
- **Particionamento natural**: Consolidação por lojista permite paralelismo entre lojas
- **Load Balancer**: Nginx / IIS ARR

### 9.2 Resiliência

- **Retry Policy**: MassTransit — 5 tentativas, intervalo de 2 segundos
- **Dead Letter Queue**: RabbitMQ para mensagens que esgotam retries
- **Health Checks**: Liveness + Readiness em todos os serviços
- **Desacoplamento**: Transactions API opera mesmo que Consolidation API esteja fora do ar

### 9.3 Segurança

- **Autenticação**: JWT Bearer (planejado — v1.1)
- **SQL Injection**: Prevenido via EF Core parametrizado
- **Secrets**: Variáveis de ambiente / Azure KeyVault ready
- **Auditoria**: Serilog estruturado com correlationId

## 10. Requisitos de Desempenho

| Métrica | Target |
|---------|--------|
| **Latência (p50)** | < 100ms |
| **Latência (p99)** | < 500ms |
| **Throughput** | 50+ req/s |
| **Disponibilidade** | 99.5% |

## 11. Decisões Arquiteturais (ADRs)

Veja [docs/ADRs.md](docs/ADRs.md) para as 7 decisões documentadas com contexto, vantagens e trade-offs.

| ADR | Decisão | Status |
|-----|---------|--------|
| 001 | Event-Driven com RabbitMQ (MassTransit) | Accepted |
| 002 | Banco Compartilhado (shared database) | Accepted |
| 003 | MassTransit como abstração de mensageria | Accepted |
| 004 | Clean Architecture | Accepted |
| 005 | xUnit + Moq + Testcontainers | Accepted |
| 006 | Health Checks e Observabilidade | Accepted |
| 007 | SOLID Principles | Accepted |

## 12. Evolução Futura

- [ ] CQRS (Comando-Query Responsibility Segregation)
- [ ] Event Sourcing completo
- [ ] Saga Distributed para multi-BD
- [ ] Redis Cache para saldos consolidados
- [ ] JWT Authentication + RBAC (User, Admin)
- [ ] Prometheus metrics + Jaeger distributed tracing ou DataDog
- [ ] GraphQL API
- [ ] Webhooks para integrações externas
