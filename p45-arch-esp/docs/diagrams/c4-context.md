# C4 — Diagrama de Contexto

Visão de alto nível: quem usa o sistema e qual problema ele resolve.

```mermaid
flowchart LR
    Merchant["Comerciante<br/><i>Pessoa física ou jurídica</i>"]

    subgraph CashFlow["Sistema de Fluxo de Caixa"]
        direction TB
        Purpose["Controlar lançamentos diários<br/>e consultar saldo consolidado"]
    end

    Merchant -->|"Registra débitos/créditos<br/>Consulta relatório diário"| CashFlow
```

## Elementos

| Elemento                      | Descrição                                                                         |
| ----------------------------- | --------------------------------------------------------------------------------- |
| **Comerciante**               | Usuário final que opera o caixa via interface web                                 |
| **Sistema de Fluxo de Caixa** | Plataforma que persiste lançamentos, publica eventos e materializa saldos diários |

Definições detalhadas: [c4-definicoes-fluxo-caixa.md](../c4-definicoes-fluxo-caixa.md#nível-1-context-diagram---definições).
