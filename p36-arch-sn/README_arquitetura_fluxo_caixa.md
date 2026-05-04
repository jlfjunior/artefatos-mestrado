# Fluxo de Caixa — Documentação de Arquitetura

> **Stack:** .NET 9 (C#) · Angular · SQL Server · RabbitMQ · Redis · Docker  
> **Padrão:** Microsserviços · CQRS · DDD · Clean Architecture

---

## 1. Visão Geral do Problema

Um comerciante precisa:
1. **Registrar lançamentos** (débitos e créditos) ao longo do dia.
2. **Consultar o saldo consolidado diário** — soma de todos os lançamentos de um dia.

**Restrições críticas:**
- O serviço de lançamentos **não pode ficar indisponível** se o consolidado cair.
- O consolidado recebe até **50 req/s** nos picos, com no máximo **5% de perda**.

---

## 2. Decisões Arquiteturais

### 2.1 Padrão: Microsserviços com comunicação assíncrona

| Alternativa | Prós | Contras | Decisão |
|---|---|---|---|
| Monolito | Simples, fácil de rodar | Acoplamento total, falha cascata | ❌ Descartado |
| Microsserviços síncronos (REST) | Simples de integrar | Dependência direta: se consolidado cai, lançamento pode falhar | ❌ Rejeitado para comunicação entre serviços |
| **Microsserviços + Mensageria** | Desacoplamento total, resiliente | Complexidade operacional | ✅ **Escolhido** |

**Justificativa:** O requisito não-funcional principal exige que o serviço de lançamentos seja **totalmente independente** do consolidado. A comunicação via fila (RabbitMQ) garante que os eventos de lançamento são enfileirados e processados pelo consolidado quando disponível — sem perda de dados e sem dependência em tempo real.

### 2.2 Padrão Interno: CQRS + DDD

- **Commands** → escrita de lançamentos (CreateLancamentoCommand)
- **Queries** → leitura do consolidado (GetConsolidadoDiarioQuery)
- Separação de modelos de leitura e escrita melhora escalabilidade e testabilidade.

### 2.3 Banco de Dados

| Serviço | Banco | Justificativa |
|---|---|---|
| Lançamentos | SQL Server | ACID, consistência transacional para débitos/créditos |
| Consolidado | SQL Server + Redis | Redis como cache de leitura para aguentar 50 req/s |

---

## 3. Arquitetura dos Componentes

```
┌─────────────────────────────────────────────────────────────┐
│                        FRONTEND                             │
│              Angular (SPA)  — porta 4200                    │
│   (Módulos: Lançamentos / Dashboard Consolidado)            │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/REST (JWT)
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                     API GATEWAY                             │
│    (YARP Reverse Proxy)                                     │
│    Roteamento, Rate Limiting, Autenticação centralizada     │
└────────────┬───────────────────────────┬────────────────────┘
             │                           │
             ▼                           ▼
┌────────────────────────┐   ┌───────────────────────────────┐
│  Serviço de Lançamentos│   │  Serviço de Consolidado Diário│
│  (LancamentoService)   │   │  (ConsolidadoService)         │
│  .NET 9 Web API         │   │  .NET 9 Web API               │
│  Porta: 5001            │   │  Porta: 5002                  │
│                        │   │                                │
│  ┌──────────────────┐  │   │  ┌────────────────────────┐   │
│  │ SQL Server       │  │   │  │ SQL Server (escrita)    │   │
│  │ (Lançamentos)    │  │   │  │ Redis   (leitura/cache) │   │
│  └──────────────────┘  │   │  └────────────────────────┘   │
│                        │   │                                │
└────────────┬───────────┘   └────────────┬──────────────────┘
             │                            ▲
             │     LancamentoCriadoEvent  │
             └──────────► RabbitMQ ───────┘
                          (Exchange: fluxo-caixa)
                          (Queue: consolidado.processar)
```

---

## 4. Estrutura dos Projetos

```
/FluxoCaixa
├── src/
│   ├── Services/
│   │   ├── FluxoCaixa.Lancamentos/           ← Microsserviço 1
│   │   │   ├── Api/                          ← Controllers, Middlewares
│   │   │   ├── Application/                  ← Commands, Queries (CQRS/MediatR)
│   │   │   ├── Domain/                       ← Entidades, VOs, Regras de Negócio
│   │   │   └── Infrastructure/               ← EF Core, RabbitMQ Publisher
│   │   │
│   │   └── FluxoCaixa.Consolidado/           ← Microsserviço 2
│   │       ├── Api/
│   │       ├── Application/
│   │       ├── Domain/
│   │       └── Infrastructure/               ← EF Core, Redis, RabbitMQ Consumer
│   │
│   └── Gateway/
│       └── FluxoCaixa.Gateway/               ← YARP API Gateway
│
├── frontend/
│   └── fluxo-caixa-web/                      ← Angular App
│       ├── src/app/
│       │   ├── core/                         ← AuthGuard, JWT Interceptor, AuthService
│       │   ├── shared/                        ← Componentes reutilizáveis (cards, badges, loading)
│       │   ├── layout/                        ← Shell, Sidebar, Header (Banco Carrefour)
│       │   ├── dashboard/                     ← Tela: Dashboard de Fluxo de Caixa
│       │   ├── lancamentos/                   ← Tela: Registro e listagem de lançamentos
│       │   ├── consolidado/                   ← Tela: Consolidado Diário (tabela + gráfico)
│       │   ├── relatorio/                     ← Tela: Relatório Consolidado com exportação
│       │   └── sistema/                       ← Tela: Arquitetura do Sistema (documentação)
│       ├── assets/
│       └── environments/                      ← environment.ts / environment.prod.ts
│
├── tests/
│   ├── FluxoCaixa.Lancamentos.Tests/
│   └── FluxoCaixa.Consolidado.Tests/
│
├── docker-compose.yml
└── README.md
```

---

## 5. Contratos de API

### 5.1 Serviço de Lançamentos (porta 5001)

#### `POST /api/lancamentos`
Registra um novo lançamento.

**Request:**
```json
{
  "tipo": "CREDITO",                      // "CREDITO" | "DEBITO"
  "valor": 150.00,
  "descricao": "Venda balcão",
  "dataHora": "2026-03-30T10:35:00-03:00" // opcional, default: now (UTC-3)
}
```

> **Nota:** `dataHora` deve sempre ser enviado com timezone offset. O sistema armazena em UTC internamente. A data do consolidado é derivada do campo `dataHora` convertido para o fuso local do comerciante.

**Response `201 Created`:**
```json
{
  "id": "uuid",
  "tipo": "CREDITO",
  "valor": 150.00,
  "descricao": "Venda balcão",
  "dataHora": "2026-03-30T13:35:00Z",     // armazenado em UTC
  "dataHoraLocal": "2026-03-30T10:35:00-03:00",
  "criadoEm": "2026-03-30T13:35:00Z"
}
```

#### `GET /api/lancamentos?data=2026-03-30&fusoHorario=America/Sao_Paulo`
Lista os lançamentos de uma data (dia calendário no fuso informado). O parâmetro `fusoHorario` é opcional, default: `America/Sao_Paulo`.

---

### 5.2 Serviço de Consolidado Diário (porta 5002)

#### `GET /api/consolidado/{data}`
Retorna o saldo consolidado do dia.

**Response `200 OK`:**
```json
{
  "data": "2026-03-30",
  "fusoHorario": "America/Sao_Paulo",
  "periodoInicio": "2026-03-30T03:00:00Z",  // 00:00 local em UTC
  "periodoFim": "2026-03-31T02:59:59Z",     // 23:59:59 local em UTC
  "totalCreditos": 3500.00,
  "totalDebitos": 1200.00,
  "saldoConsolidado": 2300.00,
  "quantidadeLancamentos": 12,
  "ultimaAtualizacao": "2026-03-30T13:30:00Z"
}
```

> **Cache:** Resposta cacheada no Redis por 60 segundos. Para o dia atual, cache é invalidado ao receber novo evento via fila.

---

## 6. Modelo de Domínio

### Entidade: `Lancamento`
```csharp
public class Lancamento
{
    public Guid Id { get; private set; }
    public TipoLancamento Tipo { get; private set; }  // CREDITO | DEBITO
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }

    // Sempre em UTC — nunca DateOnly ou DateTime sem Kind
    public DateTimeOffset DataHora { get; private set; }

    public DateTimeOffset CriadoEm { get; private set; }

    // Data do consolidado é calculada pela data local do comerciante
    public DateOnly DataConsolidado(string fusoHorario)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(fusoHorario);
        var local = TimeZoneInfo.ConvertTime(DataHora, tz);
        return DateOnly.FromDateTime(local.DateTime);
    }

    public static Lancamento Criar(TipoLancamento tipo, decimal valor,
                                   string descricao, DateTimeOffset dataHora)
    {
        if (valor <= 0) throw new DomainException("Valor deve ser positivo.");
        return new Lancamento
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Valor = valor,
            Descricao = descricao,
            // Normaliza para UTC na entrada
            DataHora = dataHora.ToUniversalTime(),
            CriadoEm = DateTimeOffset.UtcNow
        };
    }
}
```

### Entidade: `ConsolidadoDiario`
```csharp
public class ConsolidadoDiario
{
    public DateOnly Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal SaldoConsolidado => TotalCreditos - TotalDebitos;

    public void AplicarLancamento(TipoLancamento tipo, decimal valor)
    {
        if (tipo == TipoLancamento.Credito) TotalCreditos += valor;
        else TotalDebitos += valor;
    }
}
```

---

## 7. Fluxo de Eventos (Mensageria)

```
1. POST /api/lancamentos
2. LancamentoService persiste no SQL Server
3. LancamentoService publica evento: LancamentoCriadoEvent → RabbitMQ
4. ConsolidadoService (Consumer) recebe o evento
5. ConsolidadoService atualiza ConsolidadoDiario no SQL Server
6. ConsolidadoService invalida cache Redis da data correspondente
7. Próxima chamada GET /api/consolidado/{data} acerta o banco e atualiza Redis
```

**Resiliência da fila:**
- Fila do RabbitMQ é **durable** e **persistent** (mensagens sobrevivem a restart).
- Consumer usa **ack manual**: só faz ack após persistência confirmada.
- Dead Letter Queue (DLQ) para reprocessamento de falhas.

---

## 8. Segurança

| Aspecto | Implementação |
|---|---|
| Autenticação | JWT Bearer Token (Identity/ASP.NET Core) |
| Autorização | Roles: `Comerciante` |
| HTTPS | Habilitado via certificado (dev: `dotnet dev-certs`) |
| Rate Limiting | Configurado no API Gateway (ASP.NET Core Rate Limiting) |
| Secrets | `appsettings.json` + variáveis de ambiente (Docker secrets em prod) |
| CORS | Configurado para permitir apenas a origem Angular |

---

## 9. Resiliência e Disponibilidade

### Lançamentos (alta prioridade)
- **Independência total:** Não chama o ConsolidadoService diretamente.
- **Circuit Breaker (Polly):** Proteção na publicação para RabbitMQ (retry + circuit breaker).
- **Health Checks:** `/health` exposto.

### Consolidado (tolerância a falha)
- **Cache Redis:** Absorve picos de leitura (50 req/s com TTL de 60s = ~3.000 req por ciclo de cache).
- **Processamento assíncrono:** Consome fila em background (IHostedService).
- **Idempotência:** Consumer verifica se evento já foi processado pelo `EventId`.

### Cálculo de capacidade para 50 req/s com 5% de perda:
- 50 req/s × 95% = 47,5 req/s atendidas
- Redis com TTL=60s: a cada 60s apenas 1 req vai ao banco por data consultada
- Para 50 datas distintas simultâneas: 50/60 ≈ 0,8 req/s no banco — confortável

---

## 10. Tecnologias e Pacotes NuGet

### Lançamentos Service
| Pacote | Finalidade |
|---|---|
| `MediatR` | CQRS (Commands/Queries) |
| `FluentValidation` | Validação de entrada |
| `Microsoft.EntityFrameworkCore` | ORM |
| `RabbitMQ.Client` / `MassTransit` | Publisher de eventos |
| `Polly` | Retry / Circuit Breaker |
| `Serilog` | Logging estruturado |

### Consolidado Service
| Pacote | Finalidade |
|---|---|
| `MediatR` | CQRS |
| `MassTransit.RabbitMQ` | Consumer de eventos |
| `StackExchange.Redis` | Cache distribuído |
| `Microsoft.EntityFrameworkCore` | ORM |
| `Serilog` | Logging |

### Frontend Angular
| Biblioteca | Finalidade |
|---|---|
| `@angular/material` | UI Components (cards, tables, buttons, dialogs) |
| `@angular/cdk` | Overlays e portais para modais |
| `ngx-charts` | Gráfico de linha "Trajetória do Saldo" |
| `@angular/router` | Roteamento SPA com lazy-loading por módulo |
| `HttpClient` + `interceptors` | Comunicação com Gateway (JWT automático, error handling) |
| `@ngx-translate/core` | i18n PT/EN (ambas versões de tela existem no Stitch) |
| `file-saver` + `jspdf` | Exportação local PDF/CSV no Relatório Consolidado |
| `date-fns` | Formatação de `DateTimeOffset` e cálculo de períodos |

---

## 11. Infraestrutura (docker-compose.yml)

```yaml
version: '3.8'
services:
  lancamentos-api:
    build: ./src/Services/FluxoCaixa.Lancamentos
    ports: ["5001:8080"]
    depends_on: [sqlserver, rabbitmq]
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
      - RabbitMQ__Host=rabbitmq

  consolidado-api:
    build: ./src/Services/FluxoCaixa.Consolidado
    ports: ["5002:8080"]
    depends_on: [sqlserver, rabbitmq, redis]
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
      - Redis__ConnectionString=redis:6379

  gateway:
    build: ./src/Gateway/FluxoCaixa.Gateway
    ports: ["5000:8080"]
    depends_on: [lancamentos-api, consolidado-api]

  angular-frontend:
    build: ./frontend/fluxo-caixa-web
    ports: ["4200:80"]

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "SuaSenha@123"
      ACCEPT_EULA: "Y"
    ports: ["1433:1433"]

  rabbitmq:
    image: rabbitmq:3-management
    ports: ["5672:5672", "15672:15672"]

  redis:
    image: redis:alpine
    ports: ["6379:6379"]
```

---

## 12. Plano de Testes Completo

> **Ferramentas:** xUnit · FluentAssertions · Moq · Testcontainers · k6 · Jasmine/Karma · Cypress

---

### 12.1 Pirâmide de Testes

```
             ▲  E2E (Cypress)         — poucos, lentos, alto valor
            ▲▲  Integração (.NET)     — médio volume, containers reais
           ▲▲▲  Unitários (xUnit)    — muitos, rápidos, isolados
          ▲▲▲▲  Frontend Unit (Karma) — componentes isolados
```

---

### 12.2 Backend — Testes Unitários

**Framework:** `xUnit` + `FluentAssertions` + `Moq`  
**Projeto:** `tests/FluxoCaixa.Lancamentos.Tests` e `tests/FluxoCaixa.Consolidado.Tests`

#### 12.2.1 Domínio — `Lancamento`

| Cenário de Teste | Resultado Esperado |
|---|---|
| Criar lançamento CRÉDITO com valor positivo | Objeto criado, `DataHora` em UTC |
| Criar lançamento DÉBITO com valor positivo | Objeto criado corretamente |
| Criar lançamento com valor zero | `DomainException` lançada |
| Criar lançamento com valor negativo | `DomainException` lançada |
| `DataConsolidado()` com fuso `America/Sao_Paulo` às 23h UTC | Retorna o dia correto no fuso local |
| `DataHora` normalizado para UTC na construção | `DateTimeOffset.Kind == Utc` |

```csharp
[Fact]
public void Criar_ValorZero_DeveLancarDomainException()
{
    var act = () => Lancamento.Criar(TipoLancamento.Credito, 0m, "Teste",
                                     DateTimeOffset.UtcNow);
    act.Should().Throw<DomainException>().WithMessage("Valor deve ser positivo.");
}

[Fact]
public void DataConsolidado_HorarioMeiaNorteUTC_RetornaDiaAnteriorSaoPaulo()
{
    // 00:30 UTC = 21:30 do dia anterior em BRT (UTC-3)
    var lancamento = Lancamento.Criar(TipoLancamento.Credito, 100m, "Test",
                                      new DateTimeOffset(2026, 3, 30, 0, 30, 0, TimeSpan.Zero));
    var data = lancamento.DataConsolidado("America/Sao_Paulo");
    data.Should().Be(new DateOnly(2026, 3, 29));
}
```

#### 12.2.2 Domínio — `ConsolidadoDiario`

| Cenário de Teste | Resultado Esperado |
|---|---|
| Aplicar crédito | `TotalCreditos` incrementado |
| Aplicar débito | `TotalDebitos` incrementado |
| `SaldoConsolidado` = créditos - débitos | Cálculo correto |
| Múltiplos lançamentos mistos | Saldo líquido correto |

#### 12.2.3 Application — Handlers CQRS

| Handler | Cenário | Mock necessário | Esperado |
|---|---|---|---|
| `CreateLancamentoHandler` | Lançamento válido | `ILancamentoRepository`, `IEventPublisher` | Salva + publica evento |
| `CreateLancamentoHandler` | Falha ao publicar evento | `IEventPublisher` lança exceção | Propaga exceção (Outbox pattern futuro) |
| `GetLancamentosHandler` | Query com data válida | `ILancamentoRepository` retorna lista | Retorna DTOs mapeados |
| `GetConsolidadoHandler` | Data com cache hit | `IRedisCache` retorna dado | Retorna cache sem acessar DB |
| `GetConsolidadoHandler` | Data sem cache | `IRedisCache` retorna null, DB responde | Salva no cache e retorna |

```csharp
[Fact]
public async Task Handle_LancamentoValido_DeveSalvarEPublicarEvento()
{
    // Arrange
    var repoMock = new Mock<ILancamentoRepository>();
    var publisherMock = new Mock<IEventPublisher>();
    var handler = new CreateLancamentoHandler(repoMock.Object, publisherMock.Object);
    var command = new CreateLancamentoCommand("CREDITO", 100m, "Teste",
                                              DateTimeOffset.UtcNow);
    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    repoMock.Verify(r => r.AddAsync(It.IsAny<Lancamento>()), Times.Once);
    publisherMock.Verify(p => p.PublishAsync(It.IsAny<LancamentoCriadoEvent>()),
                         Times.Once);
    result.Id.Should().NotBeEmpty();
}
```

#### 12.2.4 Comandos de Execução

```bash
# Rodar todos os testes com coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Gerar relatório HTML (ReportGenerator)
reportgenerator -reports:coverage/**/coverage.cobertura.xml \
                -targetdir:coverage/html -reporttypes:Html

# Meta: >= 80% de cobertura de linhas
```

---

### 12.3 Backend — Testes de Integração

**Framework:** `Microsoft.AspNetCore.Mvc.Testing` + `Testcontainers.MsSql` + `Testcontainers.Redis`

> Containers efêmeros são criados e destruídos a cada execução. Não requerem dependências locais.

#### 12.3.1 Serviço de Lançamentos — Endpoints

| Endpoint | Cenário | Payload / Parâmetros | Status Esperado | Verificações |
|---|---|---|---|---|
| `POST /api/lancamentos` | Lançamento CRÉDITO válido | `tipo, valor, descricao, dataHora` | `201 Created` | Body com `id` UUID, `dataHora` em UTC |
| `POST /api/lancamentos` | Valor negativo | `valor: -50` | `400 Bad Request` | Mensagem de erro em `errors` |
| `POST /api/lancamentos` | Sem autenticação | Header sem JWT | `401 Unauthorized` | — |
| `POST /api/lancamentos` | Descrição vazia | `descricao: ""` | `400 Bad Request` | Campo `descricao` em erros |
| `GET /api/lancamentos` | Data com lançamentos | `?data=2026-03-30` | `200 OK` | Lista não vazia |
| `GET /api/lancamentos` | Data sem lançamentos | `?data=2099-01-01` | `200 OK` | Lista vazia `[]` |
| `GET /api/lancamentos` | Data inválida | `?data=abc` | `400 Bad Request` | — |
| `GET /health` | Serviço saudável | — | `200 OK` | `status: "Healthy"` |

#### 12.3.2 Serviço de Consolidado — Endpoints

| Endpoint | Cenário | Status Esperado | Verificações |
|---|---|---|---|
| `GET /api/consolidado/{data}` | Dia com lançamentos processados | `200 OK` | `saldoConsolidado` correto |
| `GET /api/consolidado/{data}` | Dia sem nenhum lançamento | `200 OK` | Todos os valores = 0 |
| `GET /api/consolidado/{data}` | Segunda chamada (cache) | `200 OK` | Header `X-Cache: HIT` |
| `GET /api/consolidado/range` | Range de 7 dias | `200 OK` | Array com 7 objetos |

#### 12.3.3 Teste de Fluxo End-to-End (backend)

```csharp
[Fact]
public async Task FluxoCompleto_CriarLancamento_ConsolidaCorretamente()
{
    // 1. Criar lançamento via API de Lançamentos
    var payload = new { tipo = "CREDITO", valor = 500m,
                        descricao = "Venda", dataHora = "2026-03-30T10:00:00-03:00" };
    var postResponse = await _lancamentosClient.PostAsJsonAsync("/api/lancamentos", payload);
    postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

    // 2. Aguardar processamento assíncrono (RabbitMQ consumer)
    await Task.Delay(500); // Em testes, usar polling ou await de evento

    // 3. Verificar consolidado atualizado
    var getResponse = await _consolidadoClient.GetAsync("/api/consolidado/2026-03-30");
    var consolidado = await getResponse.Content.ReadFromJsonAsync<ConsolidadoDiarioDto>();
    consolidado!.TotalCreditos.Should().Be(500m);
    consolidado.SaldoConsolidado.Should().Be(500m);
}
```

---

### 12.4 Backend — Testes de Contrato (Consumer-Driven)

> Valida que o evento publicado pelo Lançamentos Service é compatível com o esperado pelo Consolidado Service.

**Ferramenta:** `PactNet` (Pact Contract Testing)

| Contrato | Producer | Consumer | Campos validados |
|---|---|---|---|
| `LancamentoCriadoEvent` | LancamentosService | ConsolidadoService | `eventId`, `tipo`, `valor`, `dataHora` (UTC), `lancamentoId` |

```csharp
// Exemplo de pact — consumer side
[Fact]
public void ConsolidadoService_EsperaEvento_ComCamposObrigatorios()
{
    _pact.UponReceiving("um LancamentoCriadoEvent")
         .WithContent(Match.Type(new {
             eventId    = "uuid",
             lancamentoId = "uuid",
             tipo       = "CREDITO",
             valor      = 150.00m,
             dataHora   = "2026-03-30T13:35:00Z"
         }))
         .Verify<LancamentoCriadoEvent>(evt =>
         {
             evt.Tipo.Should().NotBeNullOrEmpty();
             evt.Valor.Should().BeGreaterThan(0);
             evt.DataHora.Kind.Should().Be(DateTimeKind.Utc);
         });
}
```

---

### 12.5 Backend — Testes de Carga

**Ferramenta:** `k6` (JavaScript)  
**Cenário principal:** Pico no Consolidado — 50 req/s com no máximo 5% de perda.

```javascript
// k6/consolidado_pico.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    pico_consolidado: {
      executor: 'constant-arrival-rate',
      rate: 50,               // 50 requisições por segundo
      timeUnit: '1s',
      duration: '2m',         // 2 minutos de pico
      preAllocatedVUs: 60,
      maxVUs: 100,
    },
  },
  thresholds: {
    http_req_failed:   ['rate<0.05'],    // < 5% de erros
    http_req_duration: ['p(95)<200'],    // p95 < 200ms
    http_req_duration: ['p(99)<500'],    // p99 < 500ms
  },
};

export default function () {
  const data = '2026-03-30';
  const res = http.get(`http://localhost:5000/api/consolidado/${data}`, {
    headers: { Authorization: `Bearer ${__ENV.TOKEN}` },
  });
  check(res, {
    'status 200': (r) => r.status === 200,
    'tem saldo':  (r) => JSON.parse(r.body).saldoConsolidado !== undefined,
  });
}
```

```bash
# Executar
k6 run k6/consolidado_pico.js -e TOKEN=<jwt>

