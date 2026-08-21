# Fluxo de Caixa - Sistema de Controle de Lancamentos

Um projeto .NET 8 escalavel e resiliente para controle de fluxo de caixa com dois servicos desacoplados via mensageria.

## Visao Geral

Este projeto implementa controle de fluxo de caixa para **4 lojas (lojistas)** com:

- **Transactions API**: Cria e gerencia lancamentos (debitos/creditos) por lojista
- **Consolidation API**: Calcula e disponibiliza saldos consolidados por lojista por dia

Os servicos comunicam-se via **eventos assincronos** com RabbitMQ (MassTransit), garantindo:

- **Desacoplamento**: Falha em um servico nao afeta o outro
- **Escalabilidade**: Consolidacao suporta 50+ req/s com scaling horizontal
- **Resiliencia**: Retry automatico (5x) + Dead Letter Queue
- **Multi-loja**: Cada lojista tem seu proprio saldo diario consolidado

---

## Lojistas

| Valor JSON | Codigo | Nome        |
|------------|--------|-------------|
| `1`        | 0001   | Loja Norte  |
| `2`        | 0002   | Loja Sul    |
| `3`        | 0003   | Loja Leste  |
| `4`        | 0004   | Loja Oeste  |

---

## Arquitetura

```
┌──────────────────────────────────────────────────────────┐
│                   CLIENT (API REST)                       │
└────────────────────┬─────────────────────────────────────┘
                     │
        ┌────────────┴──────────────┐
        │                           │
   ┌────▼──────────────┐      ┌─────▼─────────────┐
   │  Transactions API │      │  Consolidation API │
   │  (Port 5001)      │      │  (Port 5002)       │
   └────┬──────────────┘      └────┬───────────────┘
        │                          │
        │ Publica evento           │ Consome + processa
        │ TransactionCreated       │ saldo por lojista/dia
        │                          │
        ├──────────────────────────┘
        │
   ┌────▼──┐    ┌──────────┐
   │  SQL  │    │ RabbitMQ │
   │Server │    │(MassTransit)
   └───────┘    └──────────┘
```

Padroes aplicados: **Clean Architecture**, **Event-Driven**, **SOLID**, **Repository + Unit of Work**.

---

## Requisitos

- Docker & Docker Compose
- .NET 8 SDK (apenas para desenvolvimento local)

---

## Como Rodar

### 1. Clonar o Repositorio

```bash
git clone https://github.com/seu-usuario/fluxo-caixa.git
cd fluxo-caixa
```

### 2. Subir com Docker Compose

```bash
docker-compose up -d
```

Isso inicia:

| Servico | Porta | Descricao |
|---------|-------|-----------|
| SQL Server | 1433 | Banco de dados (banco `CaixaDb` criado automaticamente) |
| RabbitMQ | 5672 / 15672 | Mensageria (Management UI na 15672) |
| Transactions API | 5001 | Servico de lancamentos |
| Consolidation API | 5002 | Servico de consolidacao |

Aguarde ~30 segundos para todos os containers ficarem saudaveis:

```bash
docker-compose ps
```

### 3. Acessar as APIs

- **Transactions API (Swagger)**: http://localhost:5001/swagger
- **Consolidation API (Swagger)**: http://localhost:5002/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest / guest)

---

## Exemplos de Uso

### Criar Lancamento

```bash
curl -X POST http://localhost:5001/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "lojista": 1,
    "amount": 150.50,
    "type": "Credit",
    "description": "Venda de produtos",
    "transactionDate": "2025-06-16T00:00:00Z"
  }'
```

Valores de `lojista`: `1`=Loja Norte, `2`=Loja Sul, `3`=Loja Leste, `4`=Loja Oeste.

Resposta (`201 Created`):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "lojista": 1,
  "lojistaCodigo": "0001",
  "lojaNome": "Loja Norte",
  "amount": 150.50,
  "type": "Credit",
  "description": "Venda de produtos",
  "transactionDate": "2025-06-16T00:00:00Z",
  "createdAt": "2025-06-16T14:30:00Z",
  "isProcessed": false
}
```

### Listar Lancamentos

```bash
curl "http://localhost:5001/api/transactions?startDate=2025-06-01&endDate=2025-06-30&pageNumber=1&pageSize=20"
```

### Obter Consolidado do Dia

```bash
curl http://localhost:5002/api/consolidations/2025-06-16
```

Resposta:
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "lojista": 1,
  "lojistaCodigo": "0001",
  "lojaNome": "Loja Norte",
  "consolidationDate": "2025-06-16T00:00:00Z",
  "debitTotal": 200.00,
  "creditTotal": 350.50,
  "dailyBalance": 150.50,
  "processedCount": 2,
  "lastUpdatedAt": "2025-06-16T14:35:00Z"
}
```

### Listar Consolidados com Filtro de Periodo

```bash
curl "http://localhost:5002/api/consolidations?startDate=2025-06-01&endDate=2025-06-30&pageNumber=1&pageSize=10"
```

---

## Testes

```bash
# Todos os testes
dotnet test tests/

# Com coverage
dotnet test tests/ /p:CollectCoverage=true

# Servico especifico
dotnet test tests/Transactions.API.Tests/
dotnet test tests/Consolidation.API.Tests/
```

### Cobertura atual

| Camada | Testes |
|--------|--------|
| `TransactionTests` (Domain) | Create valido, amount negativo, data futura, MarkAsProcessed, tipos (Theory) |
| `TransactionServiceTests` (Application) | CreateAsync, GetByIdAsync valido/invalido |
| `ConsolidationTests` (Domain) | Create, AddTransaction debito/credito/multiplos, amount invalido |
| `ConsolidationServiceTests` (Application) | ProcessTransaction nova data, ProcessTransaction data existente |

