# ADR - Architecture Decision Records

## ADR-001: Event-Driven Architecture em vez de Sincrono

**Data**: 2025-06-16
**Status**: Accepted
**Contexto**: Servico de consolidacao precisa lidar com 50 req/s com tolerancia a falhas. Transacoes devem ser gravadas independentemente do estado do consolidador.

### O Problema

Se Consolidacao fosse chamada sincronamente (REST), qualquer falha ou lentidao no consolidador atrasaria ou bloquearia a gravacao de transacoes. Alem disso, picos de requisicao causariam gargalos.

### Decisao

Usar **Event-Driven Architecture** com MassTransit + RabbitMQ:

- **Transactions API** publica `TransactionCreatedEvent` apos persistir a transacao
- **Consolidation API** consome o evento via `TransactionCreatedEventConsumer` (MassTransit)
- **Consolidation API** publica `ConsolidationUpdatedEvent` apos atualizar o saldo

### Vantagens

- Desacoplamento: Transactions nao depende do estado de Consolidation
- Escalabilidade: Consolidation pode escalar independentemente
- Tolerancia: Retry automatico (5x, 2s) + Dead Letter Queue para falhas persistentes
- Extensibilidade: Novos consumidores (auditoria, analytics) sem alterar produtores

### Desvantagens (Trade-offs)

- Eventual Consistency: Saldos levam milissegundos para atualizar apos a transacao
- Complexidade: Mensageria, consumidores, deadletter
- Debugging mais dificil: Fluxo assincrono

### Alternativas Consideradas

1. **REST Sincrono**: Simples mas fragil e sem escalabilidade
2. **gRPC**: Melhor performance mas ainda sincrono
3. **Event-Driven (escolhida)**: Resiliente e escalavel

---

## ADR-002: Banco de Dados Compartilhado

**Data**: 2025-06-16
**Status**: Accepted
**Contexto**: Ambos os servicos precisam persistir e ler dados. Cada um gerencia suas proprias tabelas dentro do mesmo banco.

### O Problema

Dois bancos separados causariam complexidade de sincronizacao e transacoes distribuidas (Saga pattern) sem beneficio claro na fase inicial.

### Decisao

**Um banco SQL Server compartilhado** (`CaixaDb`) com tabelas separadas por servico:

- `Transactions` — gerenciada exclusivamente por Transactions.API
- `Consolidations` — gerenciada exclusivamente por Consolidation.API

### Vantagens

- Simplicidade: Apenas um banco para configurar e fazer backup
- Transacoes ACID locais garantidas
- Menor overhead operacional

### Desvantagens

- Acoplamento pelo banco: Um servico pode impactar o outro em carga extrema
- Escalabilidade limitada a longo prazo

### Evolucao Futura

Com crescimento, migrar para:
1. **Multi-database com Saga Pattern** — cada servico com seu banco
2. **Event Sourcing** — eventos como source of truth

---

## ADR-003: MassTransit como Abstracao de Mensageria

**Data**: 2025-06-16
**Status**: Accepted
**Contexto**: Os servicos precisam de mensageria confiavel com retry, dead letter e suporte a evolucao futura do broker.

### Decisao

Usar **MassTransit 8** como camada de abstracao sobre **RabbitMQ 3.12**, em vez de usar o cliente RabbitMQ diretamente.

### Justificativa

| Aspecto | RabbitMQ direto | MassTransit + RabbitMQ |
|---------|-----------------|------------------------|
| **Retry** | Manual | Configuravel declarativamente |
| **Dead Letter Queue** | Manual | Automatico |
| **Serialization** | Manual | Automatico (JSON) |
| **Troca de broker** | Reescrever tudo | Mudar um pacote |
| **Consumers** | Boilerplate | Implementar `IConsumer<T>` |
| **Testes** | Dificil mockar | `IPublishEndpoint` facil de mockar |

### Configuracao Atual

- Retry: 5 tentativas com intervalo de 2 segundos
- Endpoint formatter: kebab-case
- Consumer: `TransactionCreatedEventConsumer`

### Evolucao Futura

Se necessario migrar de RabbitMQ para Azure Service Bus ou Amazon SQS, basta trocar o pacote MassTransit sem alterar os servicos de dominio.

---

## ADR-004: Clean Architecture com Domain-Driven Design

**Data**: 2025-06-16
**Status**: Accepted
**Contexto**: Projeto precisa ser mantenivel e testavel por anos.

### Decisao

Implementar **Clean Architecture** em 4 camadas por servico:

```
┌──────────────────────────────────────┐
│     ENTRYPOINT (Controllers)         │  HTTP Requests / Responses
├──────────────────────────────────────┤
│    APPLICATION (Services, DTOs)      │  Casos de uso
├──────────────────────────────────────┤
│      DOMAIN (Entities, Logic)        │  Regras de negocio
├──────────────────────────────────────┤
│   INFRASTRUCTURE (DB, Messaging)     │  Detalhes tecnicos
└──────────────────────────────────────┘
```

