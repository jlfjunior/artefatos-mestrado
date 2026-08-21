# Teste de carga — fluxo de caixa (escrita + leitura)

Valida o comportamento do sistema sob carga mista, alinhado aos requisitos não funcionais do produto:

| Cenário     | RNF / objetivo                                      | Operação                                                      |
| ----------- | --------------------------------------------------- | ------------------------------------------------------------- |
| **Escrita** | Desempenho do caminho crítico de lançamentos        | `POST /api/transactions` (persistência + publicação RabbitMQ) |
| **Leitura** | Até **50 req/s** no consolidado, **≤ 5%** de falhas | `GET /api/balances/today` (ou data fixa)                      |

Os dois cenários rodam **em paralelo** para simular uso real: registros de lançamentos enquanto o consolidado é consultado.

## Pré-requisitos

1. Ambiente em execução: `docker compose up --build`
2. [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) instalado
3. Consumer `CashFlow.Consumer.Consolidation` ativo (materializa `daily_balances` após cada lançamento)

## Executar

```bash
k6 run docs/load-test/k6-cashflow.js
```

Variáveis opcionais:

| Variável       | Padrão                  | Descrição                                                    |
| -------------- | ----------------------- | ------------------------------------------------------------ |
| `API_BASE_URL` | `http://localhost:8081` | URL base da API                                              |
| `USER_ID`      | UUID do cliente seed    | Cliente titular dos lançamentos e consultas                  |
| `WRITE_RATE`   | `15`                    | Requisições de escrita por segundo                           |
| `READ_RATE`    | `50`                    | Requisições de leitura por segundo (RNF)                     |
| `DURATION`     | `30s`                   | Duração de cada cenário                                      |
| `BALANCE_DATE` | *(vazio = hoje UTC)*    | Data fixa para `GET /api/balances/{date}` em vez de `/today` |

Exemplo com taxas customizadas:

```bash
k6 run \
  -e API_BASE_URL=http://localhost:8081 \
  -e WRITE_RATE=20 \
  -e READ_RATE=50 \
  -e DURATION=1m \
  docs/load-test/k6-cashflow.js
```

## O que o script faz

1. **Setup**
   - Obtém token OAuth2 (`grant_type=password`) com o funcionário seed
   - Cria **5 lançamentos** iniciais para aquecer banco, fila e consumer antes da carga principal

2. **Cenário `transaction_writes`**
   - Taxa constante (padrão 15 req/s) de `POST /api/transactions`
   - Exercita API, transação PostgreSQL, publicação na exchange `cashflow.transaction.created.event` e processamento assíncrono

3. **Cenário `balance_reads`**
   - Taxa constante de **50 req/s** de `GET /api/balances/today?userId=...`
   - Exercita leitura da projeção `daily_balances` (consolidado)

4. **Thresholds k6**
   - Falhas globais: `http_req_failed < 5%`
   - Leitura: `p(95) < 500 ms`
   - Escrita: `p(95) < 1000 ms` (caminho mais pesado: transação + mensageria)

## Relatório de capacidade

Resultado de execução local em **16/06/2026**, com stack completa via `docker compose up --build` (API, consumer de consolidação, PostgreSQL 16 e RabbitMQ 3).

### Cenário validado

| Parâmetro        | Valor                                      |
| ---------------- | ------------------------------------------ |
| Duração          | 30 s por cenário (paralelos)               |
| Taxa de escrita  | 15 req/s — `POST /api/transactions`        |
| Taxa de leitura  | 50 req/s — `GET /api/balances/today`       |
| Carga simultânea | **65 req/s** agregados (escrita + leitura) |
| Autenticação     | OAuth2 password grant (funcionário seed)   |
| Warm-up          | 5 lançamentos no `setup` antes da carga    |

### Resultados

| Indicador        | Escrita            | Leitura       | Global                   |
| ---------------- | ------------------ | ------------- | ------------------------ |
| Requisições HTTP | ~450               | ~1 501        | **1 958**                |
| Taxa de falha    | 0%                 | 0%            | **0%** (limite: &lt; 5%) |
| Latência média   | 10,5 ms            | 5,8 ms        | 7,2 ms                   |
| Latência p(95)   | **17,8 ms**        | **9,5 ms**    | 13,5 ms                  |
| Latência máxima  | 84,8 ms            | 90,5 ms       | 324,0 ms                 |
| Threshold p(95)  | &lt; 1 000 ms ✓    | &lt; 500 ms ✓ | —                        |
| Checks k6        | 100% (201 Created) | 100% (200 OK) | 100%                     |