Stack: **xUnit** + **Moq** + **FluentAssertions** + **Testcontainers** (infra pronta para integration tests).

---

## Estrutura do Projeto

```
fluxo-caixa/
├── docker-compose.yml
├── SQL_CREATE_DATABASE.sql         # Script de inicializacao do banco
├── FluxoCaixa.sln
│
├── services/
│   ├── Shared/                     # Contratos, DTOs, Enums compartilhados
│   │   ├── Contracts/              # TransactionCreatedEvent, ConsolidationUpdatedEvent
│   │   ├── DTOs/                   # TransactionDto, ConsolidationDto, etc.
│   │   └── Enums/                  # Lojista
│   │
│   ├── Transactions.API/           # Servico de Transacoes (Port 5001)
│   │   └── src/
│   │       ├── Domain/             # Transaction, ITransactionRepository, IUnitOfWork
│   │       ├── Application/        # TransactionService, ITransactionService
│   │       ├── Infrastructure/     # DbContext, Repository, UnitOfWork
│   │       └── EntryPoint/         # TransactionsController
│   │
│   └── Consolidation.API/          # Servico de Consolidacao (Port 5002)
│       └── src/
│           ├── Domain/             # Consolidation, IConsolidationRepository, IUnitOfWork
│           ├── Application/        # ConsolidationService, IConsolidationService
│           ├── Infrastructure/     # DbContext, Repository, UnitOfWork,
│           │                       # TransactionCreatedEventConsumer
│           └── EntryPoint/         # ConsolidationsController
│
├── tests/
│   ├── Transactions.API.Tests/
│   │   └── Unit/ (Domain + Application)
│   └── Consolidation.API.Tests/
│       └── Unit/ (Domain + Application)
│
└── docs/
    ├── ADRs.md                     # 7 decisoes arquiteturais
    ├── API.md                      # Referencia completa de endpoints
    └── DEVELOPMENT.md              # Guia de desenvolvimento
```

---

## Desenvolvimento Local

Para rodar os servicos fora do Docker (ex: debugging):

**Terminal 1 — Transactions API**
```bash
cd services/Transactions.API
dotnet run
# http://localhost:5001/swagger
```

**Terminal 2 — Consolidation API**
```bash
cd services/Consolidation.API
dotnet run
# http://localhost:5002/swagger
```

A infraestrutura (SQL Server + RabbitMQ) deve estar rodando via `docker-compose up -d sqlserver rabbitmq`.

---

## Fluxo de Dados

### Criar Transacao

```
POST /api/transactions
  → TransactionService.CreateAsync()
  → Persiste Transaction no SQL Server
  → Publica TransactionCreatedEvent no RabbitMQ
  → TransactionCreatedEventConsumer (Consolidation.API)
  → ConsolidationService.ProcessTransactionAsync()
  → Cria/atualiza Consolidation (Lojista + Data) no SQL Server
  → Publica ConsolidationUpdatedEvent no RabbitMQ
```

### Obter Consolidacao

```
GET /api/consolidations/{date}
  → ConsolidationService.GetByDateAsync(date)
  → SQL Server
  → Retorna ConsolidationDto
```

---

## Seguranca

Implementado:
- Validacao de entrada (Amount > 0, data nao muito futura)
- SQL Injection prevention (EF Core parametrizado)
- Health checks para monitoramento
- Error handling robusto com logging estruturado

Planejado (v1.1):
- JWT Authentication
- Role-based Authorization
- Rate limiting
- HTTPS enforcement em producao

---

## Performance e Escalabilidade

| Metrica | Alvo |
|---------|------|
| Latencia P50 | < 100ms |
| Latencia P99 | < 500ms |
| Throughput | 50+ req/s |
| Disponibilidade | 99.5% |

### Escalar Consolidation API

```bash
docker-compose up -d --scale consolidation-api=3
```

---

## Troubleshooting

### Containers nao iniciam

```bash
docker-compose logs transactions-api
docker-compose logs consolidation-api
docker-compose logs sqlserver
```

### SQL Server demora a subir

O container `sql-init` aguarda o SQL Server estar pronto antes de criar o banco. Se falhar, reinicie:

```bash
docker-compose restart sql-init
```

### Mensagens nao sendo consumidas

```bash
# Verificar RabbitMQ
# http://localhost:15672 → Queues → verificar se ha mensagens pendentes

docker-compose logs consolidation-api
docker-compose restart rabbitmq
```

### Porta em uso

Altere a porta no `docker-compose.yml`:
```yaml
ports:
  - "5011:5001"  # expoe na 5011 em vez de 5001
```

---

## Tecnologias

| Camada | Tecnologia |
|--------|------------|
| **API** | ASP.NET Core 8 |
| **Linguagem** | C# 12 |
| **Banco** | SQL Server 2022 |
| **ORM** | Entity Framework Core 8 |
| **Mensageria** | RabbitMQ 3.12 via MassTransit 8 |
| **Testes** | xUnit + Moq + FluentAssertions + Testcontainers |
| **Container** | Docker + Docker Compose |
| **Logging** | Serilog 4 |
| **Docs** | Swagger / OpenAPI |
| **CI/CD** | GitHub Actions |

---

## Documentacao

| Arquivo | Conteudo |
|---------|----------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | Diagrama, padroes, decisoes, schema do banco |
| [docs/ADRs.md](docs/ADRs.md) | 7 Architecture Decision Records detalhados |
| [docs/API.md](docs/API.md) | Referencia completa de endpoints |
| [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) | Setup dev, padroes, debugging |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Guidelines para contribuicao |
| [CHANGELOG.md](CHANGELOG.md) | Historico de versoes |

---

## Licenca

MIT License — veja [LICENSE](LICENSE) para detalhes.
