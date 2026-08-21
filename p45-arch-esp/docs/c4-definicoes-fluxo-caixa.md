# Definições de Arquitetura C4 – Sistema de Fluxo de Caixa

## Autor

Samuel Fabel

## Contexto de Negócio

O **Sistema de Fluxo de Caixa** atende comerciantes que precisam controlar o fluxo de caixa diário com lançamentos de débitos e créditos, além de consultar um relatório com o saldo diário consolidado.

Características principais:

- **Controle de lançamentos** — registro e consulta de movimentações financeiras (débito e crédito); lançamentos imutáveis após criação
- **Consolidado diário** — visão agregada do saldo por dia, derivada dos lançamentos
- **Alta disponibilidade do serviço de lançamentos** — o controle de lançamentos não pode ficar indisponível se o consolidado diário cair
- **Desempenho do consolidado** — em picos, até 50 requisições por segundo com no máximo 5% de perda
- **Stack** — C# (.NET 10), PostgreSQL, RabbitMQ, Blazor, Dapper, FluentMigrator, Docker Compose

**Objetivo:** Entregar uma arquitetura escalável e resiliente que separe responsabilidades entre escrita de lançamentos e materialização/consulta do consolidado, usando integração assíncrona por mensagens.

**Diagramas visuais:** [Contexto](diagrams/c4-context.md) · [Containers](diagrams/c4-containers.md)

---

## Nível 1: Context Diagram - Definições

### Atores e Sistemas Externos

#### Comerciante

- **Tipo:** Pessoa Física / Jurídica
- **Descrição:** Usuário que registra lançamentos e consulta saldos consolidados
- **Interação:** Acessa a interface Blazor para cadastrar débitos/créditos e visualizar relatório diário

#### Sistema de Fluxo de Caixa

- **Tipo:** Sistema de Software
- **Descrição:** Plataforma composta por serviços de lançamentos, consolidado e interface web
- **Responsabilidades:**
  - Persistir lançamentos com integridade transacional
  - Publicar eventos de alteração para processamento assíncrono
  - Materializar e expor saldos diários consolidados
  - Garantir que falhas no consolidado não bloqueiem novos lançamentos

---

## Nível 2: Container Diagram - Definições

### Containers do Sistema

#### Web App (Blazor)

- **Tecnologia:** ASP.NET Core Blazor Server
- **Responsabilidade:**
  - Interface do comerciante para criar e listar lançamentos
  - Consulta de saldo consolidado por data
  - Comunicação HTTP com APIs de backend
- **Comunicação:**
  - CashFlow API (HTTP único)

#### CashFlow API

- **Tecnologia:** ASP.NET Core Web API (.NET 10)
- **Responsabilidade:**
  - Endpoints REST para lançamentos (`/api/transactions`) e saldos (`/api/balances`)
  - OAuth2 (`/oauth/token`) e descoberta OIDC (`/.well-known/*`)
  - Autorização por escopos JWT (funcionário vs cliente)
  - Persistência transacional e publicação de mensagens tipadas após alterações no banco
- **Comunicação:**
  - PostgreSQL (`transactions`, leitura de `daily_balances`)
  - RabbitMQ (Event Message: exchange `cashflow.transaction.created.event`)

#### Consumer.Consolidation (`CashFlow.Consumer.Consolidation`)

- **Tecnologia:** .NET Worker Service (consumer de fila)
- **Responsabilidade:**
  - Consumir `TransactionCreatedMessage` da fila `cashflow.consolidation`
  - Desserializar `TransactionCreatedMessage` (tipo fixo deste consumer; exchange identifica o evento)
  - Atualizar projeção `daily_balances`
- **Comunicação:**
  - RabbitMQ (fila `cashflow.consolidation`)
  - PostgreSQL (`daily_balances`)

#### PostgreSQL

- **Tecnologia:** PostgreSQL 16
- **Responsabilidade:**
  - Armazenar lançamentos (`transactions`)
  - Armazenar projeção consolidada (`daily_balances`)
  - Garantir ACID nas operações de escrita

#### RabbitMQ

- **Tecnologia:** RabbitMQ 3
- **Responsabilidade:**
  - Desacoplar escrita de lançamentos da materialização do consolidado
  - Buffer de mensagens quando o consumer estiver indisponível
  - Suportar padrões Enterprise Integration Patterns (Message Channel, Publish-Subscribe)

---

## Nível 3: Component Diagram - Definições

### 3.1 CashFlow API - Componentes

#### Transaction Controller

- **Responsabilidade:** Traduzir DTOs para comandos da camada de aplicação

#### Transaction Application Service

- **Responsabilidade:**
  - Casos de uso: criar e listar lançamentos (imutáveis após criação)
  - Coordenar transação de banco + publicação de mensagem na mesma unidade de trabalho (Outbox dedicado como evolução — ver [ADR-010](adr/ADR-010-publicacao-mensagens-alteracoes-banco.md))
  - Validar regras de negócio via agregados de domínio

#### Transaction Repository (Dapper)

- **Responsabilidade:** Persistência SQL de lançamentos; implementa portas definidas na aplicação