**Conclusão:** a API suportou o cenário de referência do produto — **50 leituras/s no consolidado** com **15 escritas/s em paralelo**, sem erros HTTP e com latências p(95) **bem abaixo** dos limites definidos (ordem de **10–18 ms** frente a 500–1 000 ms).

### O que isso demonstra

- **Caminho de escrita** (transação PostgreSQL + outbox/publicação RabbitMQ) permaneceu estável sob carga contínua.
- **Caminho de leitura** (projeção `daily_balances`) respondeu com baixa latência mesmo com escritas concorrentes.
- O desenho assíncrono (API + consumer) não impediu a consulta do consolidado durante o pico simulado.

### Escopo e ressalvas

- Medição em **ambiente local** (Docker Desktop); produção com réplicas, cache e hardware dedicado pode divergir.
- Um único cliente (`USER_ID` seed) concentra os lançamentos; cenários multi-tenant exigem teste adicional.
- O relatório valida **disponibilidade e latência HTTP** da API; não mede diretamente o lag do consumer nem escala horizontal.
- Para reproduzir: `k6 run docs/load-test/k6-cashflow.js` (detalhes e troubleshooting abaixo).

## Interpretação

### Leitura rápida do relatório

| Métrica                        | Significado                                                       |
| ------------------------------ | ----------------------------------------------------------------- |
| `token acquired` ✓             | Login OAuth no setup funcionou **e** o JWT foi extraído do corpo  |
| `write status is 201`          | `POST /api/transactions` criou lançamento                         |
| `read status is 200`           | `GET /api/balances/today` retornou consolidado                    |
| `http_req_failed > 5%`         | Muitas requisições com status ≥ 400 ou erro de rede               |
| `p(95)` baixo com falhas altas | Respostas rápidas de **401/403** (auth), não lentidão do servidor |

- **Passou:** escrita e leitura suportaram o pico simulado dentro dos limites.
- **Escrita degradada:** investigar pool de conexões, latência do RabbitMQ ou contenção no `UnitOfWork`.
- **Leitura degradada:** investigar índices em `daily_balances`, consumer atrasado (eventual consistency) ou CPU do PostgreSQL.
- **Falhas 5xx na escrita com leitura ok:** esperado em parte do desenho — lançamentos devem permanecer disponíveis mesmo com instabilidade no consolidado; compare taxas por cenário no relatório k6.

## Troubleshooting

| Sintoma                                                 | Causa provável                                      | O que fazer                                                                    |
| ------------------------------------------------------- | --------------------------------------------------- | ------------------------------------------------------------------------------ |
| `token acquired` ✓, mas **100%** de falha em read/write | Token não extraído do JSON (`access_token` ausente) | Confirme `docker compose up` e reconstrua a API após mudanças no contrato JSON |
| Falha já no setup (`token acquired` ✗)                  | API parada ou URL errada                            | `curl http://localhost:8081/health` deve retornar `Healthy`                    |
| Escrita 201, leitura 200, mas `p(95)` alto              | Consumer atrasado ou banco sob carga                | Verifique logs de `consumer-consolidation` e índices em `daily_balances`       |
| Muitos 403                                              | Usuário sem escopo ou `USER_ID` inválido            | Use o UUID do cliente seed ou defina `USER_ID` com um cliente existente        |

Teste manual rápido (PowerShell, com API no ar):

```powershell
# 1) Health
curl.exe http://localhost:8081/health

# 2) Depois do k6, se ainda falhar, rode com uma iteração e verbose:
k6 run --vus 1 --iterations 1 docs/load-test/k6-cashflow.js
```

## Limitações conhecidas

- Ambiente local (Docker Desktop); resultados em produção variam com réplicas, cache e hardware.
- O consolidado pode ficar momentaneamente defasado em relação aos lançamentos durante o pico de escrita (consistência eventual).
- Não substitui teste de caos nem escala horizontal — ver [ambiente AWS](../ambiente-aws-kubernetes.md) e [C4](../c4-definicoes-fluxo-caixa.md).

## Script legado

O arquivo `k6-balances.js` foi substituído por `k6-cashflow.js` (somente leitura não representa o desempenho completo do sistema).
