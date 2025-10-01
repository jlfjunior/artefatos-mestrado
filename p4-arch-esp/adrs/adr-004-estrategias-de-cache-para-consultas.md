# Title

Uso da estratégias de cache para otimizar consultas

## Status

Implementada

## Context

O sistema precisa garantir boa performance nas consultas, principalmente em endpoints de saldo e saldo diário, que podem ter alto volume de acesso.
Sem cache, a API dependeria apenas do banco de dados, aumentando a latência e o consumo de recursos.

## Decision

A aplicação consulta primeiro o cache.
Caso o dado não esteja disponível (cache miss), busca no banco de dados e grava no cache.
Atualizações de dados no banco de dados sempre atualizam o cache correspondente para manter a consistência.

Prós:
-Reduz a carga no banco de dados.
-Melhora o tempo de resposta para consultas frequentes.
-Garantia de que os dados no cache estão sempre atualizados.

## Consequences
- A aplicação precisa lidar com a lógica de leitura e escrita no cache.
- Maior complexidade em cenários de consistência forte, se houver múltiplos serviços concorrentes escrevendo os mesmos dados.
