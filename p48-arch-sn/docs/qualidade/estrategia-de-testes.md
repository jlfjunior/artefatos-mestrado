# Estratégia de testes

A arquitetura deste projeto existe para atender dois requisitos não-funcionais
bem específicos:

> O serviço de Lançamentos não pode ficar indisponível se o Consolidado cair.
> Em pico, o Consolidado recebe **50 req/s**, com no máximo **5% de perda** de
> requisições.

Como esses requisitos são o coração do desenho, montei a malha de testes em
camadas que, juntas, _amarram_ essas metas a verificações executáveis. Cada
camada cobre um tipo de risco diferente; nenhuma sozinha dá a confiança toda.

## As camadas

### 1. Unidade — a regra de negócio isolada

Testam o domínio e os casos de uso sem nenhuma infraestrutura. São rápidos e
rodam em qualquer máquina, sem Docker.

- `tests/Lancamentos.UnitTests` — invariantes do agregado `Lancamento` (valor
  positivo, tipo válido, descrição, impacto no saldo).
- `tests/Consolidado.UnitTests` — a regra de soma do `SaldoDiario`.
- `tests/Lancamentos.Application.UnitTests` — orquestração do
  `RegistrarLancamentoService`: persistir, publicar o evento certo e confirmar
  tudo num único commit (na ordem que o outbox exige). Usa dublês das portas.
- `tests/Consolidado.Application.UnitTests` — `AtualizarSaldoService` (incluindo
  **idempotência** por `LancamentoId`) e `ConsultarSaldoService` (cache-aside,
  o caminho quente dos 50 req/s). Também com dublês.

```bash
dotnet test tests/Lancamentos.UnitTests
dotnet test tests/Consolidado.UnitTests
dotnet test tests/Lancamentos.Application.UnitTests
dotnet test tests/Consolidado.Application.UnitTests
```

### 2. Integração — o fluxo real ponta a ponta

Sobem Postgres, RabbitMQ e Redis **reais** com Testcontainers e provam que o
fluxo assíncrono funciona de verdade: registrar lançamento → outbox publica →
consumidor processa → saldo projetado.

- `tests/Lancamentos.IntegrationTests` — a publicação via outbox.
- `tests/Consolidado.IntegrationTests` — a projeção e a idempotência no consumer.

Quando não há Docker no ambiente, esses testes são **skipados** automaticamente
(atributo `DockerDisponivelFact`), sem quebrar o build.

```bash
dotnet test tests/Lancamentos.IntegrationTests
dotnet test tests/Consolidado.IntegrationTests
```

### 3. Mutação — a qualidade dos próprios testes

Cobertura de linha não garante que os testes verificam comportamento. O
Stryker.NET injeta mutações no código de produção e confere se os testes as
detectam, dando um **mutation score** por projeto. Foco nas quatro camadas de
regra de negócio (Domain e Application dos dois serviços).

Detalhes, comandos e como interpretar o score em
[`mutation-testing.md`](mutation-testing.md).

```bash
dotnet tool restore
cd src/Consolidado/Consolidado.Domain && dotnet stryker
```

### 4. Carga — a meta de 50 req/s com ≤ 5% de perda

Um executável NBomber (`tests/Performance/LoadTests`) que injeta exatamente
**50 requisições por segundo** no endpoint `GET /consolidado/{data}` por 60s
sustentados, depois de autenticar no `/token`. Ele mede a taxa de sucesso e os
percentis de latência (p50/p95/p99) e **falha com exit code ≠ 0 se a perda
passar de 5%** — ou seja, transforma o RNF num critério de aprovação objetivo.

Não entra no `dotnet test`: é um teste de sistema que precisa das APIs e da
infra no ar. Rode com a stack do `docker compose` em pé:

```bash
docker compose up --build           # em outro terminal
dotnet run --project tests/Performance/LoadTests -c Release
```

URLs, credenciais, taxa e duração são configuráveis por variável de ambiente
(ver o README desta pasta de testes / o `Program.cs`).

### 5. Resiliência / perda de mensagem — o desacoplamento sob estresse

`tests/Resiliencia.IntegrationTests` prova, com infraestrutura real, que o
desacoplamento entrega o que a arquitetura promete:

- **Nada se perde**: publica 500 lançamentos pelo fluxo real e confere que a
  projeção reflete exatamente todos (soma de créditos e débitos correta).
- **A fila bufferiza**: derruba o consumidor no meio do fluxo, continua
  publicando, e mostra que (a) nada é projetado enquanto ele está fora e (b) ao
  voltar, ele drena o backlog inteiro e o saldo fica correto. É a prova direta
  de "o Lançamentos não cai com o Consolidado".
- **Idempotência sob reentrega**: a mesma mensagem entregue duas vezes não dobra
  o saldo.

Também skipam sem Docker.

```bash
dotnet test tests/Resiliencia.IntegrationTests
```

## Como as camadas se conectam aos RNFs

| Requisito não-funcional | Camada(s) que evidenciam |
|---|---|
| Lançamentos disponível mesmo com Consolidado fora | **Resiliência** (consumidor derrubado + fila bufferizando o backlog) |
| 50 req/s no Consolidado com ≤ 5% de perda | **Carga** (NBomber injetando 50 req/s e falhando se a perda passar de 5%); **unidade** do cache-aside no caminho quente |
| Integridade do saldo (nada se perde, nada se duplica) | **Resiliência** (500 sem perda + idempotência sob reentrega); **integração** (idempotência no consumer); **unidade** (regra de soma e idempotência) |
| Confiança de que os testes acima de fato verificam comportamento | **Mutação** sobre o domínio e a aplicação |

## Rodando tudo

```bash
# Unidade + integração + resiliência (integração/resiliência skipam sem Docker)
dotnet test FluxoCaixa.sln

# Mutação (precisa de `dotnet tool restore` antes)
cd src/Lancamentos/Lancamentos.Domain && dotnet stryker

# Carga (precisa da stack no ar)
dotnet run --project tests/Performance/LoadTests -c Release
```
