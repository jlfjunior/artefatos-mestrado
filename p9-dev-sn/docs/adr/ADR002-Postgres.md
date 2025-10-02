# ADR 002 – PostgreSQL

**Status**: Aprovado
**Data**: 28/05/2025

## Contexto

O `ms-cashflow-management` é um projeto .NET para gestão de caixa, com foco em consistência, e relacionamento entre entidades.

## Decisão

Ficou decidido utilizar o PostgreSQL como banco de dados para este serviço. Com modelo de implantação on-premises em cluster Kubernetes com Postgres Operator da CrunchyData.

## Justificativas

- **Consistência**: O PostgreSQL tem suporte completo a transações ACID, garantindo integridade dos dados mesmo em operações volumosas e concorrentes.
- **Modelo de dados**: Ideal para estruturas com múltiplas entidades, sendo um recomendado para cenários com relacionamentos.
- **Confiabilidade**: Banco de dados robusto, amplamente adotado e com comunidade ativa.
- **Integração com ORM**: Compatibilidade com EntityFramework e ferramentas de dados do .NET.

## Consequências

- Controle e versione a modelagem de dados com migrations versionadas.
- Para gestão de desempenho o ambiente deve ser monitorado, principalmente os índices e querys.

## Alternativas

- **MongoDB**: Mais flexível em termos de schema e melhor desempenho em escrita massiva, mas sem a mesma garantia de consistência transacional.
- **SqlServer**: Similar ao PostgreSQL em muitos aspectos, mas com custo de licenciamentos elevados para o tamanho do projeto.

## Conclusão

Decisão validada com foco em integridade de dados, suporte a relacionamento de entidades e consistência transacional.
