# ADR-011 – Testes com xUnit e TDD

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

O projeto adota testes em C# com boas práticas e **TDD** como disciplina de desenvolvimento.

---

## Decisão

Usar **xUnit** como framework de testes, organizados em:

- `CashFlow.Domain.Tests` — regras de domínio e agregados
- `CashFlow.Application.Tests` — casos de uso com mocks de portas
- `CashFlow.Infrastructure.Tests` — integração leve (opcional) e adaptadores

Bibliotecas auxiliares: **FluentAssertions**, **Moq**.

Ciclo TDD: red → green → refactor para regras de negócio e casos de uso críticos.

Execução via `dotnet test` na solution (`CashFlow.sln`).

---

## Justificativa

1. xUnit é padrão de facto no ecossistema .NET.
2. Testes de domínio garantem correção de cálculo de saldo e invariantes.
3. TDD documenta comportamento esperado via testes legíveis.

---

## Consequências Positivas

- Regressão detectada cedo
- Design orientado a interfaces (portas mockáveis)

---

## Consequências Negativas

- Tempo inicial maior por ciclo red-green
- Testes de integração com RabbitMQ/Postgres podem exigir Testcontainers (evolução)

---

## Alternativas Consideradas

1. **NUnit** — Equivalente; xUnit escolhido por convenção do template .NET.
2. **Sem testes de aplicação** — Rejeitado por não atender ao requisito de qualidade do repositório.
