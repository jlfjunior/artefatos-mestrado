# ADR-015 – Lançamentos de caixa imutáveis

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Em controle de fluxo de caixa, movimentações registradas representam fatos financeiros já ocorridos. Alterar ou excluir um lançamento compromete rastreabilidade e auditoria.

---

## Decisão

O agregado `Transaction` é **imutável** após criação:

- API expõe apenas **POST** e **GET** em `/api/transactions`
- Repositório: `InsertAsync` + leituras; sem `Update`/`Delete`
- Domínio: sem método `Update()` no agregado
- UI: sem ação de exclusão na listagem

Correções futuras devem ser modeladas como **novos lançamentos** (ex.: estorno), não edição do original.

---

## Justificativa

1. Alinha com prática contábil de registro append-only.
2. Simplifica projeção do consolidado (apenas eventos de criação).
3. Compatível com Event Message no passado ([ADR-004](ADR-004-integracao-assincrona-rabbitmq.md)).

---

## Consequências Positivas

- Menos superfície de API e mensagens
- Histórico confiável para auditoria

---

## Consequências Negativas

- Usuário não pode “corrigir” lançamento errado via UI (precisa lançamento compensatório)

---

## Alternativas Consideradas

1. **CRUD completo com eventos updated/deleted** — Rejeitado por regra de negócio.
2. **Soft delete** — Rejeitado; ainda permitiria apagar fato do consolidado.
