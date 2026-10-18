# Architecture Decision Records (ADRs)

Este documento registra as principais decisões arquiteturais tomadas durante o desenvolvimento do sistema FluxoCaixa, incluindo o contexto, as alternativas consideradas e os motivos de cada escolha.

---

## ADR-001 — Padrão Arquitetural

**Status:** Aceito

**Contexto:**
O sistema possui dois domínios funcionais distintos: controle de lançamentos e o consolidado diário. Um requisito não-funcional crítico determina que o serviço de lançamentos não deve ficar indisponível se o serviço de consolidado cair. Isso exige um isolamento entre os dois contextos.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| Monolito Modular | Simples de operar, deploy único | Falha em um módulo pode afetar o outro, acoplamento implícito |
| **Microsserviços** | **Isolamento entre os serviços, deploys independentes** | **Maior complexidade operacional** |

**Decisão:**
Microsserviços com dois serviços independentes: `Entries.API` e `Consolidation.API`, cada um com seu próprio banco de dados e ciclo de vida.

**Consequências:**
- Falha no serviço de consolidado não afeta o serviço de lançamentos
- Cada serviço pode ser escalado horizontalmente de forma independente
- Times diferentes poderiam evoluir cada serviço sem conflitos
- Maior complexidade de infraestrutura
- Consistência eventual entre os serviços

---

## ADR-002 — Comunicação entre os Microsserviços

**Status:** Aceito

**Contexto:**
Com dois microsserviços independentes, precisamos de um mecanismo de comunicação que garanta que os eventos de lançamento cheguem ao serviço de consolidado mesmo que ele esteja temporariamente indisponível. A comunicação síncrona (HTTP) criaria acoplamento temporal entre os serviços.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| HTTP Síncrono | Simples, sem infra adicional | Acoplamento temporal, falha no consolidado afeta lançamentos |
| Kafka | Alto throughput, replay de eventos | Complexidade operacional excessiva para esse escopo |
| **RabbitMQ** | **Simples, durable queues, amplamente adotado** | **Menor throughput que Kafka** |

**Decisão:**
RabbitMQ com `durable queues` e `persistent messages`. Exchange do tipo `Direct` com routing key `entry.created`. Consumer com `prefetchCount: 10` e `manual ack` para garantir processamento confiável.

**Consequências:**
- Lançamentos funcionam mesmo com o consolidado fora do ar
- Mensagens persistidas sobrevivem a restart do broker
- `BasicNack` com `requeue: true` garante reprocessamento em caso de falha
- Suporta os picos de 50 req/s com buffer natural da fila
- Consistência eventual: o saldo consolidado pode ter um pequeno atraso em relação aos lançamentos

---

## ADR-003 — Banco de Dados Dedicado por Serviço

**Status:** Aceito

**Contexto:**
Em arquiteturas de microsserviços, compartilhar banco de dados entre serviços cria acoplamento implícito, mudanças no schema de um serviço podem quebrar o outro. Cada serviço deve ser dono dos seus próprios dados.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| SQL Server | Comum em ambientes .NET | Licença paga |
| **PostgreSQL por serviço** | **Open source** | **Integração menos nativa com ferramentas do ecossistema Microsoft** |

**Decisão:**
Dois bancos PostgreSQL independentes: `db_entries` para o serviço de lançamentos e `db_consolidation` para o serviço de consolidado. Cada um roda em seu próprio container.

**Consequências:**
- Isolamento entre os bancos, falha em um banco não afeta o outro
- Cada serviço pode evoluir seu schema de forma independente
- Possibilidade futura de usar bancos diferentes por serviço (ex: Elasticsearch no serviço consolidado, devido ao alto volume de busca e análise de dados)
- Não há transações distribuídas entre os serviços

---

## ADR-004 — Organização Interna dos Componentes

**Status:** Aceito

