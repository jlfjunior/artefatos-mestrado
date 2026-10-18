# Padrões de código — CashFlow

Convenções adotadas no repositório para manter consistência entre camadas, documentação e integração. Complementa [ADR-012](adr/ADR-012-convencoes-documentacao-idioma.md) (idioma) e os demais ADRs de arquitetura.

## Autor

Samuel Fabel

---

## 1. Idioma

| Artefato                                     | Idioma                                                         |
| -------------------------------------------- | -------------------------------------------------------------- |
| Código (tipos, membros, variáveis, enums)    | Inglês                                                         |
| Rotas HTTP, escopos OAuth, exchanges e filas | Inglês                                                         |
| Comentários XML no código                    | Português (Brasil)                                             |
| ADRs, C4, este documento, `README`           | Português (Brasil)                                             |
| Textos de UI (Blazor)                        | Português (Brasil)                                             |
| Mensagens de erro expostas na API            | Português (Brasil), com **código estável em inglês** (`error`) |

---

## 2. Convenções de nomes

### 2.1 Projetos e namespaces

| Camada             | Projeto                               | Exemplo de namespace                  |
| ------------------ | ------------------------------------- | ------------------------------------- |
| Domínio            | `CashFlow.Domain`                     | `CashFlow.Domain.Entities`            |
| Contratos HTTP     | `CashFlow.Contracts`                  | `CashFlow.Contracts`                  |
| Aplicação          | `CashFlow.Application`                | `CashFlow.Application.Services`       |
| Infraestrutura     | `CashFlow.Infrastructure`             | `CashFlow.Infrastructure.Persistence` |
| API / Web / Worker | `CashFlow.Api`, `.Web`, `.Consumer.*` | `CashFlow.Api.Controllers`            |
| Compartilhado      | `CashFlow.Shared`                     | `CashFlow.Shared.Messaging`           |

- Um **bounded context** por solução; pastas refletem responsabilidade, não tecnologia genérica (`Utils`, `Helpers`).
- Testes espelham a camada: `CashFlow.Domain.Tests`, `CashFlow.Application.Tests`, etc.

### 2.2 Tipos e membros (C#)

| Elemento                    | Convenção                                    | Exemplo                                         |
| --------------------------- | -------------------------------------------- | ----------------------------------------------- |
| Classe / record / interface | PascalCase, substantivo                      | `TransactionService`, `IDailyBalanceRepository` |
| Interface                   | prefixo `I`                                  | `IMessagePublisher`                             |
| Método                      | PascalCase, verbo + objeto                   | `GetByDateAsync`, `ApplyDeltaAsync`             |
| Propriedade                 | PascalCase                                   | `TotalCredits`, `BalanceDate`                   |
| Parâmetro / variável local  | camelCase                                    | `userId`, `cancellationToken`                   |
| Constante                   | PascalCase                                   | `MaxPageSize`, `SectionName`                    |
| Campo privado               | camelCase ou `_camelCase` conforme o arquivo | `_repository`                                   |
| Enum                        | PascalCase singular; membros PascalCase      | `EntryType.Credit`                              |
| Async                       | sufixo `Async`                               | `ListAsync`                                     |
| Migration                   | `Migration_YYYYMMDD_NNN_Description`         | `Migration_20260613_002_AuthAndClientOwnership` |

#### Sufixos de papel (tipo)

Convenção de **nome + sufixo** por responsabilidade. O sufixo identifica o papel do tipo no fluxo (entrada/saída HTTP, persistência, caso de uso, etc.).

