# ADR-013 – SOLID e Enterprise Integration Patterns

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Com a estrutura em camadas já definida ([ADR-003](ADR-003-arquitetura-ddd-camadas.md)), o projeto aplica **SOLID** e **Enterprise Integration Patterns (EIP)** de forma explícita na integração assíncrona ([ADR-004](ADR-004-integracao-assincrona-rabbitmq.md)). Esta ADR documenta como esses princípios e padrões aparecem no código.

---

## Decisão

Aplicar **SOLID** sobre a arquitetura em camadas ([ADR-003](ADR-003-arquitetura-ddd-camadas.md)), via portas e adaptadores:

- **SRP:** cada handler de caso de uso uma responsabilidade
- **OCP:** novos eventos/consumers sem alterar domínio
- **LSP:** repositórios substituíveis por fakes em testes
- **ISP:** interfaces pequenas (`ITransactionRepository`, `IMessagePublisher`)
- **DIP:** Application depende de abstrações, Infrastructure implementa

Aplicar **EIP** selecionados:

| Padrão                | Uso                                                                                       |
| --------------------- | ----------------------------------------------------------------------------------------- |
| Event Message         | Exchange `cashflow.transaction.created.event` por evento                                  |
| Message Channel       | Fila `cashflow.consolidation` durável                                                     |
| Publish-Subscribe     | Exchanges fanout vinculadas à fila de consolidado                                         |
| Event-Driven Consumer | Consumer.Consolidation                                                                    |
| Idempotent Receiver   | **Parcial:** `last_event_id` persistido; deduplicação antes de aplicar delta **pendente** |
| Dead Letter Channel   | DLX para mensagens com falha                                                              |

---

## Justificativa

1. Padrões nomeados facilitam comunicação em docs C4/ADR.
2. Testabilidade e manutenção a longo prazo.

---

## Consequências Positivas

- Código alinhado a vocabulário de arquitetura corporativa
- Extensão para novos bounded contexts

---

## Consequências Negativas

- Curva de aprendizado para time menos familiarizado com EIP
- Idempotência completa ainda não implementada — reprocessamento pode duplicar delta até evolução futura

---

## Alternativas Consideradas

1. **Acoplamento direto entre serviços** — Rejeitado por SOLID/EIP/resiliência.
