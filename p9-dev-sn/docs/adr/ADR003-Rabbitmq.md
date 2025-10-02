# ADR 003 – RabbitMQ

**Status**: Aprovado
**Data**: 28/05/2025

## Contexto

O ecossistema do Cashflow App é composto por dois microsserviços desenvolvidos em .NET. A comunicação entre esses serviços deve ser assíncrona, desacoplada e resiliente a falhas.

## Decisão

Ficou decidido utilizar o RabbitMQ como broker de eventos. Com modelo de implantação on-premises em cluster Kubernetes com operador do RabbitMQ da VMware.

## Justificativas

- **Confiabilidade**: Confirmação de entrega, persistência e dead-letter em caso de falha no consumo.
- **Desacoplamento**: Permite que os serviços evoluam sem dependência de comunicação, garantindo escalabilidade e resiliência.
- **Administração**: UI que facilita o monitoramento e gerenciamento das filas.
- **Confiabilidade**: Protocolo AMQP consolidado com bom suporte da comunidade e operadores estáveis para Kubernetes.

## Consequências

- A lógica de retry e tratamento de mensagens deve ser bem implementada para não haver perca de mensagens.

## Alternativas

- **Kafka**: Recomendado para grandes volumes de dados, mas com maior complexidade operacional e ambiente.
- **SQS**: Recurso nativo em cloud, mas que requer mais trabalho na construção de soluções de dead-letter e resiliência.

## Conclusão

Decisão validada com foco em escalabilidade, alta disponibilidade e alinhamento com a arquitetura de microsserviços do cashflow.