### Camadas

**Domain** (independente, sem dependencias externas)
```csharp
public class Transaction {
    public static Transaction Create(Lojista lojista, decimal amount, string type, DateTime date) { }
}
```

**Application** (casos de uso, orquestra dominio)
```csharp
public class TransactionService : ITransactionService {
    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken ct) { }
}
```

**Infrastructure** (detalhes tecnicos — banco, mensageria)
```csharp
public class TransactionRepository : ITransactionRepository { }
public class TransactionCreatedEventConsumer : IConsumer<TransactionCreatedEvent> { }
```

**EntryPoint** (HTTP)
```csharp
[HttpPost] public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request) { }
```

### Vantagens

- Testabilidade: Domain e Application testados sem infraestrutura
- Manutenibilidade: Mudancas isoladas por camada
- SOLID: Interface Segregation e Dependency Inversion naturais

---

## ADR-005: Testes com xUnit + Moq + Testcontainers

**Data**: 2025-06-16
**Status**: Accepted
**Contexto**: Projeto precisa de qualidade e confianca.

### Decisao

- **Framework**: xUnit 2.6
- **Mocking**: Moq 4.20
- **Assertions**: FluentAssertions 6.12 (legibilidade)
- **Integration Tests**: Testcontainers 3.5 (SQL Server e RabbitMQ em containers reais)

### Piramide de Testes

```
         /\
        /  \   E2E (manual/Selenium)
       /────\
      /      \  Integration (Testcontainers — pronto, a implementar)
     /────────\
    /          \  Unit (Moq) — implementado
   /────────────\
```

### Coverage Alvo

| Camada | Alvo |
|--------|------|
| Domain | 90%+ |
| Application | 80%+ |
| Infrastructure | 70%+ |
| EntryPoint | 60%+ |

---

## ADR-006: Health Checks e Observabilidade

**Data**: 2025-06-16
**Status**: Accepted
**Contexto**: Producao requer observabilidade e monitoramento.

### Decisao

Endpoint `GET /health` em ambos os servicos verificando:

- SQL Server (conectividade e latencia)
- RabbitMQ (conectividade)

### Resposta

```json
{
  "status": "Healthy",
  "checks": {
    "sqlserver": "Healthy",
    "rabbitmq": "Healthy"
  },
  "timestamp": "2025-06-16T14:30:00Z"
}
```

Possiveis status: `Healthy`, `Degraded`, `Unhealthy`.

### Evolucao Futura

- Prometheus metrics (`/metrics`)
- Distributed tracing (Jaeger / OpenTelemetry)
- Logging centralizado (Splunk / Datadog / Seq)

---

## ADR-007: SOLID Principles

**Data**: 2025-06-16
**Status**: Accepted

### Aplicacao de SOLID no Projeto

| Principio | Implementacao |
|-----------|---------------|
| **S**ingle Responsibility | `TransactionService` so cria/lista transacoes; `ConsolidationService` so consolida |
| **O**pen/Closed | `ITransactionRepository` aberto para extensao, fechado para modificacao |
| **L**iskov Substitution | `TransactionRepository` e `ConsolidationRepository` intercambiaveis pelas interfaces |
| **I**nterface Segregation | `ITransactionService` e `IConsolidationService` especificas; nao expoe metodos desnecessarios |
| **D**ependency Inversion | Services dependem de `IUnitOfWork`, nao de `TransactionDbContext` diretamente |

### Exemplos

```csharp
// SRP
public class TransactionService : ITransactionService { /* apenas transacoes */ }
public class ConsolidationService : IConsolidationService { /* apenas consolidacoes */ }

// OCP + DIP
public TransactionService(
    IUnitOfWork unitOfWork,      // abstracao
    IPublishEndpoint publisher,  // abstracao MassTransit
    ILogger<TransactionService> logger) { }

// ISP
public interface ITransactionService
{
    Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken ct);
    Task<TransactionDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PaginationResponse<TransactionDto>> GetByDateRangeAsync(...);
}
```

---

## Sumario

| ADR | Decisao | Status |
|-----|---------|--------|
| 001 | Event-Driven com RabbitMQ via MassTransit | Accepted |
| 002 | Banco Compartilhado (CaixaDb) | Accepted |
| 003 | MassTransit como abstracao de mensageria | Accepted |
| 004 | Clean Architecture (Domain/Application/Infrastructure/EntryPoint) | Accepted |
| 005 | xUnit + Moq + FluentAssertions + Testcontainers | Accepted |
| 006 | Health Checks (SQL Server + RabbitMQ) | Accepted |
| 007 | SOLID Principles | Accepted |

---

**Para o diagrama e schema do banco, veja [ARCHITECTURE.md](../ARCHITECTURE.md)**
