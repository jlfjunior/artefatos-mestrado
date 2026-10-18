# ADR-010 – Publicação de mensagens em alterações no banco

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Requisito: *"Todas operações que provocam alterações no banco devem publicar mensagens."*

Lançamentos de caixa são **imutáveis** após criação: não há UPDATE nem DELETE em `transactions`.

---

## Decisão

Toda operação de **INSERT** em `transactions` publica **Event Message**:

| Operação | Evento / Exchange                    | Mensagem                    |
| -------- | ------------------------------------ | --------------------------- |
| Insert   | `cashflow.transaction.created.event` | `TransactionCreatedMessage` |

Publicação via `IMessagePublisher.PublishAsync(IIntegrationMessage)` na exchange do evento.

Fluxo: transação PostgreSQL → persistência → publish → commit. Se a publicação falhar, a transação é revertida (rollback). **Limitação:** o broker não participa da transação SQL; falha após publish bem-sucedido e antes do commit exigiria Outbox (evolução futura).

---

## Justificativa

1. Event Message reflete fato ocorrido no passado.
2. Consumer desserializa tipo fixo; a exchange identifica o evento no broker.
3. Alinhado a Enterprise Integration Patterns.

---

## Consequências Positivas

- Contrato claro por evento
- Extensível com novos `IIntegrationMessage` e exchanges

---

## Consequências Negativas

- Dupla escrita (DB + broker); Outbox recomendado em produção

---

## Alternativas Consideradas

1. **Mensagens de update/delete** — Rejeitadas por violar regra de negócio financeira.
2. **Exchange única com enum no payload** — Rejeitada em favor de Event Message por exchange ([ADR-004](ADR-004-integracao-assincrona-rabbitmq.md)).
