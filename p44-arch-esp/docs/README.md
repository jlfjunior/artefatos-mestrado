# Documentação do repositório

Índice dos materiais em `docs/` para navegação rápida.

## Padrões de código

- [**Code standards**](code-standards.md) — convenções de nomes, clean code, documentação XML, tabelas, endpoints e eventos.

## Arquitetura C4

- [**Definições C4 – Fluxo de Caixa**](c4-definicoes-fluxo-caixa.md) — contexto, containers, componentes, modelo de dados e fluxos.
- [**Diagramas visuais (Mermaid)**](diagrams/README.md) — contexto e containers.
- [**Ambiente AWS / Kubernetes**](proposta-ambiente.md) — implantação alvo em EKS, RDS, Kong, Terraform e observabilidade.

## Teste de carga

- [**Load test — fluxo de caixa**](load-test/README.md) — script k6 com escrita (`POST /api/transactions`) e leitura do consolidado (50 req/s).

## Decisões arquiteturais (ADRs)

Arquivos em [`adr/`](./adr/), numerados em sequência:

| ADR                                                             | Tema                                    |
| --------------------------------------------------------------- | --------------------------------------- |
| [ADR-000](adr/ADR-000-template.md)                              | Template para novos ADRs                |
| [ADR-001](adr/ADR-001-persistencia-postgresql.md)               | Persistência PostgreSQL                 |
| [ADR-002](adr/ADR-002-framework-dotnet10.md)                    | Framework .NET 10                       |
| [ADR-003](adr/ADR-003-arquitetura-ddd-camadas.md)               | Arquitetura DDD em camadas              |
| [ADR-004](adr/ADR-004-integracao-assincrona-rabbitmq.md)        | Integração assíncrona com RabbitMQ      |
| [ADR-005](adr/ADR-005-acesso-dados-dapper.md)                   | Acesso a dados com Dapper               |
| [ADR-006](adr/ADR-006-migrations-fluentmigrator.md)             | Migrações com FluentMigrator            |
| [ADR-007](adr/ADR-007-frontend-blazor.md)                       | Frontend Blazor                         |
| [ADR-008](adr/ADR-008-docker-compose.md)                        | Execução com Docker Compose             |
| [ADR-009](adr/ADR-009-separacao-lancamentos-consolidado.md)     | Separação lançamentos vs consolidado    |
| [ADR-010](adr/ADR-010-publicacao-mensagens-alteracoes-banco.md) | Publicação de mensagens em alterações   |
| [ADR-011](adr/ADR-011-testes-xunit-tdd.md)                      | Testes com xUnit e TDD                  |
| [ADR-012](adr/ADR-012-convencoes-documentacao-idioma.md)        | Convenções de idioma (código vs docs)   |
| [ADR-013](adr/ADR-013-solid-eip.md)                             | SOLID e Enterprise Integration Patterns |
| [ADR-014](adr/ADR-014-ci-github-actions.md)                     | CI com GitHub Actions                   |
| [ADR-015](adr/ADR-015-lancamentos-imutaveis.md)                 | Lançamentos de caixa imutáveis          |
| [ADR-016](adr/ADR-016-oauth2-jwt-autenticacao.md)               | OAuth2, JWT e autorização por escopos   |
| [ADR-017](adr/ADR-017-camada-contracts.md)                      | Camada `CashFlow.Contracts` (DTOs HTTP) |

## Raiz do projeto

- [**README principal**](../README.md) — como executar a aplicação, estrutura e scripts.
