# ADR-006 – Migrações com FluentMigrator

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Alterações de schema devem ser versionadas, reproduzíveis em Docker e CI, sem `EnsureCreated` ad hoc. Complementa a escolha de PostgreSQL ([ADR-001](ADR-001-persistencia-postgresql.md)).

---

## Decisão

Adotar **FluentMigrator** com migrations em C# no projeto `Infrastructure`, executadas na inicialização das APIs e do Worker (ou via entrypoint Docker).

Convenção: `Migration_YYYYMMDD_NNN_Description.cs`.

---

## Justificativa

1. Migrations code-first em C# alinhadas ao stack .NET.
2. Histórico auditável em tabela `VersionInfo`.
3. Integração simples com PostgreSQL via runner.

---

## Consequências Positivas

- Schema consistente entre ambientes
- Rollback forward documentado por migration

---

## Consequências Negativas

- Migrations devem ser idempotentes e revisadas em PR
- Rollback automático limitado (preferir migrations forward-only)

---

## Alternativas Consideradas

1. **Scripts SQL soltos** — Rejeitados por falta de versionamento integrado.
2. **EF Migrations** — Rejeitadas por uso de Dapper, não EF.
