# ADR-008 – Execução com Docker Compose

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

O projeto deve rodar integralmente via Docker Compose para facilitar demonstração local e paridade entre ambientes.

---

## Decisão

Fornecer **`docker-compose.yml`** e **`Dockerfile` multi-stage** único (targets por serviço):

| Serviço Compose          | Target Dockerfile        | Projeto                               | Porta (host) |
| ------------------------ | ------------------------ | ------------------------------------- | ------------ |
| `postgres`               | imagem oficial           | PostgreSQL 16                         | 5432         |
| `rabbitmq`               | imagem oficial           | RabbitMQ 3 + management               | 5672 / 15672 |
| `api`                    | `api`                    | `CashFlow.Api` (lançamentos + saldos) | 8081         |
| `consumer-consolidation` | `consumer-consolidation` | `CashFlow.Consumer.Consolidation`     | —            |
| `web`                    | `web`                    | `CashFlow.Web` (Blazor Server)        | 8080         |

Variáveis de conexão (`Database__*`, `RabbitMq__*`, `ApiSettings__BaseUrl`) injetadas por `environment`.

**Ordem de subida:** health checks em PostgreSQL e RabbitMQ; `api` aguarda infraestrutura saudável; `consumer-consolidation` aguarda `api` (migrations/topologia); `web` aponta para `api` na rede interna.

**Health da API:** `/liveness` (processo ativo) e `/health` (PostgreSQL + RabbitMQ).

Comando de entrada: `docker compose up --build`.

---

## Justificativa

1. README com instrução única (`docker compose up`).
2. Elimina dependência de instalação local de Postgres/RabbitMQ.
3. Um Dockerfile compartilhado reduz duplicação de build entre API, consumer e web.

---

## Consequências Positivas

- Onboarding rápido para avaliadores
- Ambiente reproduzível
- Paridade com projetos `CashFlow.*` da solução

---

## Consequências Negativas

- Build inicial mais lento (restore/publish de múltiplos projetos)
- Debugging local pode preferir `dotnet run` híbrido

---

## Alternativas Consideradas

1. **Apenas scripts locais** — Rejeitado por requisito explícito de Docker Compose.
2. **APIs separadas (transactions-api / consolidation-api)** — Rejeitado; lançamentos e consulta de saldo ficam na mesma API (`CashFlow.Api`), com consolidado assíncrono no consumer.
