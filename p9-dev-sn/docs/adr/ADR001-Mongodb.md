# ADR 001 – MongoDB

**Status**: Aprovado  
**Data**: 28/05/2025

## Contexto

O `ms-cashflow-transactions` é um projeto .NET para gestão de transações de um caixa, com foco em alta performance de escrita, flexibilidade na modelagem de dados e escalabilidade horizontal. 

## Decisão

Ficou decido utilizar o MongoDB como banco de dados para este serviço. Com modelo de implantação on-premises em cluster Kubernetes com Mongo Operator.

## Justificativas

- **Alta performance na escrita**: MongoDB é otimizado para operações de escrita, especialmente em cenários com inserções massivas como registros financeiros.
- **Modelo de dados**: Permite a evolução da estrutura de dados sem necessidade de migrações complexas, como o projeto é evolutivo está é uma grande função.
- **Escalabilidade**: O MongoDB suporta clusterização nativa, permitindo distribuição dos dados com escalabilidade horizontal.
- **Relacionamentos**: Como o modelo de dados não necessita de relacionamentos, um banco NoSQL seria a melhor recomendação, dado a flexibilidade e performance na leitura e escrita.
- **Alta disponibilidade e replicação**: Com replica set o MongoDB tem alta disponibilidade e tolerância a falhas.

## Consequências

- Backup e planejamento de índices devem ser acompanhados para garantir performance e integridade dos dados.

## Alternativas

- **PostgreSQL**: Consistente e transacional, mas com menor flexibilidade no schema e menor performance em escrita para operações massivas.
- **CosmosDB**: Ótimo por ser PaaS em cloud, mas alto custo e limitações nos recursos comparados ao MongoDB nativo.

## Conclusão

Decisão validada com foco em MVP, velocidade de entrega e escalabilidade futura.
