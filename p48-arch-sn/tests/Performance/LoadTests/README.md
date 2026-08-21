# Teste de carga (NBomber)

Reproduz o requisito não-funcional do Consolidado: **50 req/s** no endpoint
`GET /consolidado/{data}` por 60s sustentados, com **no máximo 5% de perda**.

O cenário autentica em `POST /token` (usuário `gerente` por padrão), usa o
Bearer e injeta a carga com um modelo de chegada aberto
(`Simulation.Inject(rate: 50, interval: 1s, during: 60s)`). Ao final, imprime a
taxa de sucesso e os percentis p50/p95/p99, gera relatórios (HTML/MD/TXT) em
`relatorios-carga/` e **encerra com exit code ≠ 0 se a perda passar de 5%** —
para poder quebrar um pipeline.

Não faz parte do `dotnet test`: é um executável que precisa das APIs e da infra
no ar.

## Como rodar

```bash
# 1) suba a stack (em outro terminal)
docker compose up --build

# 2) rode a carga
dotnet run --project tests/Performance/LoadTests -c Release
```

## Configuração por variável de ambiente

Todos têm defaults que batem com o `docker-compose` do repositório:

| Variável | Default | Para que serve |
|---|---|---|
| `LOADTEST_LANCAMENTOS_URL` | `http://localhost:5080` | base do serviço que emite o token |
| `LOADTEST_CONSOLIDADO_URL` | `http://localhost:5090` | base do serviço consultado |
| `LOADTEST_USUARIO` | `gerente` | usuário do login (`gerente` ou `admin`) |
| `LOADTEST_SENHA` | `Senha@123` | senha do login |
| `LOADTEST_DATA` | dia atual (UTC) | data consultada no `/consolidado/{data}` |
| `LOADTEST_RATE` | `50` | requisições por segundo |
| `LOADTEST_DURACAO_SEG` | `60` | duração da injeção, em segundos |
| `LOADTEST_PERDA_MAX_PCT` | `5.0` | limite de perda que reprova o teste |

Exemplo apontando para outro host e exigindo perda menor:

```bash
LOADTEST_CONSOLIDADO_URL=http://staging:5090 LOADTEST_PERDA_MAX_PCT=2 \
  dotnet run --project tests/Performance/LoadTests -c Release
```
