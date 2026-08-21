# ADR-002 – Framework .NET 10

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

O projeto adota **C#** com a versão mais recente do **.NET**. O ambiente utiliza **.NET 10 SDK**.

---

## Decisão

Adotar **.NET 10** (`net10.0`) como target framework de todos os projetos da solução, com `global.json` fixando o SDK 10.0.301.

Usar recursos pertinentes da stack atual (OpenAPI nativo em `MapOpenApi`, templates de projeto atualizados).

---

## Justificativa

1. Alinhamento com versão mais recente instalada pelo time.
2. Melhorias de performance e tooling do runtime/SDK 10.
3. CI configurado com `dotnet-version: 10.0.x`.

---

## Consequências Positivas

- Stack atualizada
- Paridade entre dev, Docker (`sdk:10.0`, `aspnet:10.0`) e CI

---

## Consequências Negativas

- Máquinas sem SDK 10 precisam instalar ou usar Docker

---

## Alternativas Consideradas

1. **Permanecer em .NET 8 LTS** — Rejeitada após disponibilidade do .NET 10 no ambiente.
