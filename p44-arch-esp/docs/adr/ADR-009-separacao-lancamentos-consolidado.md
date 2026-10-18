# ADR-009 – Separação lançamentos vs consolidado

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Requisito não funcional: *"O serviço de controle de lançamento não deve ficar indisponível se o sistema de consolidado diário cair."*

---

## Decisão

Separar em **dois processos deployáveis** além do frontend:

1. **`CashFlow.Api`** — API HTTP única com lançamentos e consulta de consolidado
2. **`CashFlow.Consumer.Consolidation`** — processamento assíncrono da projeção de saldo

A escrita de lançamentos e a publicação de mensagens ocorrem na API. A materialização do consolidado roda no consumer. Em picos de leitura, a API escala horizontalmente por réplicas.

---

## Justificativa

1. Consumer indisponível não bloqueia novos lançamentos (fila bufferiza).
2. API unificada reduz fragmentação operacional e simplifica o Blazor (uma base URL).
3. Consumer pode ser escalado ou pausado independentemente.

---

## Consequências Positivas

- Isolamento de falhas no processamento assíncrono
- Menos serviços HTTP para operar

---

## Consequências Negativas

- Consistência eventual no consolidado
- Consumer exige monitoramento de fila

---

## Alternativas Consideradas

1. **Duas APIs (Transactions + Consolidation)** — Rejeitada por fragmentação desnecessária neste escopo.
2. **Monólito síncrono** — Rejeitado por acoplar disponibilidade.
