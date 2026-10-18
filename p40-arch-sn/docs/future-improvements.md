# Melhorias Futuras

Este documento descreve evoluções planejadas para o sistema FluxoCaixa. Todas as decisões de escopo foram tomadas conscientemente para manter o foco nas decisões arquiteturais dentro do tempo disponível, estas melhorias demonstram a visão de longo prazo do sistema.

---

## 1. Redis — Cache do Consolidado Diário

**Motivação:**
Os endpoints do serviço de consolidado diário são os mais acessados nos picos de 50 req/s. Consultar o PostgreSQL a cada requisição é desnecessário para dados que mudam com baixa frequência ao longo do dia.

**Como seria implementado:**
- Cache com TTL de 1 minuto no `GetDailyReportHandler`
- Invalidação do cache ao processar um novo `EntryCreatedEvent` no consumer
- Redis rodando como container adicional no `docker-compose.yml`
- Interface `ICacheService` no Domain para não acoplar a Application ao Redis diretamente

**Impacto esperado:**
- Redução das consultas ao PostgreSQL nos picos
- Menor latência de resposta do consolidado
- Suporte confortável aos 50 req/s com margem para crescimento

---

## 2. Outbox Pattern — Maior Confiabilidade na Entrega de Eventos

**Motivação:**
Atualmente existe uma janela de falha entre o `SaveChanges` (persistir o lançamento) e o `BasicPublish` (publicar no RabbitMQ). Se a aplicação cair entre essas duas operações, o evento se perde e o consolidado nunca será atualizado para aquele lançamento.

**Como seria implementado:**
- Tabela `outbox_messages` no mesmo banco `db_entries`
- Ao criar um lançamento, persiste o evento na tabela outbox na mesma transação do lançamento
- Worker em background lê a tabela outbox e publica no RabbitMQ
- Após confirmação do broker, marca a mensagem como processada

**Impacto esperado:**
- Elimina a janela de falha entre persistir e publicar
- Reduz significativamente a perda de eventos em cenários de falha
- O consumer do consolidado precisa ser idempotente para lidar com possíveis duplicatas (at-least-once delivery)

> O Outbox Pattern não garante zero perda de eventos, mas garante entrega pelo menos uma vez. Para maior resiliência, podemos combinar com o Dead Letter Queue para mensagens que falham repetidamente e consumer idempotente para lidar com duplicatas.

---

## 3. Autenticação e Autorização com Roles

**Motivação:**
Atualmente o sistema apenas autentica (verifica quem é o usuário via JWT), mas não autoriza (verifica o que cada usuário pode fazer). Qualquer portador de um token válido tem acesso total a todos os endpoints.

**Como seria implementado:**

**Roles planejadas:**
| Role | Permissões |
|---|---|
| `Operator` | Criar lançamentos (`POST /api/entries`) |
| `Manager` | Visualizar lançamentos e consolidado (`GET`) |
| `Admin` | Acesso total |

**Nos controllers:**
```csharp
[Authorize(Roles = "Admin,Operator")]
[HttpPost]
public async Task<IActionResult> CreateAsync(...)

[Authorize(Roles = "Admin,Manager")]
[HttpGet]
public async Task<IActionResult> GetByDateAsync(...)
```

**Endpoint de Login:**
- Serviço de identidade dedicado ou endpoint `/auth/login` recebendo `username` e `password`
- Geração do JWT com as claims e roles do usuário
- Refresh token para renovação sem novo login
- Integração com provedor de identidade externo (ex: Keycloak, Azure AD) para ambientes corporativos

**Impacto esperado:**
- Controle granular de acesso por perfil de usuário
- Auditoria de quem criou cada lançamento (claim `sub` do JWT)

---

## 4. Observabilidade — Serilog + Seq

**Motivação:**
Atualmente os logs são apenas o output padrão do ASP.NET Core no console do Docker. Em produção, precisamos de logs estruturados, centralizados e pesquisáveis para diagnosticar problemas rapidamente.

**Como seria implementado:**
- Serilog nos dois serviços com enrichers de `CorrelationId`, `ServiceName` e `Environment`
- Seq como servidor de logs rodando no `docker-compose.yml`
- Logs estruturados em JSON com níveis adequados por contexto
- Rastreabilidade de uma requisição do Gateway até o consumer do RabbitMQ via `CorrelationId`

