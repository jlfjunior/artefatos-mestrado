# CashFlowDailyConsolidation

Este repositório contém dois serviços em **.NET 8** para gerenciamento de fluxo de caixa (**CashFlow**) e consolidação diária (**DailyConsolidation**). O projeto segue uma arquitetura limpa (**Clean Architecture**), com camadas bem definidas (Domain, Application, Infrastructure) e APIs separadas para cada responsabilidade.

## Visão Geral

1. **CashFlowService.API**  
   Responsável pelo controle de lançamentos (débito e crédito). Possui autenticação via **Basic Authentication**, exigindo credenciais para acessar os endpoints.

2. **DailyConsolidationService.API**  
   Responsável pela consolidação diária dos lançamentos, retornando relatórios com totais de créditos e débitos. Também utiliza **Basic Authentication**.

Ambos compartilham a camada de **Application** (DTOs e Interfaces), **Domain** (Entidades) e **Infrastructure** (serviços, repositórios, etc.). Um **ApplicationDbContext** InMemory é usado como banco de dados para simplificar a execução local.

## Estrutura do Projeto

```
CashFlowDailyConsolidationSolution/
├── CashFlowDailyConsolidationSolution.sln
├── src/
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── ConsolidationReportRequest.cs
│   │   │   ├── CreateTransactionRequest.cs
│   │   │   └── TransactionResponse.cs
│   │   └── Interfaces/
│   │       ├── ICashFlowService.cs
│   │       └── IConsolidationService.cs
│   ├── Domain/
│   │   └── Entities/
│   │       ├── DailyBalance.cs
│   │       └── Transaction.cs
│   ├── Infrastructure/
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs
│   │   └── Services/
│   │       ├── CashFlowService.cs
│   │       └── ConsolidationService.cs
│   ├── CashFlowService.API/
│   │   ├── Authentication/
│   │   │   └── BasicAuthenticationHandler.cs
│   │   ├── Controllers/
│   │   │   └── CashFlowController.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── launchSettings.json
│   ├── DailyConsolidationService.API/
│   │   ├── Authentication/
│   │   │   └── BasicAuthenticationHandler.cs
│   │   ├── Controllers/
│   │   │   └── ConsolidationController.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── launchSettings.json
└── tests/
    ├── CashFlowService.Tests/
    │   └── CashFlowControllerTests.cs
    └── DailyConsolidationService.Tests/
        └── ConsolidationControllerTests.cs
```

## Tecnologias Utilizadas

- **.NET 8**  
- **Entity Framework Core (InMemory)**  
- **Microsoft.Extensions.Logging** (para logs)  
- **Polly** (CircuitBreaker, usado no DailyConsolidationService)  
- **Basic Authentication** (para o CashFlowService e DailyConsolidationService)  
- **Swagger** para documentação dos endpoints  
- **xUnit e Moq** para testes unitários  

## Como Executar Localmente

### 1. Clonar o repositório
git clone <URL-do-repositório> cd CashFlowDailyConsolidationSolution

### 2. Abrir a solução no Visual Studio 2022

Abra o arquivo `CashFlowDailyConsolidationSolution.sln` no Visual Studio 2022 (ou outra IDE de sua preferência).

### 3. Restaurar pacotes NuGet

O Visual Studio fará isso automaticamente ao abrir a solução, ou você pode executar manualmente:
dotnet restore


### 4. Configurar as credenciais no CashFlowService.API

No arquivo `appsettings.json` do **CashFlowService.API**, há uma seção `"BasicAuth"` com `Username` e `Password`. Ajuste conforme necessário:

```json
"BasicAuth": {
  "Username": "admin",
  "Password": "password123"
}
```

### 5. Executar cada projeto

Selecione o CashFlowService.API como projeto de inicialização e rode a aplicação.
Em seguida, selecione o DailyConsolidationService.API como projeto de inicialização (ou abra outra instância do Visual Studio) e rode a aplicação.

### 6. Acessar o Swagger

Por padrão, cada serviço expõe o Swagger em /swagger. Por exemplo:

- CashFlowService.API: https://localhost:5001/swagger
- DailyConsolidationService.API: https://localhost:7096/swagger

Ajuste as portas conforme definido em seu launchSettings.json.

### 7. Autenticação

Ao testar endpoints da API, inclua o header Authorization no formato Basic. Exemplo:
```
Authorization: Basic YWRtaW46cGFzc3dvcmQxMjM=
```
Onde YWRtaW46cGFzc3dvcmQxMjM= é a representação Base64 de admin:password123.

### 8. Executar Testes

Dentro da pasta tests/, execute:

dotnet test

Ou use o Test Explorer do Visual Studio para rodar e visualizar os resultados.

### Uso Básico
-----------

### CashFlowService.API
-------------------

POST api/cashflow/transactions
Cria um novo lançamento (débito ou crédito).
Exemplo de Payload:
```json
{
  "date": "2025-02-18",
  "amount": 100,
  "type": "Credit",
  "description": "Venda de produto"
}
```

Requer Basic Auth.

```GET api/cashflow/transactions/{date}```
Lista todos os lançamentos para uma data específica.
Exemplo: ```api/cashflow/transactions/2025-02-18```
Requer Basic Auth.

### DailyConsolidationService.API
-----------------------------

```GET api/consolidation/report/{date}```
Gera um relatório consolidado de créditos, débitos e saldo do dia.
Exemplo: ```api/consolidation/report/2025-02-18```
Requer Basic Auth.

### Evoluções Futuras
-----------------

Autenticação unificada (JWT)
Poderíamos migrar ambos os serviços para JWT, facilitando a gestão de tokens e a escalabilidade em ambientes distribuídos.

Mensageria Assíncrona
Utilizar um broker (RabbitMQ, Kafka, etc.) para desacoplar ainda mais o CashFlowService do DailyConsolidationService, garantindo que o primeiro não seja afetado por eventuais indisponibilidades do segundo.

Persistência em Banco Relacional
Substituir o InMemory por SQL Server ou PostgreSQL para produção, possibilitando histórico persistente de lançamentos e consolidações.

Cache Distribuído (Redis)
Implementar cache no DailyConsolidationService para armazenar relatórios recentes e melhorar a performance em cenários de alta carga.

Monitoramento e Observabilidade
Integrar com ferramentas como Application Insights, Prometheus, Grafana para logs, métricas e alertas em produção.

Observações Finais
------------------

Este projeto demonstra a separação de responsabilidades entre dois serviços (CashFlow e Consolidation), uso de boas práticas de Clean Architecture, testes unitários e uma forma simples de autenticação (Basic Auth). Há espaço para melhorias e adoção de padrões mais avançados (mensageria, cache, monitoramento etc.), mas esta base fornece um ponto de partida sólido para evolução em um ambiente real.




