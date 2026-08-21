# C4 — Diagrama de Containers

Deploy units e integrações entre os componentes do sistema.

```mermaid
flowchart TB
    Merchant["Comerciante"]

    subgraph System["Sistema de Fluxo de Caixa"]
        Web["Web App<br/><b>Blazor Server</b><br/>:8080"]
        API["CashFlow API<br/><b>ASP.NET Core</b><br/>:8081"]
        Consumer["Consumer.Consolidation<br/><b>.NET Worker</b>"]
        DB[("PostgreSQL<br/>transactions<br/>daily_balances")]
        MQ[("RabbitMQ<br/>Event Message")]
    end

    Merchant -->|"HTTPS"| Web
    Web -->|"REST + OAuth2"| API
    API -->|"INSERT / SELECT"| DB
    API -->|"publish<br/>cashflow.transaction.created.event"| MQ
    MQ -->|"consume<br/>cashflow.consolidation"| Consumer
    Consumer -->|"UPSERT daily_balances"| DB
    API -->|"SELECT daily_balances"| DB
```

## Responsabilidades

| Container | Papel |
|-----------|-------|
| **Web App** | UI do comerciante; login OAuth; formulários de lançamento e consulta de saldo |
| **CashFlow API** | REST de lançamentos e saldos; emissão de tokens; publicação de eventos após persistência |
| **Consumer.Consolidation** | Projeção assíncrona do consolidado diário a partir dos eventos |
| **PostgreSQL** | Fonte de verdade dos lançamentos e projeção de saldos |
| **RabbitMQ** | Desacopla escrita de lançamentos da materialização do consolidado |

Definições detalhadas: [c4-definicoes-fluxo-caixa.md](../c4-definicoes-fluxo-caixa.md#nível-2-container-diagram---definições).