| Papel                        | Sufixo / padrão                               | Camada / local                                           | Visibilidade            | Exemplo                                                                  |
| ---------------------------- | --------------------------------------------- | -------------------------------------------------------- | ----------------------- | ------------------------------------------------------------------------ |
| **DTO**                      | `Request`, `Response` ou nome descritivo      | `CashFlow.Contracts/` (`{Dominio}Dtos.cs`)               | `public`                | `CreateTransactionRequest`, `BalanceResponse`, `ErrorResponse`           |
| **Row**                      | `Row`                                         | `Infrastructure/.../Repositories/`                       | `private`               | `TransactionRow`, `DailyBalanceRow`, `TotalsRow`                         |
| **Repository** (porta)       | `Repository`                                  | `Application/Abstractions/`                              | `public` interface `I…` | `ITransactionRepository`                                                 |
| **Repository** (adaptador)   | `Repository`                                  | `Infrastructure/.../Repositories/`                       | `public` classe         | `TransactionRepository`                                                  |
| **Service**                  | `Service`                                     | `Application/Services/`                                  | `public`                | `TransactionService`, `BalanceService`                                   |
| **Controller**               | `Controller`                                  | `CashFlow.Api/Controllers/`                              | `public`                | `TransactionsController`, `BalancesController`                           |
| **Handler**                  | `Handler`                                     | API, auth, HTTP pipeline, mensageria                     | conforme papel          | `ApiExceptionHandler`, `ScopeAuthorizationHandler`, `BearerTokenHandler` |
| **Exception**                | `Exception`                                   | `Shared/Exceptions/`, `Application/Exceptions/`          | `public`                | `CodedException`, `ForbiddenException`, `ValidationException`            |
| **Message**                  | `Message`                                     | `Shared/Messaging/`                                      | `public`                | `TransactionCreatedMessage`, `IIntegrationMessage`                       |
| **Event** (constante / tipo) | `Event` em classe estática ou sufixo `.event` | `Shared/Messaging/`                                      | `public`                | `EventExchanges`, `cashflow.transaction.created.event`                   |
| **Consumer**                 | `Consumer`                                    | `CashFlow.Consumer.*`, `Infrastructure/Messaging/`       | `public`                | `ConsolidationConsumer`, `MessageConsumer<,>`                            |
| **Publisher**                | `Publisher`                                   | `Application/Abstractions/`, `Infrastructure/Messaging/` | porta `I…` / impl.      | `IMessagePublisher`, `RabbitMqMessagePublisher`                          |
| **Projector**                | `Projector`                                   | `Application/Services/`                                  | `public`                | `DailyBalanceProjector`                                                  |

**DTO** — contrato HTTP entre API, Application e Web; sem regra de negócio e **sem referência** a Domain/Infrastructure. Projeto dedicado `CashFlow.Contracts` ([ADR-017](adr/ADR-017-camada-contracts.md)). Preferir `record`; agrupar em `{Dominio}Dtos.cs`. Entrada: sufixo `Request`. Saída: sufixo `Response`. Erros: `ErrorResponse`. Payload de mensagem de integração: sufixo `Data` em `Shared/Messaging` (`TransactionCreatedData`) — não confundir com DTOs HTTP.

**Row** — mapeamento interno Dapper ↔ SQL; `private sealed record` no repositório; sufixo `Row`; converter para entidade ou DTO no adaptador; nunca expor fora da Infrastructure.

**Repository** — porta `I{Entidade}Repository` na Application (ISP); implementação `{Entidade}Repository` na Infrastructure, herdando `RepositoryBase` quando usar Dapper; métodos orientados ao domínio (`GetByIdAsync`, `InsertAsync`).

**Service** — um service por agregado/caso de uso em `Application/Services/`; orquestra portas; sem ASP.NET ou Dapper; evitar `*Helper` (e `*Manager` — ver **Manager** abaixo).

**Controller** — `{Recurso}Controller` no plural alinhado à rota; herda `ControllerBase`; injeta um `{Recurso}Service`; actions em inglês (`List`, `GetById`, `Create`).

**Handler** — trata um evento ou concern transversal (exceção HTTP, autorização, `DelegatingHandler`); sufixo `Handler`; uma responsabilidade por tipo; autorização: `{Critério}AuthorizationHandler` + `{Critério}Requirement`.

**Exception** — sufixo `Exception`; `CodedException` (Shared) para código + descrição; `AppException` (Application) como base de negócio; concretos por semântica (`ForbiddenException`, `ValidationException`); código API em `Error`, mensagem em `Description` / `Message`.

**Message** — envelope **CloudEvents** publicado/consumido no broker; sufixo **`Message`**; implementa `IIntegrationMessage`; herda `CloudEvent<TData>` quando aplicável. Payload de negócio no tipo irmão **`{Agregado}{Ação}Data`**. Agrupar em `{Dominio}Messages.cs`. Factory estático `Create(...)` na mensagem; `id` gerado internamente.

