# ADR 0002 — CQRS com read model dedicado

Status: aceito

## Contexto
As duas operações do sistema têm necessidades opostas. Registrar lançamento é
escrita transacional, fonte da verdade, baixa frequência. Consultar o saldo do
dia é leitura, alta frequência (50 req/s no pico) e se beneficia de cache.
Modelar as duas em cima da mesma estrutura otimizaria mal os dois lados.

## Decisão
Separar leitura de escrita (CQRS):

- **Lançamentos** é o write model. Guarda cada lançamento individual.
- **Consolidado** é o read model. Mantém uma projeção do saldo já somado por
  dia (`saldos_diarios`), atualizada de forma incremental conforme os eventos
  chegam.

A consulta nunca varre a tabela de lançamentos para somar na hora; ela lê a
projeção pronta, com cache na frente.

## Consequências
- Leitura barata e cacheável: a consulta do dia é um lookup por chave.
- A projeção é eventualmente consistente em relação à escrita — coerente com o
  desenho assíncrono do ADR 0001.
- Duplicação controlada de dados (o saldo é derivável dos lançamentos). É o
  trade-off clássico de CQRS e está alinhado com a meta de performance da
  leitura.
