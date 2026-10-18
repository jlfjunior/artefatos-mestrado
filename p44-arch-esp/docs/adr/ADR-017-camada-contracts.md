# ADR-017 – Camada de contratos (`CashFlow.Contracts`)

Data: 2026-06-16  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Os DTOs de entrada e saída da API HTTP estavam em `CashFlow.Application/Dtos/`. O frontend Blazor (`CashFlow.Web`) precisava desses tipos para deserializar respostas e montar requisições, o que obrigava uma referência de projeto à camada de **Application** — acoplando a UI a casos de uso, portas e dependências transitivas indevidas.

Requisitos:

- Manter **controllers finos** e services na Application sem vazar implementação para a Web.
- Compartilhar contratos estáveis entre **API**, **Application** e **Web** (OpenAPI ↔ cliente HTTP).
- Projeto de contratos **sem dependências** de domínio, infraestrutura ou ASP.NET.

---

## Decisão

Criar o projeto **`CashFlow.Contracts`** (`src/CashFlow.Contracts/`):

| Aspecto          | Regra                                                                                                                                              |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Conteúdo**     | `record` de request/response HTTP (`*Request`, `*Response`, `ErrorResponse`, DTOs OAuth/OIDC expostos na API)                                      |
| **Namespace**    | `CashFlow.Contracts`                                                                                                                               |
| **Dependências** | Nenhuma referência a outros projetos da solução                                                                                                    |
| **Organização**  | Um arquivo `{Dominio}Dtos.cs` por agregado (`TransactionDtos.cs`, `BalanceDtos.cs`, etc.)                                                          |
| **JSON HTTP**    | `snake_case` via `ContractsJsonSerializerOptions` (`JsonNamingPolicy.SnakeCaseLower`); exceções: `error_description`, campos JWK (`kty`, `kid`, …) |

**Referências de projeto:**

| Projeto                                           | Referencia `CashFlow.Contracts`? | Observação                                               |
| ------------------------------------------------- | -------------------------------- | -------------------------------------------------------- |
| `CashFlow.Web`                                    | Sim (**apenas** Contracts)       | `CashFlowApiClient` consome DTOs; sem Application/Shared |
| `CashFlow.Application`                            | Sim                              | Services retornam/aceitam DTOs de contrato               |
| `CashFlow.Api`                                    | Indiretamente (via Application)  | Controllers usam `CashFlow.Contracts` nos tipos de API   |
| `CashFlow.Domain` / `Infrastructure` / `Consumer` | Não                              | Domínio e mensageria permanecem em Domain/Shared         |

A pasta `Application/Dtos/` foi removida; tipos migrados para `Contracts`.

Cobertura de testes: `CashFlow.Contracts` é **excluído** do gate de cobertura em `coverlet.runsettings` (tipos anêmicos, sem regra de negócio).

---

## Justificativa

1. **DIP na borda da UI** — Web depende só de contratos públicos, não de Application.
2. **Contrato explícito** — mesmo assembly para documentação OpenAPI e cliente Blazor.
3. **Build leve** — projeto sem pacotes; compilação rápida para hosts que só precisam dos tipos.

---

## Consequências Positivas

- Fronteira clara entre apresentação (Web) e casos de uso (Application)
- Evolução dos DTOs centralizada sem arrastar Domain para a UI
- Alinhamento com APIs que expõem modelos compartilhados em assembly dedicado

---

## Consequências Negativas

- Mais um projeto na solução
- Mapeamento entidade ↔ DTO continua na Application (não eliminado, apenas isolado)

---

## Alternativas Consideradas

1. **Web referenciar Application** — Rejeitado por acoplamento e dependências transitivas desnecessárias.
2. **.NET Standard / pacote NuGet externo** — Rejeitado neste repositório; solução inteira em `net10.0`.
3. **Duplicar DTOs na Web** — Rejeitado por divergência API/UI.

---

## Referências

- [ADR-003](ADR-003-arquitetura-ddd-camadas.md) — camadas DDD
- [ADR-007](ADR-007-frontend-blazor.md) — Blazor Server
- [code-standards.md](../code-standards.md) — convenção de DTOs