**Contexto:**
Cada microsserviço precisa de uma organização interna que separe claramente as responsabilidades, facilite os testes unitários e evite que regras de negócio vazem para camadas erradas.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| Anêmico (só DTOs e services) | Simples | Regras de negócio espalhadas, difícil de testar |
| Clean Architecture | Muito explícito na separação, boa testabilidade | Mais camadas e abstrações do que o necessário para o escopo do projeto |
| **DDD Leve** | **Separação por domínios, boa testabilidade** | **Maior esforço inicial de modelagem em comparação à abordagem anêmica** |

**Decisão:**
Quatro camadas por serviço: `Domain` → `Application` → `Infrastructure` → `API`. O domínio não conhece nenhuma outra camada. A infraestrutura implementa interfaces definidas no domínio (Dependency Inversion).

**Consequências:**
- Regras de negócio encapsuladas nas entidades e value objects
- Testes unitários isolados sem dependência de banco ou broker
- Fácil substituição de implementações de infraestrutura
- Domain Events desacoplam a publicação de mensagens da lógica de negócio

---

## ADR-005 — API Gateway

**Status:** Aceito

**Contexto:**
Com dois microsserviços expostos, precisamos de um ponto único de entrada para os clientes. O Gateway também centraliza a validação do JWT, evitando que cada serviço precise reimplementar a autenticação.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| Sem Gateway | Simples | Clientes precisam conhecer todos os serviços, JWT duplicado |
| Ocelot | Popular no ecossistema .NET | configuração mais verbosa |
| Nginx | Alta performance, amplamente usado | Configuração fora do ecossistema .NET |
| **YARP** | **Nativo .NET, configuração via appsettings, baixo overhead** | **Menos recursos que outas soluções** |

**Decisão:**
YARP (Yet Another Reverse Proxy) configurado via `appsettings.json` com roteamento por path e validação JWT centralizada.

**Consequências:**
- Clientes interagem com um único endpoint (`localhost:8080`)
- JWT validado uma única vez no Gateway
- Fácil adição de novos serviços no futuro via configuração
- Integração natural com o pipeline ASP.NET Core

---

## ADR-006 — Autenticação

**Status:** Aceito

**Contexto:**
Os endpoints precisam de autenticação. A escolha do mecanismo impacta a escalabilidade horizontal dos serviços.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| API Key | Simples | Sem expiração nativa, difícil rotação |
| Sessão com Redis | Permite revogação imediata | Dependência de Redis, estado compartilhado |
| **JWT Stateless** | **Sem estado, escalável horizontalmente** | **Revogação requer blacklist** |

**Decisão:**
JWT com validação de `issuer`, `audience`, `lifetime` e `signing key`. Token gerado externamente (jwt.io) para fins de teste, em produção seria gerado por um serviço de identidade dedicado (ex: Keycloak ou Identity Server).

**Consequências:**
- Sem estado compartilhado entre instâncias
- Validação eficiente sem consulta ao banco
- Revogação de tokens requer implementação de blacklist

---

## ADR-007 — Publicação no RabbitMQ

**Status:** Aceito

**Contexto:**
Ao criar um lançamento, precisamos publicar um evento no RabbitMQ para o serviço de consolidado. A forma como fazemos isso impacta o acoplamento entre o domínio e a infraestrutura.

**Alternativas Consideradas:**

| Alternativa | Prós | Contras |
|---|---|---|
| Publicar direto no handler | Simples | Handler conhece o broker, difícil de testar |
| Outbox Pattern | Garantia transacional | Mais complexidade, requer tabela de outbox |
| **Domain Events** | **Domínio desacoplado, fácil de testar** | **Sem garantia transacional (sem Outbox)** |

**Decisão:**
A entidade `Entry` levanta o `EntryCreatedEvent` internamente ao ser criada. O `CreateEntryHandler` lê os domain events após persistir e os publica no RabbitMQ via `IMessagePublisher`. A interface `IMessagePublisher` fica no Domain para evitar que a Infrastructure dependa da Application.

**Consequências:**
- A entidade é responsável por declarar o que aconteceu com ela
- Testes unitários validam os domain events sem infraestrutura
- O domínio não conhece RabbitMQ, apenas a interface
- Sem garantia de atomicidade entre persistir e publicar
