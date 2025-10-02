# Entidade `Transaction`

Está entidade representa uma transação financeira no cashflow, como um débito ou crédito.

## Estrutura da Classe

```csharp
public class Transaction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public ETransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } = null;
}
```

## Campos

| Campo         | Tipo               | Descrição                                                                                                                                  |
| ------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `Id`          | `string`           | Identificador da transação é um `ObjectId` do MongoDB.                                                                                     |
| `Description` | `string`           | Descrição da transação, por exemplo: "Pagamento de aluguel".                                                                               |
| `Amount`      | `decimal`          | Valor da transação.                                                                                                                        |
| `Date`        | `DateTime`         | Data em que a transação ocorreu, que pode ser diferente da data atual                                                                      |
| `Category`    | `string`           | Categoria da transação, que são pré-definidas: "Vendas", "Lucros", "Pagamentos", "Folha de Pagamento".                                     |
| `Type`        | `ETransactionType` | Tipo da transação, definido por um enum (`ETransactionType`). Que são: "Expense" e "Income".                                               |
| `CreatedAt`   | `DateTime`         | Data e hora em que a transação foi criada.                                                                                                 |
| `UpdatedAt`   | `DateTime?`        | Data e hora em que foi atualizado a transação.                                                                                             |
