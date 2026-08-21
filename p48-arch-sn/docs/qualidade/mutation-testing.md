# Testes de mutação com Stryker.NET

Cobertura de linha mente. Um suite pode tocar 100% das linhas e ainda assim não
verificar nada — basta que os asserts sejam fracos ou ausentes. Foi por isso que
adicionei testes de mutação ao projeto: eles medem o quanto os meus testes
realmente _detectam_ defeitos, não só o quanto eles _passam_ pelo código.

## O que é mutação, na prática

O Stryker pega o código de produção e injeta pequenas alterações — os
_mutantes_. Troca um `>` por `>=`, um `+` por `-`, um `&&` por `||`, remove uma
chamada, inverte um `return`. Para cada mutante, ele roda os meus testes:

- se algum teste **falha**, o mutante foi **morto** (_killed_) — ótimo, meus
  testes perceberam a mudança de comportamento;
- se **todos passam**, o mutante **sobreviveu** (_survived_) — sinal de que
  aquela linha não está sendo verificada de verdade.

O **mutation score** é a fração de mutantes mortos sobre o total de mutantes
viáveis. Um score alto quer dizer que, se um bug parecido com aqueles mutantes
entrar no código, meus testes vão pegá-lo.

## Por que aqui

O domínio deste projeto é pequeno, mas as regras que existem são as que
sustentam o requisito não-funcional: a soma do saldo, a idempotência por
`LancamentoId`, a validação dos valores. São exatamente os pontos onde um
mutante sobrevivente revelaria um teste frouxo. Rodar mutação nessas camadas me
dá confiança de que a malha de testes de unidade está apertada onde importa.

Foco a mutação nos quatro projetos com regra de negócio:

- `Lancamentos.Domain` e `Lancamentos.Application`
- `Consolidado.Domain` e `Consolidado.Application`

Deixo de fora a Infrastructure e as APIs de propósito: ali o valor vem dos
testes de integração (com Postgres, RabbitMQ e Redis reais), não de mutar
adapters. O Stryker brilha sobre lógica pura, que roda rápido e sem Docker.

Cada um desses projetos é exercitado pelo seu projeto de testes de **unidade**:

| Projeto-alvo                | Testes que o exercitam              |
|-----------------------------|-------------------------------------|
| `Lancamentos.Domain`        | `tests/Lancamentos.UnitTests`       |
| `Lancamentos.Application`   | `tests/Lancamentos.Application.UnitTests` |
| `Consolidado.Domain`        | `tests/Consolidado.UnitTests`       |
| `Consolidado.Application`   | `tests/Consolidado.Application.UnitTests` |

## Como rodar

O Stryker está fixado como ferramenta **local** no manifesto
`.config/dotnet-tools.json`. Primeiro restaure a ferramenta (uma vez por clone):

```bash
dotnet tool restore
```

Depois rode o Stryker a partir da pasta do projeto que quer mutar — cada um tem
o seu `stryker-config.json` apontando para o projeto-alvo e o projeto de teste:

```bash
# Domínio de Lançamentos
cd src/Lancamentos/Lancamentos.Domain && dotnet stryker

# Aplicação de Lançamentos
cd src/Lancamentos/Lancamentos.Application && dotnet stryker

# Domínio de Consolidado
cd src/Consolidado/Consolidado.Domain && dotnet stryker

# Aplicação de Consolidado
cd src/Consolidado/Consolidado.Application && dotnet stryker
```

Cada execução gera um relatório HTML em `StrykerOutput/<timestamp>/reports/` na
pasta do projeto (ignorado pelo Git). Abra o `mutation-report.html` no navegador
para navegar mutante a mutante e ver exatamente quais sobreviveram e onde.

## Como ler o score

Os thresholds estão configurados assim em cada `stryker-config.json`:

- **high: 80** — acima disso, o projeto está em bom estado (verde no relatório).
- **low: 60** — entre 60 e 80 é zona de atenção (amarelo).
- **break: 50** — abaixo de 50 o Stryker retorna **exit code diferente de zero**,
  o que derruba um pipeline de CI. É o piso que não quero furar.

Quando um mutante sobrevive, a leitura é: "existe um comportamento que eu posso
quebrar sem nenhum teste reclamar". A correção quase nunca é caçar o número do
score — é olhar o mutante sobrevivente e escrever (ou reforçar) o assert que o
mataria. Vale também o bom senso: alguns mutantes são equivalentes (não mudam o
comportamento observável) e não dá para matá-los; o relatório ajuda a
distinguir esses casos de um teste realmente faltando.

Como referência, na primeira execução o `Lancamentos.Domain` fechou em torno de
**91%** — bem acima do limiar `high`, o que bate com o cuidado que tomei nos
testes de unidade do domínio.
