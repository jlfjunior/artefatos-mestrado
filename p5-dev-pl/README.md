# Sistema de Gestão de Fluxo de Caixa

Este é um sistema baseado em microsserviços .NET 8 para gerenciar lançamentos de fluxo de caixa e gerar consolidações diárias.

## Explicando a proposta

<img width="1405" height="806" alt="image" src="https://github.com/user-attachments/assets/3c1eea9d-4fef-4f4e-a3cb-09ae454faec5" />

Foram criados dois microsserviços principais para atender às necessidades de gestão de fluxo de caixa:

1. **Lançamentos**: Permite o registro de entradas e saídas financeiras.
1. **Consolidação Diária**: Consolida os lançamentos diários, permitindo a visualização do saldo diário.

O motivo da decisão é cada serviço ser indenpendente, permitindo escalabilidade e manutenção mais simples. A separação entre lançamentos e consolidação diária facilita a implementação de regras de negócio específicas e a reutilização de componentes.

Sendo assim, baseado no diagrama acima, temos a aplicação de lançamentos que registra os lançamentos em um banco de dados relacional, devido aos dados estruturados, e essa base de dados deve ser configurada para ter transactional replication para outra base somente de leitura.
O serviço de consolidação diária consome os dados da base de dados de leitura, assim que recebe um request com uma data específica, é realizada uma consulta se existe cache para aquela data, caso exista, retorna o cache, caso não exista, realiza a consulta no banco de dados e armazena o resultado no cache para futuras consultas.

## Implementações futuras

- Implementar gateway de API para unificar os endpoints dos microsserviços, facilitando o acesso, segurança e monitoramento.
- Implementar políticas de retry e circuit breaker para melhorar a resiliência dos serviços, por exemplo, utilizando Polly.
- Implementar autenticação e autorização, garantindo que apenas usuários autorizados possam acessar os serviços.
- Implementar monitoramento e logging centralizado, utilizando ferramentas como ELK Stack.

## Arquitetura

A solução segue os princípios da Clean Architecture e está dividida em dois microsserviços principais:

### 1. Serviço de Lançamentos (Entries)
Responsável por gerenciar os lançamentos individuais do fluxo de caixa (créditos e débitos).

Estrutura:
- **CashFlow.Entries.API**: Endpoints REST
- **CashFlow.Entries.Application**: Lógica de negócio e serviços
- **CashFlow.Entries.Domain**: Entidades, interfaces e regras de negócio
- **CashFlow.Entries.Infrastructure**: Implementação de acesso a dados e serviços externos
- **CashFlow.Entries.Test**: Testes unitários

### 2. Serviço de Consolidação Diária (Daily Consolidated)
Responsável pela consolidação dos lançamentos diários do fluxo de caixa.

Estrutura:
- **CashFlow.DailyConsolidated.API**: Endpoints REST
- **CashFlow.DailyConsolidated.Application**: Lógica de negócio e serviços
- **CashFlow.DailyConsolidated.Domain**: Entidades, interfaces e regras de negócio
- **CashFlow.DailyConsolidated.Infrastructure**: Implementação de acesso a dados e serviços externos
- **CashFlow.DailyConsolidated.Test**: Testes unitários

## Tecnologias Utilizadas

- **.NET 8**: Framework principal
- **Redis**: Utilizado para cache e armazenamento de dados
- **Docker**: Containerização dos serviços
- **Swagger/OpenAPI**: Documentação e testes das APIs
- **Clean Architecture**: Padrão arquitetural
- **Testes Unitários (xUnit)**: Garantia de qualidade e confiabilidade

## Como Rodar Localmente

### Pré-requisitos

- Docker e Docker Compose
- .NET 8 SDK (para desenvolvimento)

### Passos para Execução

1. Clone o repositório:
```bash
git clone [url-do-repositorio]
cd CashFlow
```

2. Execute com Docker Compose:
```bash
docker-compose up -d
```

Isso irá iniciar:
- API de Lançamentos em http://localhost:5001
- API de Consolidação Diária em http://localhost:5003
- Redis na porta 6379

### Acesso às APIs

Após iniciar, acesse:
- Swagger da API de Lançamentos: http://localhost:5001/swagger
- Swagger da API de Consolidação Diária: http://localhost:5003/swagger

## Estrutura do Projeto

```
CashFlow/
├── Entries/                          # Microsserviço de Lançamentos
│   ├── CashFlow.Entries.API/
│   ├── CashFlow.Entries.Application/
│   ├── CashFlow.Entries.Domain/
│   ├── CashFlow.Entries.Infrastructure/
│   └── CashFlow.Entries.Test/
│
├── DailyConsolidated/               # Microsserviço de Consolidação Diária
│   ├── CashFlow.DailyConsolidated.API/
│   ├── CashFlow.DailyConsolidated.Application/
│   ├── CashFlow.DailyConsolidated.Domain/
│   ├── CashFlow.DailyConsolidated.Infrastructure/
│   └── CashFlow.DailyConsolidated.Test/
│
└── docker-compose.yml
```
