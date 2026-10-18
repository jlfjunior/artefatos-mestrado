# ADR-005 – Acesso a dados com Dapper

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Necessidade de persistência SQL performática, com controle explícito de queries para leituras de consolidado em alto volume. Complementa a escolha de PostgreSQL ([ADR-001](ADR-001-persistencia-postgresql.md)).

---

## Decisão

Usar **Dapper** como micro-ORM para todos os repositórios PostgreSQL.

---

## Justificativa

1. Queries SQL explícitas e otimizáveis para relatório diário.
2. Overhead mínimo em comparação com ORM completo.

---

## Consequências Positivas

- Performance previsível
- SQL transparente em code review

---

## Consequências Negativas

- Mapeamento manual entidade/coluna
- Sem change tracking automático

---

## Alternativas Consideradas

1. **Entity Framework Core** — Rejeitado por preferência explícita do projeto e controle de SQL.
2. **ADO.NET puro** — Rejeitado por verbosidade desnecessária.