#### Message Publisher (RabbitMQ)

- **Responsabilidade:** Publicar Event Message na exchange do evento (`cashflow.transaction.created.event`)

#### Balances Controller / Balance Query Service

- **Responsabilidade:** `GET /api/balances`, `/api/balances/today`, `/api/balances/{date}`

### 3.2 Consumer.Consolidation - Componentes

#### Message Handler

- **Responsabilidade:** Desserializar `TransactionCreatedMessage`; delegar a `DailyBalanceProjector.ProjectAsync`

### 3.3 Web App (Blazor) - Componentes

#### Transaction Pages

- **Responsabilidade:** Formulários e listagem de lançamentos

#### Daily Balance Report Page

- **Responsabilidade:** Exibir saldo consolidado por dia

#### Api Client

- **Responsabilidade:** Cliente HTTP tipado para CashFlow API (URL única)

---

## Modelo de Dados

### Entidades (PostgreSQL)

Schema final após `Migration_20260613_001_InitialSchema` e `Migration_20260613_002_AuthAndClientOwnership`.

#### TRANSACTIONS

- **Propósito:** persistir lançamentos imutáveis de caixa (débito/crédito).
- **Titular (`user_id`):** usuário cliente dono do lançamento; consolidado e consultas filtram por este vínculo.
- **Autor (`created_by`):** usuário que registrou o lançamento (tipicamente funcionário).
- **Índices:** por data contábil e por cliente+data, para listagens e relatórios.

```sql
CREATE TABLE transactions (
    id UUID PRIMARY KEY,
    description VARCHAR(500) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    entry_type VARCHAR(10) NOT NULL,
    transaction_date DATE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    user_id UUID NOT NULL REFERENCES users (id),
    created_by UUID NOT NULL REFERENCES users (id)
);

CREATE INDEX idx_transactions_transaction_date ON transactions (transaction_date);
CREATE INDEX idx_transactions_user_date ON transactions (user_id, transaction_date);
```

#### DAILY_BALANCES

- **Propósito:** projeção de leitura do saldo diário consolidado por cliente (consistência eventual).
- **Granularidade:** uma linha por par `(user_id, balance_date)`.
- **Campos agregados:**
  - `total_credits` / `total_debits` — soma dos lançamentos **apenas do dia**;
  - `balance` — saldo **acumulado ao fim do dia** (saldo do último dia anterior + créditos do dia − débitos do dia), calculado por `DailyBalanceCalculator.ComputeCumulativeBalance`.
- **Dias sem movimentação:** não há linha em `daily_balances`. Na consulta (`GET /api/balances/{date}` ou `/today`), a API devolve `totalCredits` e `totalDebits` zerados e repete o `balance` do **último dia anterior com registro** (carry-forward). Sem histórico, o saldo retornado é zero.
- **`last_event_id`:** identificador do último CloudEvent aplicado (base para idempotência futura).

```sql
CREATE TABLE daily_balances (
    user_id UUID NOT NULL REFERENCES users (id),
    balance_date DATE NOT NULL,
    total_credits NUMERIC(18, 2) NOT NULL DEFAULT 0,
    total_debits NUMERIC(18, 2) NOT NULL DEFAULT 0,
    balance NUMERIC(18, 2) NOT NULL DEFAULT 0,
    last_event_id UUID,
    updated_at TIMESTAMPTZ NOT NULL,
    PRIMARY KEY (user_id, balance_date)
);
```

#### AUTH (OAuth2 / usuários)

Persistência de identidade, autorização e emissão/validação de JWT. Separa **quem autentica** (usuário + client OAuth), **o que pode fazer** (escopos) e **como assinar tokens** (chaves RSA).

##### USERS

- **Propósito:** cadastro de usuários autenticáveis da plataforma.
- **Papéis (`role`):** `Employee` (funcionário) e `Client` (cliente/comerciante).
- **Uso no domínio:** `transactions.user_id` aponta para o cliente titular; `transactions.created_by` para quem lançou.

##### AUTHORIZATION_SCOPES / USER_AUTHORIZATION_SCOPES

- **Propósito:** catálogo de escopos OAuth2 e vínculo N:N usuário ↔ escopo.
- **Exemplos de escopo:** `transactions.write`, `balances.read.self`, `users.read`.
- **Uso na API:** escopos viram claims no JWT e alimentam políticas ASP.NET Core (`AuthorizationPolicies`).

##### OAUTH_CLIENTS / OAUTH_CLIENT_SECRETS

- **Propósito:** clients confidenciais autorizados a obter tokens (ex.: Blazor `cashflow.web`).
- **`grant_types`:** grant types permitidos (atualmente `password` para login da interface).
- **Secret:** armazenado apenas como hash; validado no `POST /oauth/token`.

##### SIGNING_KEYS

- **Propósito:** pool de chaves RSA para assinar access tokens e expor JWKS em `/.well-known/jwks.json`.
- **`key_id` (kid):** identificador público usado no header JWT e na resolução de chave na validação.
- **Chave privada:** persistida cifrada; chave mestra derivada de `OAuth:SigningKeysSecret`.

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY,
    full_name VARCHAR(200) NOT NULL,
    email VARCHAR(320) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    role VARCHAR(20) NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE authorization_scopes (
    code VARCHAR(100) PRIMARY KEY,
    description VARCHAR(500) NOT NULL
);

