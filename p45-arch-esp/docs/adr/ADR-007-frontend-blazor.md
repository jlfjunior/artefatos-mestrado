# ADR-007 – Frontend Blazor

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

O sistema exige backend e frontend na stack C#/.NET, com experiência web para o comerciante gerenciar lançamentos e consultar consolidado.

---

## Decisão

Adotar **Blazor Server** no projeto `CashFlow.Web`:

- Páginas para criar e listar lançamentos (imutáveis após criação)
- Página de relatório de saldo diário
- Cliente HTTP para Transactions API e Consolidation API

UI em componentes Razor; textos de interface em português (Brasil); chamadas de API em rotas inglesas.

---

## Justificativa

1. Stack unificada C# reduz contexto switching.
2. Blazor Server simplifica autenticação e estado para o MVP do produto.
3. Integração natural com ASP.NET Core hosting.

---

## Consequências Positivas

- Um ecossistema de linguagem
- Desenvolvimento rápido de formulários

---

## Consequências Negativas

- Blazor Server mantém circuito SignalR (escala horizontal exige sticky sessions ou migrar para WASM)
- Separação visual clara entre Web e APIs exige CORS/configuração de URLs

---

## Alternativas Consideradas

1. **Blazor WebAssembly** — Adiado por maior complexidade de deploy inicial.
2. **React/Angular separado** — Rejeitado por exigir stack adicional fora do escopo C#.
