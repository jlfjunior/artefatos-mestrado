# ADR-014 – CI com GitHub Actions

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Repositório público no GitHub com fluxo de CI que compila e executa testes a cada push e pull request. Automatiza a suíte definida em [ADR-011](ADR-011-testes-xunit-tdd.md).

---

## Decisão

Workflow `.github/workflows/ci.yml` com **dois jobs**:

### Job `build`

1. Trigger: `push` e `pull_request` na branch `main`
2. Runner `ubuntu-latest` com **.NET 10 SDK**
3. Cache de pacotes NuGet (`setup-dotnet` + `cache-dependency-path`)
4. Passos: `dotnet restore` → `dotnet build --configuration Release --no-restore` → upload do artefato `dotnet-build` (`src/**/bin/Release`, `tests/**/bin/Release`)

### Job `test` (depende de `build`)

1. Mesmo runner e SDK; checkout + download do artefato `dotnet-build` produzido no job anterior (sem restore/build repetidos)
2. `dotnet test CashFlow.sln --no-build` com `coverlet.runsettings` e coleta Cobertura — suíte completa (Domain, Application, Infrastructure)
3. Gate de qualidade: `scripts/check-coverage.py` exige **≥ 80%** em `CashFlow.Domain` + `CashFlow.Application` (exclusões: `Program`, `Migrations`, `DependencyInjection`)

### Job `cleanup` (depende de `build` e `test`)

1. Executa com `if: always()` quando o build teve sucesso (mesmo se os testes falharem)
2. Remove o artefato `dotnet-build` via `geekyeggo/delete-artifact@v6` (`permissions: actions: write`)

## Justificativa

1. Feedback rápido em PRs com jobs separados (build visível antes dos testes)
2. Gate mínimo de qualidade definido para o repositório, com cobertura focada na regra de negócio
3. GitHub Actions nativo para repositório GitHub
4. .NET 10 alinhado à stack da solução

---

## Consequências Positivas

- Regressões bloqueadas antes do merge
- Cobertura mensurável e reproduzível localmente:

  ```bash
  dotnet test CashFlow.sln --configuration Release \
    --settings coverlet.runsettings \
    --collect:"XPlat Code Coverage" --results-directory coverage-out
  python3 scripts/check-coverage.py 80 coverage-out
  ```

- Documentação viva de como compilar e validar

---

## Consequências Negativas

- CI não executa Docker Compose completo no MVP (custo/tempo); evolução com job de integração
- Cobertura não inclui camadas de entrega (Api, Web, Consumer) nem DTOs — aceito para manter o gate focado

---

## Alternativas Consideradas

1. **Azure Pipelines** — Rejeitado por preferência GitHub Actions para repo público.
2. **Sem CI** — Rejeitado por requisito explícito.
3. **Job único build+test** — Substituído por jobs separados para clareza no painel do GitHub.
4. **Cobertura da solution inteira** — Rejeitado; infraestrutura e hosts diluem o indicador sem refletir regras de negócio.
