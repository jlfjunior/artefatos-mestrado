# Entidade `CashStatement`

Esta entidade representa o caixa diário, com as informações consolidadas como saldo, entradas, saídas e histórico das transações consolidadas.

## Estrutura da Classe

```csharp
public class CashStatement
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public bool isOpening { get; set; }
    public DateTime OpeningDate { get; set; }
    public DateTime ClosingDate { get; set; }

    public ICollection<ConsolidatedTransactionHistory> Transactions { get; set; }
}
```

## Campos

| Campo          | Tipo                                          | Descrição                                                                                    |
| -------------- | --------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `Id`           | `Guid`                                        | Identificador do caixa.                                                                      |
| `Balance`      | `decimal`                                     | Saldo do caixa, com base nos Inflows e Outflows.                                             |
| `Inflow`       | `decimal`                                     | Total de entradas (créditos).                                                                |
| `Outflow`      | `decimal`                                     | Total de saídas (débitos).                                                                   |
| `isOpening`    | `bool`                                        | Flag para se o caixa está aberto.                                                            |
| `OpeningDate`  | `DateTime`                                    | Data de abertura do caixa.                                                                   |
| `ClosingDate`  | `DateTime`                                    | Data de fechamento do caixa.                                                                 |
| `Transactions` | `ICollection<ConsolidatedTransactionHistory>` | Lista de transações consolidadas que fazem deste caixa.                                      |
