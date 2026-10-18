# ADR-001 – Base de persistência: PostgreSQL

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

O sistema de fluxo de caixa precisa armazenar lançamentos financeiros e saldos consolidados com integridade referencial, transações ACID e consultas analíticas por data.

---

## Decisão

Adotar **PostgreSQL** como SGBD relacional único do sistema.

Modelo inicial de dados:

- Tabela `transactions` (lançamentos)
- Tabela `daily_balances` (projeção consolidada)

Ferramentas de acesso (ORM) e versionamento de schema ficam fora do escopo desta decisão.

---

## Justificativa

1. ACID garante consistência em operações financeiras.
2. Índices por data suportam consultas do consolidado em picos de leitura.
3. Ferramentas maduras para backup, réplicas e operação em Docker.

---

## Consequências Positivas

- Modelo relacional claro e auditável
- Suporte a constraints e tipos numéricos precisos (`NUMERIC`)

---

## Consequências Negativas

- Alterações de schema exigem versionamento e deploy coordenado entre ambientes
- Escala de leitura extrema pode exigir réplicas ou cache (evolução futura)

---

## Alternativas Consideradas

1. **MongoDB** — Rejeitada por priorizar consistência transacional.
2. **SQL Server** — Rejeitada por alinhamento com stack open source e Docker.