CREATE TABLE user_authorization_scopes (
    user_id UUID NOT NULL REFERENCES users (id),
    scope_code VARCHAR(100) NOT NULL REFERENCES authorization_scopes (code),
    PRIMARY KEY (user_id, scope_code)
);

CREATE TABLE oauth_clients (
    id UUID PRIMARY KEY,
    client_id VARCHAR(100) NOT NULL UNIQUE,
    client_type VARCHAR(20) NOT NULL,
    grant_types VARCHAR(500) NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE oauth_client_secrets (
    oauth_client_id UUID PRIMARY KEY REFERENCES oauth_clients (id),
    secret_hash VARCHAR(500) NOT NULL
);

CREATE TABLE signing_keys (
    id UUID PRIMARY KEY,
    key_id VARCHAR(50) NOT NULL UNIQUE,
    algorithm VARCHAR(10) NOT NULL,
    key_use VARCHAR(10) NOT NULL,
    public_modulus_n VARCHAR(2048) NOT NULL,
    public_exponent_e VARCHAR(32) NOT NULL,
    encrypted_private_key TEXT NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL
);
```

---

## Fluxos Principais

### 5.1 Criação de lançamento

1. Comerciante envia POST `/api/transactions` via Blazor (Bearer JWT)
2. Transactions API valida e persiste em PostgreSQL (transação ACID)
3. Publica Event Message em `cashflow.transaction.created.event` antes do commit; rollback se a publicação falhar
4. Retorna 201 ao cliente — **sem aguardar** o consolidado
5. Consolidation Worker consome mensagem e atualiza `daily_balances`

### 5.2 Consulta de saldo

1. Comerciante consulta via Blazor:
   - `GET /api/balances` — listagem paginada
   - `GET /api/balances/today` — saldo de hoje
   - `GET /api/balances/{date}` — saldo de um dia (`DateOnly`)
2. API lê projeção em `daily_balances` para `(userId, data)`:
   - **Com linha:** retorna créditos, débitos e saldo acumulado do dia.
   - **Sem linha, com histórico anterior:** carry-forward — créditos e débitos do dia = 0; `balance` = saldo acumulado do último dia com movimentação anterior à data consultada (`BalanceService.GetByDateAsync` + `GetLastBeforeDateAsync`).
   - **Sem histórico:** saldo zerado.

### 5.3 Falha do Consolidation Worker

1. Lançamentos continuam sendo aceitos pela Transactions API
2. Mensagens acumulam na fila RabbitMQ
3. Ao recuperar o worker, eventos são reprocessados — **idempotência completa ainda pendente** (ver [ADR-013](adr/ADR-013-solid-eip.md))

---

## Requisitos Não Funcionais

| Requisito                                   | Estratégia implementada / planejada                                                                                                                                |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Lançamentos disponíveis se consolidado cair | Desacoplamento assíncrono via RabbitMQ; consumer separado                                                                                                          |
| 50 req/s no consolidado, ≤5% perda          | Índices em `daily_balances`; leitura direta na projeção; teste k6 misto (escrita + leitura) em [`load-test/`](load-test/README.md); cache e réplicas como evolução |
| Escalabilidade                              | Scale-out de API e Worker; filas duráveis                                                                                                                          |
| Resiliência                                 | DLQ, health checks na API, mensagens persistentes                                                                                                                  |
| Segurança                                   | OAuth2 password grant, JWT com escopos, validação de entrada ([ADR-016](adr/ADR-016-oauth2-jwt-autenticacao.md))                                                   |

---

## Princípios da Arquitetura

- **DDD:** Domínio rico em `Domain`; casos de uso em `Application`; contratos HTTP em `Contracts`; detalhes em `Infrastructure`
- **SOLID:** Portas e adaptadores; dependências apontando para o domínio
- **Enterprise Integration Patterns:** Event Message, Message Channel, Event-Driven Consumer, Dead Letter Channel; Idempotent Receiver **parcial** (`last_event_id` persistido, deduplicação pendente)
- **Projeção assíncrona:** escrita em `transactions`; consolidado materializado em `daily_balances` pelo consumer (consistência eventual)
- **Código em inglês; documentação em português (Brasil)**

---

## Evoluções Futuras

- Outbox pattern dedicado com polling para garantia atômica DB ↔ broker
- Idempotência completa no consumer (deduplicação por `event_id`)
- Observabilidade (OpenTelemetry, métricas Prometheus)
- Cache de leitura e read replicas PostgreSQL para picos de consulta
- API Gateway, rate limiting e HTTPS em produção
- Multi-tenant por comerciante (organização/tenant explícito além de clientes)

---

## Referências

- **C4 Model:** <https://c4model.com/>
- **Enterprise Integration Patterns:** Hohpe & Woolf
- **Domain-Driven Design:** Eric Evans