**Exemplo de log estruturado:**
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "Information",
  "service": "Entries.API",
  "correlationId": "abc-123",
  "message": "Entry created successfully",
  "entryId": "e691a91e-...",
  "amount": 150.00,
  "type": "Credit"
}
```

**Impacto esperado:**
- Diagnóstico rápido de falhas em produção
- Rastreabilidade end-to-end dos eventos
- Dashboard de métricas e alertas no Seq

---

## 5. Health Checks

**Motivação:**
Monitoramento proativo da saúde dos serviços e suas dependências (PostgreSQL, RabbitMQ). Sem health checks, falhas silenciosas podem passar despercebidas.

**Como seria implementado:**
- Endpoint `/health` em cada serviço verificando:
  - Conectividade com o PostgreSQL
  - Conectividade com o RabbitMQ
  - Saúde geral do serviço
- Integração com o YARP para roteamento inteligente, ajudando a não rotear para instâncias não saudáveis
- Health checks no `docker-compose.yml` para dependências de startup

```csharp
builder.Services.AddHealthChecks()
    .AddNpgsql(connectionString)
    .AddRabbitMQ(rabbitMqUri);
```

**Impacto esperado:**
- Detecção proativa de falhas antes que afetem os usuários
- Restart automático de containers não saudáveis pelo Docker
- Base para integração com ferramentas de monitoramento (Prometheus, Grafana)

---

## 6. Testes de Integração com Testcontainers

**Motivação:**
Os testes atuais são unitários e usam mocks para repositórios e mensageria. Isso não garante que a integração real com PostgreSQL e RabbitMQ funciona corretamente.

**Como seria implementado:**
- Testcontainers para subir instâncias reais de PostgreSQL e RabbitMQ nos testes
- Testes de integração cobrindo o fluxo completo: criar lançamento → publicar evento → consolidar
- Testes de contrato para garantir compatibilidade entre o publisher e o consumer

**Impacto esperado:**
- Cobertura de integração automatizada
- Detecção de problemas de integração antes do deploy
- Maior confiança nas mudanças de schema e infraestrutura

---

## 7. Rate Limiting

**Motivação:**
Proteção contra abuso e ataques de negação de serviço nos endpoints públicos.

**Como seria implementado:**
- Middleware de rate limiting no Gateway com limite por IP e por token JWT
- Limites diferenciados por endpoint:
  - `POST /api/entries`: 100 req/min por token
  - `GET /api/consolidation`: 300 req/min por token
- Resposta `429 Too Many Requests` com header `Retry-After`

**Impacto esperado:**
- Proteção contra abuso sem impactar usuários legítimos
- Prevenção de sobrecarga nos serviços downstream

---

## 8. Elasticsearch para o Serviço de Consolidado
 
**Motivação:**
O banco atual (PostgreSQL) atende bem o escopo inicial, mas o serviço de consolidado tem características que se beneficiam muito de um banco orientado a documentos e buscas:
- Consultas por período de datas
- Relatórios agregados (soma de créditos/débitos por semana, mês, ano)
- Alto volume de leituras nos picos de 50 req/s ou mais
- Dados que crescem continuamente e raramente são alterados após consolidados

**Como seria implementado:**
- Substituir o PostgreSQL do `Consolidation.Infrastructure` pelo Elasticsearch
- O consumer do RabbitMQ indexaria os documentos `DailyBalance` diretamente no Elasticsearch
- Queries de período usando o poder nativo de range queries do Elasticsearch
- PostgreSQL poderia ser mantido como fonte de verdade com Elasticsearch como camada de leitura, seguindo o padrão CQRS, com as escritas no PostgreSQL e as leituras no Elasticsearch
- Interface `IDailyBalanceRepository` já abstrai o banco, facilitando a troca sem impacto nas camadas superiores

**Impacto esperado:**
- Consultas de período extremamente rápidas independente do volume de dados
- Capacidade de relatórios complexos (agrupamentos por semana, mês, ano) sem impacto de performance
- Escalabilidade horizontal nativa do Elasticsearch para crescimento contínuo de dados
- Redução de carga no PostgreSQL nos picos de leitura

---
 
## 9. Escalabilidade Horizontal
 
**Motivação:**
Para suportar crescimento além dos 50 req/s atuais, os serviços precisam escalar horizontalmente com múltiplas instâncias.
 
**Como seria implementado:**
- Múltiplas instâncias de `Entries.API` e `Consolidation.API` com load balancing no YARP
- Consumers concorrentes no RabbitMQ com `prefetchCount` ajustado por instância
- Redis para cache compartilhado entre instâncias (descrito no item 1)
- Sessão stateless via JWT garante que qualquer instância atenda qualquer requisição
- Orquestração com Kubernetes em produção substituindo o Docker Compose

**Impacto esperado:**
- Capacidade de suportar mais req/s com adição de instâncias
- Zero downtime em deploys com rolling updates
- Auto-scaling baseado em métricas de CPU e tamanho da fila RabbitMQ