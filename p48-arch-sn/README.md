# Controle de Fluxo de Caixa Diário

Sistema que registra lançamentos diários (débito e crédito) de um comerciante e
disponibiliza o saldo consolidado por dia. O domínio é simples; o que importa
aqui é a arquitetura, montada em torno de um requisito não-funcional específico:

> O serviço de lançamentos não pode ficar indisponível se o consolidado cair.
> Em pico, o consolidado recebe 50 req/s, com até 5% de perda aceitável.

Para atender isso, os dois lados são serviços independentes que conversam só de
forma assíncrona via mensageria. Se o consolidado cair, os eventos se acumulam
na fila e o lançamento continua operando normalmente.

## Arquitetura em uma olhada

Dois microsserviços, CQRS, comunicação assíncrona:

- **Lançamentos** (write side) — recebe e persiste os lançamentos. É a fonte da
  verdade. Na mesma transação da escrita, grava o evento numa _outbox_, que é
  publicada no RabbitMQ.
- **Consolidado** (read side) — consome os eventos e mantém a projeção do saldo
  por dia. A consulta lê esse read model com cache no Redis.

O detalhamento das decisões e o diagrama estão em:

- [`docs/arquitetura/visao-geral.md`](docs/arquitetura/visao-geral.md) — visão e diagrama do fluxo
- [`docs/arquitetura/decisoes.md`](docs/arquitetura/decisoes.md) — decisões e racional
- [`docs/arquitetura/adr/`](docs/arquitetura/adr/) — ADRs das decisões principais

Cada serviço segue arquitetura hexagonal: `Domain` (núcleo puro), `Application`
(casos de uso e portas), `Infrastructure` (adapters: EF Core, MassTransit,
Redis, outbox) e `Api` (Minimal API). O contrato dos eventos
(`LancamentoRegistrado`) vive em `src/BuildingBlocks/Contracts`, compartilhado
entre produtor e consumidor.

## Stack

.NET 8 (Minimal API), PostgreSQL (EF Core/Npgsql), RabbitMQ via MassTransit
(com outbox, retry/backoff e DLQ), Redis (cache de leitura), FluentValidation,
JWT Bearer, Serilog, OpenTelemetry (OTLP) e health checks.

> **Sobre o SDK:** o único SDK .NET 8 desta máquina é um preview. Para não
> depender dele, o `global.json` fixa o SDK final 10.0.201 com
> `rollForward: latestMajor`, e todos os projetos miram `net8.0`. O SDK 10
> compila net8.0 sem problema. Dentro dos containers Docker o SDK 8 usado é o
> final, então o `global.json` não é copiado para a imagem.

## Como rodar localmente

### Opção A — tudo no Docker

```bash
docker compose up --build
```

Sobe Postgres, RabbitMQ (com management UI), Redis, Jaeger (coletor OTLP + UI de
tracing) e os dois serviços. Endereços:

| Recurso | URL |
|---|---|
| **Frontend (React)** | http://localhost:5173 |
| Lançamentos API | http://localhost:5080/swagger |
| Consolidado API | http://localhost:5090/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| Jaeger UI | http://localhost:16686 |
| Health checks | http://localhost:5080/health · http://localhost:5090/health |

### Opção B — infra no Docker, APIs no dotnet

Útil para depurar. Sobe só a infra:

```bash
docker compose up postgres rabbitmq redis jaeger
```

E em dois terminais:

```bash
dotnet run --project src/Lancamentos/Lancamentos.Api
dotnet run --project src/Consolidado/Consolidado.Api
```

As tabelas são criadas no boot de cada serviço (`EnsureCreated`) — não é preciso
rodar migrations à mão.

### Frontend (React)

Uma SPA em React + Vite consome as duas APIs: tela de login, registro de
lançamentos, consulta do consolidado do dia e relatório por período. No Compose
ela já sobe em http://localhost:5173. Para rodar em modo de desenvolvimento:

```bash
cd frontend
npm install
npm run dev
```

## Autenticação e papéis

Os endpoints exigem JWT **com papel**. A autorização espelha o CQRS:

| Papel | Pode |
|---|---|
| `Operador` | registrar lançamentos |
| `Gerente` | consultar o consolidado e o relatório |
| `Admin` | tudo |

Usuários de teste (senha `Senha@123` para os três): `operador`, `gerente`, `admin`.

## Exemplos de chamada

Faça login para obter o token (serve nos dois serviços):

```bash
TOKEN=$(curl -s -X POST http://localhost:5080/token \
  -H "Content-Type: application/json" \
  -d '{"usuario":"admin","senha":"Senha@123"}' | jq -r .token)
```

Registrar um lançamento (crédito):

```bash
curl -X POST http://localhost:5080/lancamentos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tipo":"Credito","valor":1500.00,"data":"2026-06-19","descricao":"venda do dia"}'
```

Registrar um débito:

```bash
curl -X POST http://localhost:5080/lancamentos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"tipo":"Debito","valor":400.00,"data":"2026-06-19","descricao":"fornecedor"}'
```

Consultar o saldo consolidado do dia (o token serve nos dois serviços):