**Event** — nome do **fato de domínio** no broker e na constante C# (padrão EIP *Event Message*). Classe estática **`EventExchanges`** com membros `{Agregado}{Ação}`; valor string **`cashflow.{agregado}.{ação}.event`** (lowercase, pontos). Campo de rastreio em projeção: `LastEventId`. Não confundir: **Event** = identificador/tipo do fato; **Message** = tipo C# do envelope serializado.

**Consumer** — worker que consome fila e delega à Application; sufixo **`Consumer`**; projeto `CashFlow.Consumer.{Contexto}`. Implementação concreta herda **`MessageConsumer<TService, TMessage>`** (`BackgroundService`); uma fila/contexto por consumer (`ConsolidationConsumer`). Registrado como `AddHostedService<…>`.

**Publisher** — porta **`IMessagePublisher`** na Application; implementação **`{Broker}MessagePublisher`** na Infrastructure; publica `IIntegrationMessage` na exchange do evento.

**Projector** — serviço de **projeção de leitura** (materializa/consolida dados); sufixo **`Projector`** em `Application/Services/`; invocado pelo consumer, sem HTTP (`DailyBalanceProjector`).

#### Proibido / evitar

- Abreviações obscuras (`txn`, `bal`) — preferir nomes completos em inglês.
- Tipos genéricos sem significado (`Info`, `Util`) quando houver termo de domínio.
- `*Manager` para orquestração — usar `Service`, `Projector` ou `HostedService` (ver tabela acima).
- `var` quando o tipo não for óbvio na linha.

### 2.3 Arquivos

- Um tipo público principal por arquivo; nome do arquivo = nome do tipo.
- **DTO:** agrupar em `src/CashFlow.Contracts/{Dominio}Dtos.cs` (`BalanceDtos.cs`, `TransactionDtos.cs`).
- **Repository:** `{Entidade}Repository.cs` em `Infrastructure/Persistence/Repositories/`.
- **Service:** `{Entidade}Service.cs` em `Application/Services/`.
- **Controller:** `{Recurso}Controller.cs` em `CashFlow.Api/Controllers/`.
- **Handler:** `{Concern}Handler.cs` na pasta do concern (`Exceptions/`, `Authorization/`, `Services/`).
- **Message:** `{Dominio}Messages.cs` em `Shared/Messaging/`.
- **Consumer:** `{Contexto}Consumer.cs` em `CashFlow.Consumer.{Contexto}/`.
- **Publisher / Projector:** `{Nome}Publisher.cs`, `{Nome}Projector.cs` nas pastas de Messaging e Services.

---

## 3. Clean code e arquitetura

Princípios alinhados a [ADR-003](adr/ADR-003-arquitetura-ddd-camadas.md) e [ADR-013](adr/ADR-013-solid-eip.md).

### 3.1 Responsabilidades por camada

| Camada             | Pode                                                          | Não pode                                        |
| ------------------ | ------------------------------------------------------------- | ----------------------------------------------- |
| **Domain**         | Regras de negócio, invariantes, cálculos puros                | Referenciar EF, Dapper, HTTP, RabbitMQ          |
| **Contracts**      | DTOs HTTP (`record` anêmicos)                                 | Regra de negócio, referências a outros projetos |
| **Application**    | Casos de uso, orquestração, mapeamento entidade ↔ DTO, portas | SQL direto, detalhes de broker                  |
| **Infrastructure** | Implementar portas, migrations, OAuth, messaging              | Regras de negócio complexas                     |
| **Api / Web**      | HTTP, auth, serialização, UI                                  | Lógica de domínio                               |

### 3.2 Regras práticas

