# CashFlow — Sistema de Fluxo de Caixa

[![CI](https://github.com/samuelfabel/fluxo-caixa/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/samuelfabel/fluxo-caixa/actions/workflows/ci.yml)

Projeto de **portfólio** em **C# / .NET 10** para controle de lançamentos financeiros (débitos e créditos) e consulta de **saldo diário consolidado**, com arquitetura desacoplada, resiliente e documentada (C4 + ADRs).

## Visão geral

| Serviço                    | Responsabilidade                                                                  | Porta (Docker) |
| -------------------------- | --------------------------------------------------------------------------------- | -------------- |
| **API**                    | Lançamentos + consulta de consolidado; publica mensagens após alterações no banco | 8081           |
| **Consumer.Consolidation** | Consome fila e materializa saldo diário                                           | —              |
| **Web (Blazor Server)**    | Interface do comerciante                                                          | 8080           |
| **PostgreSQL**             | Persistência relacional                                                           | 5432           |
| **RabbitMQ**               | Integração assíncrona                                                             | 5672 / 15672   |

Documentação em [`docs/`](docs/README.md) (C4, ADRs, [padrões de código](docs/code-standards.md), [ambiente AWS](docs/proposta-ambiente.md)).

## Como executar

```bash
docker compose up --build
```

- **Frontend:** <http://localhost:8080>
- **API / OpenAPI:** <http://localhost:8081/openapi/v1.json>
- **API / Scalar UI:** <http://localhost:8081/scalar/v1> (somente em Development)
- **RabbitMQ Management:** <http://localhost:15672> (guest/guest)

### Acesso à aplicação (usuários de demonstração)

Após subir o ambiente, acesse <http://localhost:8080> e faça login com um dos perfis abaixo (criados automaticamente pelas migrations):

| Perfil          | E-mail                       | Senha             | Capacidades                                                                     |
| --------------- | ---------------------------- | ----------------- | ------------------------------------------------------------------------------- |
| **Funcionário** | `funcionario@cashflow.local` | `Funcionario@123` | Criar lançamentos para clientes, listar todos os lançamentos e consultar saldos |
| **Cliente**     | `cliente@cashflow.local`     | `Cliente@123`     | Listar próprios lançamentos e consultar próprio saldo consolidado               |

Para chamadas diretas à API, obtenha um token em `POST /oauth/token` (grant type `password`) com client `cashflow.web` / secret definido no `docker-compose.yml`.

### Local (sem Docker)

```bash
dotnet restore && dotnet build && dotnet test

dotnet run --project src/CashFlow.Api
dotnet run --project src/CashFlow.Consumer.Consolidation
dotnet run --project src/CashFlow.Web
```

## Estrutura

```bash
src/
  CashFlow.Api/
  CashFlow.Consumer.Consolidation/
  CashFlow.Contracts/
  CashFlow.Domain/
  CashFlow.Application/
  CashFlow.Infrastructure/
  CashFlow.Shared/
  CashFlow.Web/
```

## Event Messages (integração)

| Evento            | Exchange                             | Mensagem                    |
| ----------------- | ------------------------------------ | --------------------------- |
| Lançamento criado | `cashflow.transaction.created.event` | `TransactionCreatedMessage` |

Body JSON no formato **CloudEvents 1.0** (`specversion`, `id`, `source`, `type`, `time`, `data`). A exchange coincide com `type`.

Lançamentos são **imutáveis** — não há update nem delete. Ver [ADR-015](docs/adr/ADR-015-lancamentos-imutaveis.md).

## Capacidade da API

Teste de carga com [k6](docs/load-test/k6-cashflow.js) em ambiente Docker local (**16/06/2026**). Cenário misto por 30 s: **15 escritas/s** (`POST /api/transactions`) + **50 leituras/s** (`GET /api/balances/today`) em paralelo.

| Métrica                  | Resultado                       |
| ------------------------ | ------------------------------- |
| Taxa de falha HTTP       | **0%** (limite &lt; 5%)         |
| Latência p(95) — escrita | **17,8 ms** (limite &lt; 1 s)   |
| Latência p(95) — leitura | **9,5 ms** (limite &lt; 500 ms) |
| Volume total             | ~1 960 requisições em 30 s      |

A API atendeu o cenário de referência com margem confortável de latência. Metodologia, parâmetros e troubleshooting: [`docs/load-test/`](docs/load-test/README.md).

## Autor

Samuel Fabel
