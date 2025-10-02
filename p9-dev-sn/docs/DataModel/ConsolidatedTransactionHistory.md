# Entidade `ConsolidatedTransactionHistory`

Esta entidade representa o registro histórico das transações que foram consolidadas no caixa, com relacionamento ao caixa.

## Estrutura da Classe

```csharp
public class ConsolidatedTransactionHistory
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; }
    public DateTime Date { get; set; }

    public Guid CashStatementId { get; set; }
}
```

## Campos

| Campo             | Tipo                                          | Descrição                                                                                    |
| ------------------| --------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `Id`              | `Guid`                                        | Identificador do caixa.                                                                      |
| `TransactionId`   | `string`                                      | Id da transação recebida do serviço de transactions.                                         |
| `Date`            | `DateTime`                                    | Data da transação.                                                                           |
| `CashStatementId` | `Guid`                                        | Id para relacionamento com o caixa.                                                          |