1. **Controllers finos** — delegar a services da Application; sem `try/catch` de negócio (usar `ApiExceptionHandler`).
2. **Portas pequenas (ISP)** — interfaces focadas (`ITransactionRepository`, não `IRepository` genérico).
3. **Imutabilidade onde couber** — agregado `Transaction` sem setters públicos; lançamentos não são alterados após criação ([ADR-015](adr/ADR-015-lancamentos-imutaveis.md)).
4. **Fail fast** — validar invariantes no domínio; códigos de erro estáveis na API (`invalid_amount`, `access_denied`).
5. **Sem over-engineering** — preferir diff mínimo; não introduzir abstração para um único uso.
6. **Dependência para dentro** — Application depende de abstrações e de `Contracts`; Infrastructure implementa portas.

### 3.3 Injeção de dependência

- Services de aplicação: **scoped**.
- Repositórios e `IUnitOfWork`: **scoped** (mesma conexão por requisição).
- Publicadores/consumidores de mensagem: conforme ciclo de vida do host (singleton/scoped documentado no `DependencyInjection`).

---

## 4. Documentação (XML)

Comentários XML em **português (Brasil)** em todos os membros **públicos** de `src/`.

### 4.1 Padrões por construção

#### Classe estática

```csharp
/// <summary>
/// Atributos obrigatórios da especificação CloudEvents 1.0.
/// </summary>
public static class CloudEventAttributes
```

#### Tipo genérico

```csharp
/// <summary>
/// Envelope CloudEvents 1.0 com payload tipado em <c>data</c>.
/// </summary>
/// <typeparam name="TData">Conteúdo de negócio do evento.</typeparam>
public record CloudEvent<TData> : IIntegrationMessage
```

#### Constante

```csharp
/// <summary>Versão da especificação CloudEvents.</summary>
public const string SpecVersion = "1.0";
```

#### Propriedade

```csharp
/// <summary>Identificador único do evento (<c>id</c>).</summary>
string Id { get; }
```

#### Record posicional (DTO)

```csharp
/// <summary>
/// Saldo consolidado de um dia contábil.
/// </summary>
/// <param name="UserId">Usuário cliente titular do consolidado.</param>
/// <param name="Date">Data contábil do consolidado.</param>
public sealed record BalanceResponse(Guid UserId, DateOnly Date, ...);
```

#### Classe com construtor primário

```csharp
/// <summary>
/// Casos de uso de consulta de saldos (somente leitura).
/// </summary>
/// <param name="repository">Repositório de saldos diários.</param>
/// <param name="currentUser">Contexto de usuário atual.</param>
public sealed class BalanceService(IDailyBalanceRepository repository, ICurrentUserContext currentUser)
```

#### Método

```csharp
/// <summary>
/// Lista saldos diários paginados.
/// </summary>
/// <param name="userId">ID do usuário.</param>
/// <param name="cancellationToken">Token de cancelamento.</param>
/// <returns>Resposta paginada de saldos diários.</returns>
public async Task<PaginatedBalanceResponse> ListAsync(...)
```

### 4.2 OpenAPI

- Controllers com `[ProducesResponseType]` para sucesso e erros (`ErrorResponse`).
- DTOs com `[Tags]` nos controllers e summaries que descrevem o caso de uso.

---

## 5. Banco de dados (PostgreSQL)

Convenções aplicadas nas migrations FluentMigrator ([ADR-006](adr/ADR-006-migrations-fluentmigrator.md)).

### 5.1 Tabelas e colunas

| Regra          | Convenção                                    | Exemplo                                   |
| -------------- | -------------------------------------------- | ----------------------------------------- |
| Nome da tabela | `snake_case`, plural ou substantivo coletivo | `transactions`, `daily_balances`, `users` |
| Coluna         | `snake_case`                                 | `user_id`, `created_at`, `entry_type`     |
| PK simples     | coluna `id UUID`                             | `transactions.id`                         |
| PK composta    | constraint nomeada                           | `pk_daily_balances_user_date`             |
| FK             | `{entidade}_id`                              | `user_id`, `created_by`                   |
| Timestamp      | `created_at`, `updated_at`                   | `TIMESTAMPTZ`                             |
| Boolean        | adjetivo                                     | `enabled`                                 |
| Hash / segredo | sufixo `_hash`                               | `password_hash`, `secret_hash`            |

### 5.2 Índices

