# ADR-004 – Integração assíncrona com RabbitMQ

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Requisito não funcional: o serviço de lançamentos não pode ficar indisponível se o consolidado diário cair. A integração deve seguir **Event Message** (EIP): eventos nomeados no passado, publicados em exchanges dedicadas.

---

## Decisão

Adotar **RabbitMQ** como message broker, aplicando padrões EIP:

- **Event Message** — uma exchange **fanout** por evento de domínio
- **Message Channel** durável (`cashflow.consolidation`) vinculada a todas as exchanges de evento
- **Event-Driven Consumer** no `Consumer.Consolidation`
- **Dead Letter Channel** para falhas persistentes

Evento inicial:

| Evento            | Exchange                             |
| ----------------- | ------------------------------------ |
| Lançamento criado | `cashflow.transaction.created.event` |

**Mensagem de negócio:** CloudEvent 1.0 com `TransactionCreatedData` em `data`. O atributo `type` (`cashflow.transaction.created.event`) coincide com a exchange; cada consumer desserializa seu CloudEvent tipado.

---

## Justificativa

1. Desacopla API do consumer em tempo de execução.
2. Event Message documenta fatos imutáveis do passado ([EIP](https://www.enterpriseintegrationpatterns.com/patterns/messaging/EventMessage.html)).
3. Novos eventos = nova exchange + binding, sem alterar contratos existentes.

---

## Consequências Positivas

- Resiliência alinhada ao requisito de negócio
- Contrato de mensagens testável (`IntegrationMessageSerializer`)
- Semântica explícita no broker por evento

---

## Consequências Negativas

- Consistência eventual no consolidado
- Operação de filas e exchanges (monitoramento, DLQ)

---

## Alternativas Consideradas

1. **Exchange única com routing keys por operação** — Rejeitada; preferimos Event Message com exchange por evento.
2. **Chamada HTTP síncrona** — Rejeitada por violar requisito de disponibilidade.