```bash
curl http://localhost:5090/consolidado/2026-06-19 \
  -H "Authorization: Bearer $TOKEN"
```

Resposta esperada (depois que os eventos forem processados):

```json
{
  "data": "2026-06-19",
  "totalCreditos": 1500.00,
  "totalDebitos": 400.00,
  "saldo": 1100.00,
  "atualizadoEmUtc": "2026-06-19T13:40:12Z"
}
```

A consulta é eventualmente consistente: logo após registrar, o saldo pode levar
um instante para refletir, enquanto o evento atravessa a fila.

Relatório do saldo consolidado por período (saldo dia a dia + totais do intervalo):

```bash
curl "http://localhost:5090/consolidado/relatorio?de=2026-06-01&ate=2026-06-30" \
  -H "Authorization: Bearer $TOKEN"
```

```json
{
  "de": "2026-06-01",
  "ate": "2026-06-30",
  "dias": [
    { "data": "2026-06-19", "totalCreditos": 1500.00, "totalDebitos": 400.00, "saldo": 1100.00 }
  ],
  "totalCreditos": 1500.00,
  "totalDebitos": 400.00,
  "saldo": 1100.00
}
```

## Testes

```bash
# tudo
dotnet test

# só os de unidade (não precisam de Docker)
dotnet test tests/Lancamentos.UnitTests
dotnet test tests/Consolidado.UnitTests
```

- **Unidade** (`*.UnitTests`): regras de domínio puras — saldo, validações.
  Sem infraestrutura.
- **Integração** (`*.IntegrationTests`): sobem PostgreSQL, RabbitMQ e Redis
  reais com **Testcontainers** e provam o fluxo ponta a ponta — registra
  lançamento → outbox publica → consumidor processa → saldo projetado. Há um
  teste dedicado provando a **idempotência** (mesmo evento entregue duas vezes
  não dobra o saldo). Se não houver Docker no ambiente, esses testes são
  **skipados** automaticamente, sem falhar o build.

### Mutação, carga e resiliência

Além de unidade e integração, há três camadas que amarram os requisitos
não-funcionais a verificações executáveis. A estratégia completa está em
[`docs/qualidade/estrategia-de-testes.md`](docs/qualidade/estrategia-de-testes.md).

- **Mutação** (Stryker.NET): mede se os testes de unidade de fato detectam
  defeitos no domínio e na aplicação dos dois serviços — hoje em **100% de
  mutation score** nos quatro projetos. Como rodar e como ler o score em
  [`docs/qualidade/mutation-testing.md`](docs/qualidade/mutation-testing.md).

  ```bash
  dotnet tool restore
  cd src/Consolidado/Consolidado.Domain && dotnet stryker
  ```

- **Carga** (NBomber, `tests/Performance/LoadTests`): injeta **50 req/s** no
  `GET /consolidado/{data}` por 60s e **falha se a perda passar de 5%**,
  transformando o RNF num critério objetivo. Precisa da stack no ar e roda fora
  do `dotnet test`:

  ```bash
  docker compose up --build       # em outro terminal
  dotnet run --project tests/Performance/LoadTests -c Release
  ```

- **Resiliência / perda de mensagem** (`tests/Resiliencia.IntegrationTests`):
  prova com infraestrutura real que 500 lançamentos chegam à projeção sem perda,
  que derrubar o consumidor não perde nada (a fila bufferiza o backlog e ele
  drena ao voltar) e que reentrega não dobra o saldo. Skipam sem Docker.

  ```bash
  dotnet test tests/Resiliencia.IntegrationTests
  ```

## Observabilidade e saúde

- **Serilog** loga em formato estruturado no console.
- **OpenTelemetry** exporta traces e métricas via OTLP (no Compose, para o
  Jaeger). Instrumentação de ASP.NET Core, HttpClient, EF Core, Npgsql e
  MassTransit.
- **Health checks** em `/health` cobrindo Postgres, RabbitMQ e Redis.

## Evoluções futuras

Coisas que ficaram conscientemente fora do escopo deste desafio, mas que seriam
os próximos passos num cenário real:

- **API Gateway + Identity Provider de verdade.** Hoje o token é emitido por um
  endpoint simples com chave simétrica, só para exercitar os endpoints
  protegidos. Em produção, OAuth2/OIDC num IdP dedicado e um gateway na frente.
- **Migrations versionadas** no lugar do `EnsureCreated`, com pipeline de
  aplicação controlada.
- **Escala horizontal do consolidado** com várias instâncias consumindo a
  mesma fila (a idempotência já dá suporte a isso) e métricas para autoscaling.
- **Snapshot/reprocessamento** da projeção a partir do histórico de lançamentos,
  caso o read model precise ser reconstruído.
- **Poison message handling** mais rico: alertas sobre a DLQ e ferramenta de
  reprocessamento.
- **Dashboards** de métricas (latência da consulta, lag da fila, taxa de erro)
  e SLOs amarrados ao requisito de 50 req/s com 5% de perda.
- **mTLS** entre serviços e segregação de rede.
