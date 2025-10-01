# Title

Responsábilidades da Api

## Status

Implementada

## Context

Foi necessário definir claramente as responsabilidades da Web API para garantir simplicidade e separação de responsabilidades entre exposição de dados e processamento de consolidação.

## Decision

A Web Api do CashFlow tem responsabilidade:
- Criar conta
- Consultar contas
- Criar transação
- Consultar transações
- Consultar saldo consolidado geral
- Consultar saldo consolidado do dia.

1) A API poderá ser escalada horizontalmente.
2) Pela utilização da estratégia de cache-aside, espera-se que o tempo de resposta das requisições seja baixo, reduzindo o uso de processamento da aplicação.
3) O processamento de consolidação não será responsabilidade da API, ficando a cargo de serviços internos e assíncronos

## Consequences
- Necessidade de configurar load balancing para suportar escalabilidade horizontal.
- Exige infraestrutura de cache distribuído para garantir consistência e bom desempenho em múltiplas instâncias.