# Pico de lançamentos (escrita)
k6 run k6/lancamentos_escrita.js --vus 20 --duration 60s
```

**Metas de aceite:**

| Métrica | Meta |
|---|---|
| Taxa de erros | < 5% |
| Latência p95 | < 200ms |
| Latência p99 | < 500ms |
| Throughput mínimo | ≥ 47,5 req/s |
| Disponibilidade lançamentos durante falha do consolidado | 100% |

---

### 12.6 Frontend — Testes Unitários de Componentes

**Framework:** `Jasmine` + `Karma` (padrão Angular CLI) + `@angular/testing`

#### 12.6.1 Componentes Testados

| Componente | Cenários |
|---|---|
| `FinancialSummaryCardsComponent` | Renderiza valores BRL corretamente; badge verde para crédito, vermelho para débito |
| `QuickAddFormComponent` | Submit válido chama `LancamentosService.criarLancamento()`; campos inválidos bloqueiam submit |
| `RecentTransactionsTableComponent` | Renderiza lista vazia com mensagem; renderiza 10 itens corretamente |
| `SystemStatusComponent` | Exibe "Healthy" quando API retorna 200; exibe "Degraded" em erro |
| `LancamentoFormComponent` | Validação: valor ≤ 0 mostra erro; dataHora futura > 1h mostra aviso |
| `ConsolidadoComponent` | Gráfico recebe dados formatados; tabela renderiza todas as colunas |
| `RelatorioComponent` | Botão PDF chama `exportarPdf()`; botão CSV chama `exportarCsv()` |

```typescript
// dashboard.component.spec.ts
describe('FinancialSummaryCardsComponent', () => {
  let component: FinancialSummaryCardsComponent;
  let fixture: ComponentFixture<FinancialSummaryCardsComponent>;
  let consolidadoSvc: jasmine.SpyObj<ConsolidadoService>;

  beforeEach(() => {
    consolidadoSvc = jasmine.createSpyObj('ConsolidadoService',
                                          ['getConsolidado']);
    consolidadoSvc.getConsolidado.and.returnValue(of({
      saldoConsolidado: 2450890.12,
      totalCreditos:     124500.00,
      totalDebitos:       42120.45,
    }));
    TestBed.configureTestingModule({
      declarations: [FinancialSummaryCardsComponent],
      providers: [{ provide: ConsolidadoService, useValue: consolidadoSvc }],
    });
    fixture = TestBed.createComponent(FinancialSummaryCardsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('deve exibir saldo formatado em BRL', () => {
    const el = fixture.nativeElement.querySelector('[data-testid="saldo-total"]');
    expect(el.textContent).toContain('R$ 2.450.890,12');
  });

  it('badge de crédito deve ter classe "badge-credito"', () => {
    const badge = fixture.nativeElement.querySelector('[data-testid="badge-credito"]');
    expect(badge.classList).toContain('badge-credito');
  });
});
```

#### 12.6.2 Services Testados (com `HttpClientTestingModule`)

| Service | Cenários |
|---|---|
| `LancamentosService` | `criarLancamento()` faz POST correto; `getLancamentos()` envia fusoHorario no query param |
| `ConsolidadoService` | `getConsolidado()` faz GET na URL correta; `getConsolidadoRange()` retorna array |
| `AuthService` | Login armazena token no sessionStorage; logout limpa sessionStorage |
| `JwtInterceptor` | Header Authorization injetado em toda requisição autenticada |
| `ErrorInterceptor` | 401 redireciona para /login; 500 exibe toast de erro |

```bash
# Rodar testes unitários Angular
ng test --code-coverage --watch=false

# Meta: >= 70% de cobertura de statements
```

---

### 12.7 Frontend — Testes E2E (Cypress)

**Ferramenta:** `Cypress` v13+  
**Pré-requisito:** toda stack rodando via `docker compose up --build -d`

#### 12.7.1 Fluxos Cobertos

| Fluxo | Arquivo Cypress | Passos |
|---|---|---|
| Login e acesso ao Dashboard | `cypress/e2e/login.cy.ts` | Acessar `/login` → preencher credenciais → verificar redirecionamento para `/dashboard` |
| Criar lançamento CRÉDITO | `cypress/e2e/lancamento-credito.cy.ts` | Abrir Quick Add → preencher todos os campos → submeter → verificar novo item na tabela |
| Criar lançamento DÉBITO | `cypress/e2e/lancamento-debito.cy.ts` | Idem acima com tipo DÉBITO → verificar badge vermelho |
| Validação de formulário | `cypress/e2e/form-validacao.cy.ts` | Submeter formulário vazio → verificar mensagens de erro em todos os campos obrigatórios |
| Visualizar Consolidado Diário | `cypress/e2e/consolidado.cy.ts` | Navegar para `/consolidado` → verificar gráfico renderizado → trocar filtro para 90 dias → verificar atualização da tabela |
| Exportar CSV do Relatório | `cypress/e2e/relatorio-csv.cy.ts` | Navegar para `/relatorio` → clicar "Exportar CSV" → verificar download iniciado |
| Dashboard KPIs refletem novo lançamento | `cypress/e2e/dashboard-kpi.cy.ts` | Anotar saldo → criar lançamento → verificar KPI atualizado |

```typescript
// cypress/e2e/lancamento-credito.cy.ts
describe('Criar Lançamento CRÉDITO', () => {
  beforeEach(() => {
    cy.login('comerciante@teste.com', 'Senha@123'); // comando customizado
    cy.visit('/dashboard');
  });

  it('deve adicionar o crédito e aparecer na tabela', () => {
    // Preenche Quick Add Form
    cy.get('[data-testid="quick-tipo"]').select('CREDITO');
    cy.get('[data-testid="quick-valor"]').type('500');
    cy.get('[data-testid="quick-descricao"]').type('Venda balcão Cypress');
    cy.get('[data-testid="quick-submit"]').click();

    // Verifica feedback
    cy.get('[data-testid="toast-success"]').should('contain', 'Lançamento criado');

    // Verifica na tabela de recentes
    cy.get('[data-testid="recent-transactions"]')
      .should('contain', 'Venda balcão Cypress')
      .and('contain', 'CRÉDITO');
  });
});
```

#### 12.7.2 Comandos Customizados Cypress

```typescript
// cypress/support/commands.ts
Cypress.Commands.add('login', (email: string, senha: string) => {
  cy.request('POST', '/api/auth/login', { email, senha })
    .then(({ body }) => {
      sessionStorage.setItem('auth_token', body.token);
    });
});
```

```bash
# Abrir modo interativo
npx cypress open

# Executar headless (CI)
npx cypress run --browser chrome --headless

# Executar apenas E2E de lançamentos
npx cypress run --spec "cypress/e2e/lancamento-*.cy.ts"
```

---

### 12.8 Frontend — Testes de Acessibilidade

**Ferramenta:** `cypress-axe` (integrado ao Cypress)

```typescript
// cypress/e2e/acessibilidade.cy.ts
describe('Acessibilidade', () => {
  const paginas = ['/dashboard', '/lancamentos', '/consolidado', '/relatorio'];

  paginas.forEach((rota) => {
    it(`${rota} não deve ter violações WCAG AA`, () => {
      cy.visit(rota);
      cy.injectAxe();
      cy.checkA11y(null, {
        runOnly: { type: 'tag', values: ['wcag2aa'] },
      });
    });
  });
});
```

---

### 12.9 Estratégia de CI/CD — Pipeline de Testes

```yaml
# .github/workflows/tests.yml (exemplo)
jobs:
  backend-unit:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet test --collect:"XPlat Code Coverage"

  backend-integration:
    runs-on: ubuntu-latest
    services:
      # Testcontainers sobe seus próprios containers — não precisa de service aqui
    steps:
      - run: dotnet test tests/FluxoCaixa.*.IntegrationTests

  frontend-unit:
    runs-on: ubuntu-latest
    steps:
      - run: npm ci
      - run: ng test --watch=false --code-coverage

  e2e:
    runs-on: ubuntu-latest
    needs: [backend-unit, frontend-unit]
    steps:
      - run: docker compose up -d
      - run: npx cypress run --headless

  load-test:
    runs-on: ubuntu-latest
    needs: [e2e]
    steps:
      - uses: grafana/k6-action@v0.3.1
        with:
          filename: k6/consolidado_pico.js
```

---

### 12.10 Resumo das Metas de Qualidade

| Tipo | Ferramenta | Meta de Cobertura / Aceite |
|---|---|---|
| Unit Backend | xUnit + FluentAssertions | ≥ 80% line coverage |
| Unit Frontend | Jasmine + Karma | ≥ 70% statement coverage |
| Integração | Testcontainers + WebAppFactory | 100% dos endpoints cobertos |
| Contrato | PactNet | Todos os eventos validados |
| Carga | k6 | < 5% erros · p95 < 200ms · ≥ 47,5 req/s |
| E2E | Cypress | 100% dos fluxos críticos cobertos |
| Acessibilidade | cypress-axe | 0 violações WCAG AA |

---

## 13. Frontend — Telas e Módulos Angular

> Baseado no projeto Stitch **"PRD - Fluxo de Caixa Carrefour"** (ID: 4622808192350789869).  
> Design system: Banco Carrefour · Fonte: Manrope (headlines) + Inter (body) · Cor primária: `#002576` / `#0038A8`  
> Link Stitch: https://stitch.withgoogle.com/projects/4622808192350789869

### 13.1 Layout Shell — Componente Raiz

Todas as telas compartilham um layout base (`AppShellComponent`) com:
- **Sidebar vertical** com navegação: Dashboard · Lançamentos · Consolidado Diário · Relatórios
- **Botão de ação primário** `+ Adicionar Lançamento` na base da sidebar
- **Header** com logo Banco Carrefour, campo de busca global, ícones de notificação/ajuda e menu de perfil do usuário
- **Route outlet** para injeção do conteúdo de cada módulo

**Rota base:** `AppRoutingModule` com lazy-loading por feature module.

---

### 13.2 Tela: Dashboard de Fluxo de Caixa

**Rota:** `/dashboard`  
**Stitch Screen ID:** `34c7fd27a1e240a3b931f94786dd1cef`  
**Componente Angular:** `DashboardComponent` no módulo `DashboardModule`

#### Seções e Componentes

| Seção | Componente | Dados (API) |
|---|---|---|
| Resumo Financeiro | `FinancialSummaryCardsComponent` | `GET /api/consolidado/{hoje}` → saldo, créditos, débitos |
| Lançamentos Recentes | `RecentTransactionsTableComponent` | `GET /api/lancamentos?data={hoje}` |
| Formulário Rápido | `QuickAddFormComponent` | `POST /api/lancamentos` |
| Status do Sistema | `SystemStatusComponent` | `GET /health` de cada serviço via gateway |

#### Campos do Resumo Financeiro
- **Saldo Total** — `saldoConsolidado` formatado em BRL (`R$ 2.450.890,12`)
- **Créditos do Dia** — `totalCreditos` com badge verde (`+R$ 124.500,00`)
- **Débitos do Dia** — `totalDebitos` com badge vermelho (`-R$ 42.120,45`)

#### Tabela de Lançamentos Recentes
Colunas: `DataHora` · `Descrição` · `Tipo` (chip CRÉDITO/DÉBITO) · `Valor`  
Exibe os últimos 10 lançamentos. Link **"Ver todos"** navega para `/lancamentos`.

#### Quick Add Form (sidebar widget)
Campos: `Descrição` (text) · `Valor` (number) · `Tipo` (select: Débito/Crédito) · `DataHora` (datetime-local)  
Ao submeter: chama `POST /api/lancamentos`, refresh automático dos cards KPI.

#### System Status
Cards com indicador de uptime por serviço: **API Core (Lançamentos)** · **Payment Gateway (Consolidado)** · **Settlement Engine (RabbitMQ)**.  
Status via `GET /health` exposto no gateway.

---

### 13.3 Tela: Registro e Listagem de Lançamentos

**Rota:** `/lancamentos`  
**Componente Angular:** `LancamentosComponent` no módulo `LancamentosModule`

#### Subcomponentes

| Componente | Função |
|---|---|
| `LancamentoFormComponent` | Formulário de novo lançamento (modal ou painel lateral) |
| `LancamentosTableComponent` | Tabela completa com paginação e ordenação |
| `LancamentoFiltrosComponent` | Filtros por data, tipo e valor |

#### Formulário de Novo Lançamento
```
Campos:
  - Tipo *       → mat-select: CRÉDITO | DÉBITO
  - Valor *      → mat-input (currency mask, > 0)
  - Descrição *  → mat-input (max 200 chars)
  - Data e Hora  → mat-datepicker + timepicker (default: agora)
  - Fuso Horário → mat-select (default: America/Sao_Paulo)

Validações (FluentValidation espelhadas no frontend):
  - Valor > 0
  - Descrição não vazia
  - dataHora não pode ser futura além de 1h (warn, não erro)
```

#### Tabela de Lançamentos
Colunas: `DataHora (local)` · `Tipo` · `Descrição` · `Valor`  
Paginação server-side: `page` + `pageSize` nos query params.  
Ordenação: por `dataHora` desc padrão.

**Serviço Angular:**
```typescript
// lancamentos.service.ts
getLancamentos(data: string, fusoHorario = 'America/Sao_Paulo'): Observable<Lancamento[]>
criarLancamento(dto: CreateLancamentoDto): Observable<Lancamento>
```

---

### 13.4 Tela: Consolidado Diário

**Rota:** `/consolidado`  
**Stitch Screen IDs:** `727fee35749c4038b7c8a10ea43bdc49` (EN) / `5bfab440ab5e4c8c88b2ae9f0d7eff5d` (PT)  
**Componente Angular:** `ConsolidadoComponent` no módulo `ConsolidadoModule`

#### Layout
- **Header da tela:** Saldo Total Consolidado (`R$ 1.245.680,42`) + indicador de variação (`+2,4%`)
- **Gráfico de Linha** (`ngx-charts`): "Trajetória do Saldo" — eixo X: dias do período, eixo Y: saldo líquido diário
- **Tabela de Dados:** Saldo dia a dia com colunas:

| Data | Saldo de Abertura | Total Créditos | Total Débitos | Saldo Final |
|---|---|---|---|---|
| 30/03/2026 | R$ 1.121.180,42 | R$ 124.500,00 | R$ 0,00 | R$ 1.245.680,42 |

#### Filtros de Período
- **Últimos 30 dias** (default)
- **Últimos 90 dias**
- **Personalizado** → mat-date-range-picker

Parâmetro enviado: `GET /api/consolidado/range?inicio={data}&fim={data}` para períodos (30/90 dias) ou `GET /api/consolidado/{data}` para consulta de um único dia.

**Serviço Angular:**
```typescript
// consolidado.service.ts
getConsolidado(data: string): Observable<ConsolidadoDiario>
getConsolidadoRange(inicio: string, fim: string): Observable<ConsolidadoDiario[]>
```

---

### 13.5 Tela: Relatório Consolidado

**Rota:** `/relatorio`  
**Stitch Screen ID:** `a94efd1e2410469580719c2985df7715`  
**Componente Angular:** `RelatorioComponent` no módulo `RelatorioModule`

#### Funcionalidades
- Mesmas seções do Consolidado Diário, com layout editorial expandido
- **Exportação:**
  - `Exportar PDF` → `jspdf` + `html2canvas` captura a tabela
  - `Exportar CSV` → `file-saver` serializa os dados
- **Busca inline:** campo de busca por valor ou data (filtra a tabela client-side)

#### Ações disponíveis
```
[ Últimos 30 dias ] [ Últimos 90 dias ] [ Personalizado ]
                                   [ ↓ PDF ] [ ↓ CSV ]
```

---

### 13.6 Tela: Arquitetura do Sistema

**Rota:** `/sistema`  
**Stitch Screen IDs:** `8c6ed133634a4de1b5050950ef3452de` (EN) / `72b83dde2afc494990677cc8df687cb2` (PT)  
**Componente Angular:** `SistemaComponent` (módulo estático — apenas leitura)

Tela de documentação técnica embutida no frontend para apresentação do sistema. Exibe:
- Diagrama de arquitetura de microsserviços
- Descrição dos pilares: CQRS, RabbitMQ, Redis, SQL Server
- Métricas de desempenho alvo (99,99% disponibilidade, 50 req/s)

> **Nota:** Esta tela não consome APIs de negócio. É voltada para demonstração do desafio técnico e pode ser removida em produção.

---

### 13.7 Mapa de Rotas Angular

```typescript
// app-routing.module.ts
const routes: Routes = [
  { path: '',            redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard',  loadChildren: () => import('./dashboard/dashboard.module') },
  { path: 'lancamentos',loadChildren: () => import('./lancamentos/lancamentos.module'),
    canActivate: [AuthGuard] },
  { path: 'consolidado',loadChildren: () => import('./consolidado/consolidado.module'),
    canActivate: [AuthGuard] },
  { path: 'relatorio',  loadChildren: () => import('./relatorio/relatorio.module'),
    canActivate: [AuthGuard] },
  { path: 'sistema',    loadChildren: () => import('./sistema/sistema.module') },
  { path: 'login',      loadChildren: () => import('./auth/auth.module') },
  { path: '**',         redirectTo: 'dashboard' }
];
```

### 13.8 Core Services

| Serviço | Responsabilidade |
|---|---|
| `AuthService` | Login/logout, armazenar JWT no `sessionStorage` |
| `JwtInterceptor` | Injeta `Authorization: Bearer <token>` em toda requisição |
| `ErrorInterceptor` | Redireciona para `/login` em 401, exibe toast em 4xx/5xx |
| `LancamentosService` | Chamadas ao `POST/GET /api/lancamentos` |
| `ConsolidadoService` | Chamadas ao `GET /api/consolidado/:data` e range |
| `HealthService` | `GET /health` nos serviços para o widget de status |

### 13.9 Design Tokens (alinhado ao Stitch)

```scss
// styles/_tokens.scss
--color-primary:      #002576;  // primary
--color-primary-ctr:  #0038A8;  // primary_container
--color-secondary:    #00658D;
--color-surface:      #FAF8FF;
--color-on-surface:   #1A1B22;
--color-error:        #BA1A1A;

--font-headline:      'Manrope', sans-serif;  // headlines / display
--font-body:          'Inter', sans-serif;    // body / labels

--radius-md:          4px;   // roundness: ROUND_FOUR
--radius-full:        9999px; // botões primários
```

---

## 14. Melhorias Futuras (Out of Scope)

> Itens que demonstram visão arquitetural avançada, mas que não foram implementados por limitação de tempo:

- **Event Sourcing:** Persistir todos os eventos de lançamento como fonte da verdade, reconstruindo o estado do consolidado a qualquer ponto no tempo.
- **CQRS Read Database:** Base de dados separada para leituras do consolidado (projeções dedicadas).
- **Kubernetes (HPA):** Auto-scaling horizontal do ConsolidadoService baseado em uso de CPU/req/s.
- **OpenTelemetry + Jaeger:** Rastreamento distribuído entre os serviços via trace IDs.
- **Outbox Pattern:** Garantia de entrega de eventos mesmo em caso de falha após commit no banco (evita dupla escrita não-atômica).
- **API Versioning:** Versionamento de endpoints para evolução sem breaking changes.
- **Autenticação OAuth2/OIDC:** Integração com Identity Server / Keycloak para cenário multi-tenant.

---

## 15. Como Rodar Localmente

```bash
# Subir toda a stack (migrations aplicadas automaticamente na inicialização)
docker compose up --build -d

# Acesse:
# Frontend:    http://localhost:4200
# Gateway:     http://localhost:5000
# Swagger MS1: http://localhost:5001/swagger
# Swagger MS2: http://localhost:5002/swagger
# RabbitMQ UI: http://localhost:15672 (guest/guest)
```

**Credenciais:** `comerciante@teste.com` / `Senha@123`

---

*Documento gerado em 30/03/2026 — Desafio Técnico Fluxo de Caixa*
