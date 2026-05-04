# Fluxo de Caixa — Carrefour Banco

Sistema de controle de fluxo de caixa desenvolvido como desafio técnico. Implementa uma arquitetura de microsserviços com comunicação assíncrona via mensageria, garantindo que o serviço de lançamentos permaneça disponível mesmo quando o serviço de consolidado estiver temporariamente indisponível.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 9 / C# |
| Arquitetura | Clean Architecture · DDD · CQRS (MediatR) · Microsserviços |
| Mensageria | MassTransit + RabbitMQ |
| Cache | Redis (TTL 60s) |
| Banco de Dados | SQL Server (EF Core 9 · Code First Migrations) |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Frontend | Angular 18 (Standalone Components) |
| Testes | xUnit · FluentAssertions · Moq (38 testes) |
| Infraestrutura | Docker + Docker Compose |
| Resiliência | Polly (Retry com backoff exponencial) · Health Checks |
| Segurança | JWT Bearer · Rate Limiting (sliding window) |
| Observabilidade | Serilog (structured logging) |

---

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 18+](https://nodejs.org/) (para o frontend)
- [k6](https://k6.io/docs/get-started/installation/) (opcional, para teste de carga)

---

## Como Executar

### Opção 1 — Docker Compose (recomendado)

Sobe todos os microsserviços, banco, mensageria e cache com um único comando:

```bash
docker compose up --build -d
```

Em seguida, inicie o frontend:

```bash
cd frontend/fluxo-caixa-web
npm install
npm start
```

### Opção 2 — Execução local (.NET CLI)

```bash
# Terminal 1 — Infraestrutura
docker compose up -d sqlserver rabbitmq redis

# Terminal 2 — API de Lançamentos (porta 5001)
cd src/Services/FluxoCaixa.Lancamentos.Api
dotnet run

# Terminal 3 — API do Consolidado (porta 5002)
cd src/Services/FluxoCaixa.Consolidado.Api
dotnet run

# Terminal 4 — API Gateway (porta 5000)
cd src/Gateway/FluxoCaixa.Gateway
dotnet run

# Terminal 5 — Frontend
cd frontend/fluxo-caixa-web
npm install
npm start
```

> As migrations são aplicadas automaticamente na inicialização de cada serviço.

---

## URLs de Acesso

| Recurso | URL |
|---|---|
| Frontend | http://localhost:4200 |
| Gateway (entry point da API) | http://localhost:5000 |
| Swagger — Lançamentos | http://localhost:5001/swagger |
| Swagger — Consolidado | http://localhost:5002/swagger |
| RabbitMQ Management | http://localhost:15672 (guest / guest) |

---

## Autenticação

Todos os endpoints exigem autenticação JWT. Para obter um token:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{ "email": "comerciante@teste.com", "senha": "Senha@123" }'
```

> **Nota:** as credenciais são fixas para este desafio técnico. Em produção, a autenticação utilizaria um provedor de identidade (Keycloak, Azure AD B2C ou similar).

Use o token retornado no header de todas as requisições:

```
Authorization: Bearer <token>
```

---

## Contratos de API

### Lançamentos

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/api/lancamentos` | Registra um novo lançamento |
| `GET` | `/api/lancamentos?data=yyyy-MM-dd` | Lista lançamentos do dia |

**Payload de criação:**
```json
{
  "descricao": "Venda balcão",
  "valor": 150.00,
  "tipo": "CREDITO",
  "dataHora": "2026-03-30T13:00:00Z"
}
```

### Consolidado

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/api/consolidado/{data}` | Saldo consolidado de um dia |
| `GET` | `/api/consolidado/range?inicio=&fim=` | Saldo consolidado de um período |

---

## Testes

### Testes Unitários (38 testes · 100% aprovados)

```bash
dotnet test
```

| Projeto | Testes |
|---|---|
| `FluxoCaixa.Lancamentos.Tests` | 21 — domínio + command handlers + query handlers |
| `FluxoCaixa.Consolidado.Tests` | 17 — domínio + query handlers + event consumer |

### Teste de Carga (k6)

Valida o requisito de 50 req/s com menos de 5% de erros no serviço de consolidado.

```bash
k6 run ./k6/consolidado_pico.js
```

| Métrica | Meta |
|---|---|
| Taxa de erros | < 5% |
| Latência p95 | < 200ms |
| Latência p99 | < 500ms |
| Throughput mínimo | ≥ 47,5 req/s |

---

## Estrutura do Projeto

```
/FluxoCaixa
├── src/
│   ├── Services/
│   │   ├── FluxoCaixa.Lancamentos.*/     ← Microsserviço 1 (Domain/Application/Infrastructure/Api)
│   │   └── FluxoCaixa.Consolidado.*/     ← Microsserviço 2 (Domain/Application/Infrastructure/Api)
│   └── Gateway/
│       └── FluxoCaixa.Gateway/           ← YARP Gateway (JWT, CORS, Rate Limiting, Auth)
├── frontend/
│   └── fluxo-caixa-web/                  ← Angular 18 SPA
├── tests/
│   ├── FluxoCaixa.Lancamentos.Tests/     ← 21 testes unitários
│   └── FluxoCaixa.Consolidado.Tests/     ← 17 testes unitários
├── k6/
│   └── consolidado_pico.js               ← Teste de carga (50 req/s)
├── docker-compose.yml
├── arquitetura_fluxo_caixa.md            ← Documentação técnica completa
└── README.md
```

---

## Decisões Arquiteturais

Consulte [`README_arquitetura_fluxo_caixa.md`](./README_arquitetura_fluxo_caixa.md) para a documentação técnica completa, incluindo:

- Justificativa da separação em microsserviços com mensageria assíncrona
- Decisões sobre CQRS, DDD e Clean Architecture
- Diagrama de componentes e fluxo de eventos
- Estratégia de resiliência (Polly, Health Checks, idempotência via `ProcessedEvent`)
- Tratamento de race condition com `MERGE SQL + HOLDLOCK` para consistência do consolidado
- Estratégia de cache Redis com invalidação orientada a evento
- Plano de testes (unitários, integração, carga, E2E)
- Melhorias futuras (Outbox Pattern, OpenTelemetry, Kubernetes HPA)
