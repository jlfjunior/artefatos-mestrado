# Changelog

Todas as mudancas significantes neste projeto serao documentadas neste arquivo.

O formato e baseado em [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
e este projeto segue [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2025-06-16

### Release Inicial

#### Adicionado

**Servico de Transacoes (Transactions API)**
- Criar, listar e obter lancamentos (debito/credito) por lojista
- Validacoes de negocio (Amount > 0, data nao futura)
- Publicacao de `TransactionCreatedEvent` via MassTransit + RabbitMQ
- Endpoint `/health` com checks de SQL Server e RabbitMQ

**Servico de Consolidacao (Consolidation API)**
- Consumo de `TransactionCreatedEvent` via MassTransit + RabbitMQ
- Calculo de saldo consolidado por lojista por dia
- Publicacao de `ConsolidationUpdatedEvent` apos cada atualizacao
- Endpoints para listar e obter consolidacoes por data/periodo

**Multi-loja (Lojista)**
- Suporte a 4 lojas independentes: Loja Norte, Loja Sul, Loja Leste, Loja Oeste
- Consolidacao diaria segregada por lojista
- Indice unico `(ConsolidationDate, Lojista)` garantindo integridade

**Banco de Dados**
- SQL Server com Entity Framework Core
- Inicializacao via `SQL_CREATE_DATABASE.sql` (sem EF migrations)
- Indices em campos criticos (TransactionDate, CreatedAt, ConsolidationDate+Lojista)

**Testes**
- Unit tests para camada Domain (Transaction, Consolidation)
- Unit tests para camada Application (TransactionService, ConsolidationService)
- xUnit + Moq + FluentAssertions
- Testcontainers configurado (pronto para integration tests)

**Documentacao**
- `ARCHITECTURE.md` com visao geral, diagrama e padroes
- `docs/ADRs.md` com 7 Architecture Decision Records
- `docs/API.md` com referencia de endpoints
- `docs/DEVELOPMENT.md` com guia de desenvolvimento
- `README.md` com instrucoes completas e exemplos

**Docker e Infraestrutura**
- Dockerfiles multi-stage otimizados
- `docker-compose.yml` com SQL Server, sql-init, RabbitMQ, Transactions API, Consolidation API
- Health checks em todos os containers

**CI/CD**
- GitHub Actions: build, testes, Docker push para ghcr.io, Trivy vulnerability scan

**Configuracao**
- `.gitignore` para .NET
- `.editorconfig` com padroes de codigo
- `FluxoCaixa.sln` (Visual Studio solution)
- MIT License

#### Tecnologias Utilizadas

| Tecnologia | Versao |
|------------|--------|
| .NET / C# | 8 / 12 |
| Entity Framework Core | 8.0 |
| MassTransit | 8.1.1 |
| RabbitMQ | 3.12 |
| Serilog | 4.0 |
| xUnit | 2.6 |
| Moq | 4.20 |
| FluentAssertions | 6.12 |
| Testcontainers | 3.5 |
| SQL Server | 2022 |

#### Padroes Implementados

- Clean Architecture (Domain / Application / Infrastructure / EntryPoint)
- Repository Pattern + Unit of Work
- Event-Driven Architecture (produtor/consumidor via MassTransit)
- SOLID Principles
- Dependency Injection

---

## [1.1.0] - Planejado

#### Feature: Authentication & Authorization
- JWT Bearer tokens
- Role-based access control (User, Admin)
- Audit logging

#### Feature: Caching
- Redis para saldos consolidados
- Invalidacao em tempo real via ConsolidationUpdatedEvent
- TTL configuravel

#### Feature: Observabilidade
- Prometheus metrics
- Distributed tracing (Jaeger / OpenTelemetry)
- Logging centralizado

---

## [1.2.0] - Planejado

#### Feature: Multi-Database
- Banco separado por servico
- Saga Pattern para transacoes distribuidas
- Event Sourcing completo

#### Feature: Webhooks
- Integracoes externas via ConsolidationUpdatedEvent
- Callbacks com retry automatico

---

## [2.0.0] - Planejado

#### Feature: ML & Analytics
- Forecasting de caixa por lojista
- Anomaly detection
- Dashboards e relatorios

#### Feature: Compliance
- Multi-tenant
- LGPD compliance
- SOX audit trail
- Criptografia em repouso