| Tipo            | Padrão de nome             | Exemplo                                                      |
| --------------- | -------------------------- | ------------------------------------------------------------ |
| Índice simples  | `idx_{tabela}_{coluna(s)}` | `idx_transactions_transaction_date`                          |
| Índice composto | colunas em ordem de filtro | `idx_transactions_user_date` (`user_id`, `transaction_date`) |

### 5.3 Migrations

- Arquivo: `Migration_YYYYMMDD_NNN_Description.cs`
- Atributo: `[Migration(YYYYMMDDNNN)]` (número sequencial no dia)
- **Nunca** editar migration já aplicada em ambiente compartilhado — criar nova migration.
- Seed de dados demo apenas em migrations explícitas (ex.: usuários iniciais).

### 5.4 SQL nos repositórios

- Colunas sempre entre aspas duplas quando necessário; parâmetros Dapper `@PascalCase` mapeados de propriedades anônimas.
- Leitura de projeções (`daily_balances`) separada da escrita transacional (`transactions` + publish).

---

## 6. Endpoints HTTP

Rotas em **inglês**, **lowercase**, substantivos no plural para recursos de negócio. Convenções de **REST** para `/api/*`; **OAuth 2.0** e **OpenID Connect** para autenticação e descoberta (ver [6.5](#65-referências-normativas-http-e-oauth)).

### 6.1 Estrutura geral

| Prefixo                | Uso                    |
| ---------------------- | ---------------------- |
| `/api/{recurso}`       | API REST de negócio    |
| `/oauth`               | Token OAuth2           |
| `/.well-known`         | Descoberta OIDC / JWKS |
| `/health`, `/liveness` | Health checks          |

### 6.2 Nomenclatura de rotas

| Padrão                            | Verbo  | Uso                                                           |
| --------------------------------- | ------ | ------------------------------------------------------------- |
| `/api/{recursos}`                 | `GET`  | Listar coleção (filtros via query string)                     |
| `/api/{recursos}/{id}`            | `GET`  | Obter item por identificador                                  |
| `/api/{recursos}`                 | `POST` | Criar item na coleção                                         |
| `/api/{recursos}/me`              | `GET`  | Recurso vinculado ao usuário autenticado                      |
| `/api/{recursos}/{segmento-fixo}` | `GET`  | Sub-recurso com significado estável (`today`, etc.)           |
| `/api/{recursos}/{param}`         | `GET`  | Variante parametrizada com constraint de rota (`{date:date}`) |
| `/oauth/token`                    | `POST` | Emissão de token (form-urlencoded)                            |
| `/.well-known/{documento}`        | `GET`  | Descoberta OIDC / JWKS                                        |

- `{recursos}` no **plural**, substantivo em inglês: `transactions`, `balances`, `clients`.
- Identificadores na rota: preferir `{id:guid}` quando for UUID.
- Rotas de negócio sob `/api/`; autenticação e well-known fora desse prefixo.
- Inventário completo e contratos: OpenAPI (`/openapi/v1.json`) e [C4](c4-definicoes-fluxo-caixa.md).

### 6.3 Regras

- **Substantivos**, não verbos na URL (`/transactions`, não `/createTransaction`).
- Query string para filtros e paginação: `?userId=&page=&pageSize=&from=&to=`.
- Datas de rota: constraint `{date:date}` (`yyyy-MM-dd`).
- Resposta de erro OAuth: `{ "error": "codigo_estavel", "error_description": "Mensagem legível" }` ([RFC 6749 §5.2](https://datatracker.ietf.org/doc/html/rfc6749#section-5.2)).
- Autenticação: `Authorization: Bearer {jwt}` ([RFC 6750](https://datatracker.ietf.org/doc/html/rfc6750)); escopos validados por policy.

### 6.4 Escopos OAuth

Padrão de nome: `{recurso}.{ação}` em inglês, lowercase, segmentos separados por ponto.

O parâmetro `scope` é definido pelo [RFC 6749 §3.3](https://datatracker.ietf.org/doc/html/rfc6749#section-3.3) como string opaca — **não há formato obrigatório** na especificação. Neste projeto, adotamos a convenção abaixo (detalhada em [ADR-016](adr/ADR-016-oauth2-jwt-autenticacao.md)) e expomos valores em `scopes_supported` no documento de descoberta OIDC ([OpenID Connect Discovery 1.0](https://openid.net/specs/openid-connect-discovery-1_0.html)).

| Variante                | Significado                                 | Exemplo                 |
| ----------------------- | ------------------------------------------- | ----------------------- |
| `{recurso}.{ação}`      | Permissão específica                        | `transactions.write`    |
| `{recurso}.{ação}.all`  | Ação sobre todos os titulares (funcionário) | `transactions.read.all` |
| `{recurso}.{ação}.self` | Ação restrita ao próprio usuário (cliente)  | `balances.read.self`    |

- Escopos catalogados em `AuthorizationScopes`; políticas ASP.NET mapeadas em `AuthorizationPolicies`.
- Novo endpoint protegido: criar escopo, policy e seed na migration quando aplicável.

### 6.5 Referências normativas (HTTP e OAuth)

| Padrão adotado                                                                   | Origem                          | Especificação                                                                                                                                                                                                                       |
| -------------------------------------------------------------------------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Recursos REST (`GET/POST /api/{recursos}`, substantivos no plural, IDs na rota)  | Convenção REST / HTTP           | [RFC 9110](https://datatracker.ietf.org/doc/html/rfc9110) (semântica HTTP); [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines/blob/vNext/azure/Guidelines.md) (recursos como substantivos, URIs estáveis) |
| Prefixo `/api/` para negócio                                                     | Convenção do projeto            | Separar API de domínio de endpoints de protocolo (`/oauth`, `/.well-known`) — ver [ADR-016](adr/ADR-016-oauth2-jwt-autenticacao.md)                                                                                                 |
| `POST /oauth/token`                                                              | OAuth 2.0                       | [RFC 6749 §3.2](https://datatracker.ietf.org/doc/html/rfc6749#section-3.2) — Token Endpoint                                                                                                                                         |
| Body `application/x-www-form-urlencoded`, `grant_type`, `client_id` / Basic auth | OAuth 2.0                       | [RFC 6749 §4](https://datatracker.ietf.org/doc/html/rfc6749#section-4) (grant types); [RFC 6749 §2.3.1](https://datatracker.ietf.org/doc/html/rfc6749#section-2.3.1) (client authentication)                                        |
| Resposta de erro `error` + `error_description`                                   | OAuth 2.0                       | [RFC 6749 §5.2](https://datatracker.ietf.org/doc/html/rfc6749#section-5.2)                                                                                                                                                          |
| `Authorization: Bearer {token}`                                                  | OAuth 2.0                       | [RFC 6750](https://datatracker.ietf.org/doc/html/rfc6750) — Bearer Token Usage                                                                                                                                                      |
| Access token JWT, claim `scope`                                                  | OAuth 2.0 + JWT                 | [RFC 6749 §3.3](https://datatracker.ietf.org/doc/html/rfc6749#section-3.3); [RFC 7519](https://datatracker.ietf.org/doc/html/rfc7519)                                                                                               |
| `GET /.well-known/openid-configuration`                                          | OpenID Connect                  | [OpenID Connect Discovery 1.0](https://openid.net/specs/openid-connect-discovery-1_0.html)                                                                                                                                          |
| `GET /.well-known/jwks.json` (`jwks_uri`)                                        | JWK / OIDC                      | [RFC 7517](https://datatracker.ietf.org/doc/html/rfc7517); [OpenID Connect Discovery 1.0](https://openid.net/specs/openid-connect-discovery-1_0.html)                                                                               |
| Caminho `/.well-known/`                                                          | Well-Known URIs                 | [RFC 8615](https://datatracker.ietf.org/doc/html/rfc8615)                                                                                                                                                                           |
| Formato `{recurso}.{ação}` e variantes `.all` / `.self`                          | **Convenção deste repositório** | Inspirado em escopos opacos do [RFC 6749 §3.3](https://datatracker.ietf.org/doc/html/rfc6749#section-3.3); sufixos `.all`/`.self` não são norma IETF — ver [ADR-016](adr/ADR-016-oauth2-jwt-autenticacao.md)                        |

---

## 7. Eventos e mensageria

Padrão **Event Message** ([ADR-004](adr/ADR-004-integracao-assincrona-rabbitmq.md)): uma exchange fanout por tipo de evento. Convenções de tipos **`Message`**, **`Event`**, **`Consumer`** e **`Publisher`**: ver [§2.2 Sufixos de papel](#sufixos-de-papel-tipo).

### 7.1 Nomenclatura

| Elemento                     | Padrão                             | Exemplo                              |
| ---------------------------- | ---------------------------------- | ------------------------------------ |
| Exchange / `type` CloudEvent | `cashflow.{agregado}.{ação}.event` | `cashflow.transaction.created.event` |
| Fila de consumer             | `cashflow.{contexto}`              | `cashflow.consolidation`             |
| Dead-letter                  | `{fila}.dlx` / `{fila}.dlq`        | `cashflow.consolidation.dlq`         |
| Classe de payload            | `{Agregado}{Ação}Data`             | `TransactionCreatedData`             |
| Classe de mensagem           | `{Agregado}{Ação}Message`          | `TransactionCreatedMessage`          |
| Constante C#                 | PascalCase descritivo              | `EventExchanges.TransactionCreated`  |
| `source` CloudEvent          | URN                                | `urn:cashflow:transactions-api`      |

### 7.2 Formato da mensagem

- Envelope **CloudEvents 1.0** (`specversion`, `id`, `source`, `type`, `time`, `data`).
- `type` da mensagem = nome da exchange.
- `id` gerado no factory (`Guid.NewGuid()`), não passado pelo chamador.
- Body JSON; mensagens persistentes no RabbitMQ.
- Publicação após persistência na mesma unidade de trabalho ([ADR-010](adr/ADR-010-publicacao-mensagens-alteracoes-banco.md)).

### 7.3 Adicionar novo evento (checklist)

1. Constante em `EventExchanges` + inclusão em `All`.
2. Record `{Entity}{Action}Data` e `{Entity}{Action}Message` em `CashFlow.Shared/Messaging`.
3. Factory estático `Create(...)` na mensagem.
4. Publicar via `IMessagePublisher` após alteração no banco.
5. Consumer dedicado ou extensão de fila existente com handler tipado.
6. Documentar no C4, ADR se for decisão relevante, e tabela de eventos no `README`.

---

## 8. Testes

- Framework: **xUnit** + FluentAssertions + Moq ([ADR-011](adr/ADR-011-testes-xunit-tdd.md)).
- Nome do teste: `{Método}_Should{Comportamento}_When{Condição}` ou variante descritiva.
- Testes de domínio e aplicação **sem** infraestrutura real; mocks das portas.
- Preferir TDD em regras de negócio e casos de uso críticos.

---

## 9. Referências cruzadas

| Tema            | Documento                                                                                                                                                                                                |
| --------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Idioma          | [ADR-012](adr/ADR-012-convencoes-documentacao-idioma.md)                                                                                                                                                 |
| Camadas DDD     | [ADR-003](adr/ADR-003-arquitetura-ddd-camadas.md)                                                                                                                                                        |
| SOLID / EIP     | [ADR-013](adr/ADR-013-solid-eip.md)                                                                                                                                                                      |
| Migrations      | [ADR-006](adr/ADR-006-migrations-fluentmigrator.md)                                                                                                                                                      |
| Mensagens       | [ADR-004](adr/ADR-004-integracao-assincrona-rabbitmq.md), [ADR-010](adr/ADR-010-publicacao-mensagens-alteracoes-banco.md)                                                                                |
| Modelo de dados | [c4-definicoes-fluxo-caixa.md](c4-definicoes-fluxo-caixa.md)                                                                                                                                             |
| OAuth / JWT     | [ADR-016](adr/ADR-016-oauth2-jwt-autenticacao.md), [RFC 6749](https://datatracker.ietf.org/doc/html/rfc6749), [OpenID Connect Discovery 1.0](https://openid.net/specs/openid-connect-discovery-1_0.html) |
| Rotas / OAuth   | [§6.5 deste documento](#65-referências-normativas-http-e-oauth)                                                                                                                                          |

---

*Documento vivo — atualizar quando novas convenções forem adotadas via ADR ou revisão de código.*
