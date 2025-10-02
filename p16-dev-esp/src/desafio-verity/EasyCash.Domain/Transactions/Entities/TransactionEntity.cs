using EasyCash.Abstractions;
using EasyCash.Abstractions.Exceptions;
using EasyCash.Shared.Enums;
using EasyCash.Shared.ValueObjects;

namespace EasyCash.Domain.Transactions.Entities;

public sealed class TransactionEntity : EntityBase<Guid>
{
    public DateTimeOffset Date { get; private set; }

    public string Description { get; private set; }

    public ETransactionType Type { get; private set; }

    public Money Amount { get; private set; }

    public string Category { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public long RowVersion { get; set; } = 1;

    private TransactionEntity() { }

    public static TransactionEntity Create(
        string description,
        ETransactionType type,
        decimal amount,
        string category,
        DateTimeOffset? date = null)
    {
        var entity = new TransactionEntity()
        {
            Id = Guid.NewGuid(),
            Description = description,
            Type = type,
            Amount = Money.FromDecimal(amount),
            Category = category,
            Date = date ?? DateTimeOffset.Now,
            CreatedAt = DateTimeOffset.Now
        };

        entity.Validate();

        return entity;
    }

    public void Update(
        string description,
        ETransactionType type,
        decimal amount,
        string category,
        DateTime date)
    {
        Description = description;
        Type = type;
        Amount = Money.FromDecimal(amount);
        Category = category;
        Date = date.Date;
        UpdatedAt = DateTime.Now;

        Validate();
    }

    protected override void Validate()
    {
        var validationErros = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(Description))
            validationErros.Add(new ValidationError("Description", "Description is required"));

        if (Amount <= 0)
            validationErros.Add(new ValidationError("Amount", "Amount is required"));

        if (string.IsNullOrWhiteSpace(Category))
            validationErros.Add(new ValidationError("Category", "Category is required"));

        if (validationErros.Any())
            throw new ValidationException(validationErros);
    }
}

