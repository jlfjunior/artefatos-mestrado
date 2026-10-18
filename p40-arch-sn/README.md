# FluxoCaixa

Sistema de controle de fluxo de caixa diário com lançamentos (débitos e créditos) e relatório consolidado diário, desenvolvido com arquitetura de microsserviços em .NET 10.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-FF6600?logo=rabbitmq)
![xUnit](https://img.shields.io/badge/Tests-xUnit-512BD4)

---

## Visão Geral da Arquitetura

```
                        ┌─────────────────┐
                        │   Cliente HTTP  │
                        └────────┬────────┘
                                 │ JWT
                        ┌────────▼────────┐
                        │   API Gateway   │
                        │     (YARP)      │
                        │  (porta 8080)   │                        
                        └──┬─────────┬────┘
                           │         │
             /api/entries  │         │  /api/consolidation
                           │         │
              ┌────────────▼──┐   ┌──▼─────────────────┐
              │ Serviço de    │   │ Serviço de         │
              │ Lançamentos   │   │ Consolidado Diário │
              │ (porta 8081)  │   │ (porta 8082)       │
              └──────┬────────┘   └────────────┬───────┘
          Publicador │                         │ Consumidor
                     │      ┌───────────┐      │
                     └─────►│ RabbitMQ  │◄─────┘
                            │ entry.    │
                            │ created   │
                            └───────────┘
                     │                        │
              ┌──────▼──────┐        ┌────────▼───────┐
              │ PostgreSQL  │        │   PostgreSQL   │
              │ db_entries  │        │db_consolidation│
              └─────────────┘        └────────────────┘
```
> Para mais detalhes veja [docs/current-architecture.png](docs/current-architecture.png)

### Fluxo Principal

1. O cliente envia uma requisição autenticada com JWT para o API Gateway (YARP)
2. O Gateway valida o token e roteia para o serviço correto
3. O Serviço de Lançamentos persiste o lançamento e publica o evento `entry.created` no RabbitMQ
4. O Serviço de Consolidado consome o evento de forma assíncrona e atualiza o saldo diário
5. Cada serviço possui seu próprio banco PostgreSQL, sem banco compartilhado

> O Serviço de Lançamentos continua operando normalmente mesmo que o Serviço de Consolidado esteja fora do ar, o RabbitMQ garante a entrega das mensagens quando o serviço voltar.

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 10.0 | Plataforma principal |
| ASP.NET Core | 10.0 | APIs REST |
| YARP | 2.3.0 | API Gateway |
| Entity Framework Core | 10.0.8 | ORM |
| PostgreSQL | 16 | Banco de dados |
| Npgsql | 10.0.2 | Driver do PostgreSQL |
| RabbitMQ | 3 | Broker de mensageria |
| RabbitMQ.Client | 7.2.1 | Driver do RabbitMQ |
| Docker | 4.75.0 | Containerização |
| Docker Compose | 3.8 | Orquestração |

---

## Estrutura do Projeto

```
FluxoCaixa/
│
├── src/
│   ├── Gateway/
│   │   └── Gateway/                           ← API Gateway com YARP
│   │
│   └── Services/
│       ├── Entries/                           ← Serviço de Lançamentos
│       │   ├── Entries.API/                   ← Controllers, Program.cs, Swagger
│       │   ├── Entries.Application/           ← Use Cases, DTOs, Interfaces
│       │   ├── Entries.Domain/                ← Entidades, Value Objects, Eventos
│       │   └── Entries.Infrastructure/        ← EF Core, Repositórios, RabbitMQ Publisher
│       │
│       └── Consolidation/                     ← Serviço de Consolidado Diário
│           ├── Consolidation.API/             ← Controllers, Program.cs, Swagger
│           ├── Consolidation.Application/     ← Use Cases, DTOs
│           ├── Consolidation.Domain/          ← Entidades, Interfaces
│           └── Consolidation.Infrastructure/  ← EF Core, Repositórios, RabbitMQ Consumer
│
├── tests/
│   ├── Entries.Tests/                         ← Testes unitários do serviço de Lançamentos
│   └── Consolidation.Tests/                   ← Testes unitários do serviço de Consolidado
│
├── docs/
│   ├── architecture-decision-records.md       ← ADRs das decisões arquiteturais
│   └── future-improvements.md                 ← Melhorias futuras planejadas
│   └── current-architecture.png               ← Desenho atual da arquitetura
│   └── future-architecture.png                ← Desenho de futuras implementações na arquitetura
│
├── docker-compose.yml
├── FluxoCaixa.sln
└── README.md
```

### Padrão Arquitetural por Serviço (DDD Leve)

```
API → Application → Domain ← Infrastructure
```

- **Domain**: entidades, value objects, eventos de domínio, interfaces de repositório
- **Application**: casos de uso (handlers), DTOs
- **Infrastructure**: implementações concretas (EF Core, RabbitMQ)
- **API**: controllers, injeção de dependência, configuração

---

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e rodando
- [Git](https://git-scm.com/) para clonar o repositório

---

## Como Rodar Localmente

### 1. Clone o repositório

```bash
git clone https://github.com/leonardoss0308/FluxoCaixa.git
cd FluxoCaixa
```

### 2. Suba os containers

Com o Docker Desktop em execução, navegue até a pasta do projeto FluxoCaixa e execute o comando

```bash
docker-compose up --build
```

Aguarde todos os serviços subirem. Na primeira execução o Docker irá baixar as imagens necessárias.

### 3. Acesse os serviços

| Serviço | URL |
|---|---|
| API Gateway | http://localhost:8080 |
| Swagger — Lançamentos | http://localhost:8081/swagger |
| Swagger — Consolidado | http://localhost:8082/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

### 4. Gere um token JWT para testar

Acesse [jwt.io](https://jwt.io), clique em JWT Encoder e configure:

**Payload:**
```json
{
  "sub": "test-user",
  "iss": "FluxoCaixa",
  "aud": "FluxoCaixa",
  "exp": 9999999999
}
```

**Signature secret:**
```
fluxo-caixa-secret-key-2026-super-secure
```

> Certifique-se de que a opção "secret base64 encoded" está desmarcada.

Use o token gerado no botão Authorize do Swagger ou no header `Authorization: Bearer {token}` no Postman.

---

## Endpoints da API

### Serviço de Lançamentos

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/entries` | Cria um novo lançamento |
| GET | `/api/entries?date={date}` | Lista lançamentos por data |

**POST /api/entries — Request:**
```json
{
  "amount": 150.00,
  "currency": "BRL",
  "type": 2,
  "description": "Venda de produto",
  "date": "2024-01-15T00:00:00"
}
```

> `type`: `1` = Débito, `2` = Crédito

**POST /api/entries — Response (201):**
```json
{
  "id": "e691a91e-f733-4c8d-aebf-0393be64c67f",
  "amount": 150.00,
  "currency": "BRL",
  "type": 2,
  "description": "Venda de produto",
  "date": "2024-01-15T00:00:00Z",
  "createdAt": "2026-05-30T02:16:35Z"
}
```

**Moedas suportadas:** `BRL`, `USD`

---

### Serviço de Consolidado Diário

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/consolidation/{date}` | Saldo consolidado de um dia |
| GET | `/api/consolidation/range?from={date}&to={date}` | Saldo consolidado por período |

**GET /api/consolidation/2024-01-15 — Response (200):**
```json
{
  "id": "a8c5b0e2-c883-4759-add4-25c04396d14c",
  "date": "2024-01-15T00:00:00",
  "totalCredits": 450.00,
  "totalDebits": 80.00,
  "balance": 370.00,
  "currency": "BRL",
  "updatedAt": "2026-05-30T02:19:00Z"
}
```

---

## Como Rodar os Testes

### Via terminal

```bash
dotnet test
```

### O que é testado

| Suite | Testes | Cobertura |
|---|---|---|
| `Entries.Tests` | 14 testes | Entidade `Entry`, Value Object `Money`, `CreateEntryHandler` |
| `Consolidation.Tests` | 11 testes | Entidade `DailyBalance`, `ProcessEntryCreatedHandler` |

---

## Decisões Arquiteturais

As decisões foram tomadas considerando os requisitos não-funcionais do desafio, especialmente a independência entre os serviços e a capacidade de suportar picos de 50 req/s com no máximo 5% de perda.

| Decisão | Escolha | Motivo |
|---|---|---|
| Padrão arquitetural | Microsserviços | Isolamento de falhas entre serviços |
| Comunicação | RabbitMQ (assíncrona) | Garante que lançamentos funcionem mesmo se o consolidado cair |
| Banco de dados | PostgreSQL (um por serviço) | Isolamento total, sem ponto único de falha |
| Arquitetura interna | DDD leve | Separação clara de responsabilidades |
| Gateway | YARP | Centraliza autenticação JWT, nativo .NET |
| Autenticação | JWT stateless | Escalável horizontalmente sem sessão |

> Para detalhes completos veja [docs/architecture-decision-records.md](docs/architecture-decision-records.md)

---

## Melhorias Futuras

> Para descrição detalhada veja [docs/future-improvements.md](docs/future-improvements.md) junto com  diagrama [docs/future-architecture.png](docs/future-architecture.png)

- **Redis** — cache do consolidado diário para suportar os picos de 50 req/s com menor latência
- **Serilog + Seq** — observabilidade centralizada com logs estruturados
- **Health Checks** — endpoints `/health` para monitoramento dos serviços
- **Endpoint de Login** — geração de token JWT via API em vez de jwt.io
- **Outbox Pattern** — garantia transacional na publicação de eventos no RabbitMQ
- **Rate Limiting** — proteção contra abuso nos endpoints públicos
- **Testes de Integração** — cobertura end-to-end com Testcontainers
