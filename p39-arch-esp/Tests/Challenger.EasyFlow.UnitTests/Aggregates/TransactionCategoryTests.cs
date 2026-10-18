using Challenger.EasyFlow.Domain.Aggregates.TransactionAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class TransactionCategoryTests
{
    [Fact]
    public void AsList_Should_Return_All_Known_Categories()
    {
        var list = TransactionCategory.AsList().ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(TransactionCategory.Sale, list);
        Assert.Contains(TransactionCategory.Expense, list);
    }

    [Fact]
    public void FromId_Should_Return_Category_When_Id_Exists()
    {
        var category = TransactionCategory.FromId(1);

        Assert.Equal(TransactionCategory.Sale, category);
    }

    [Fact]
    public void FromId_Should_Throw_When_Id_Does_Not_Exist()
    {
        Assert.Throws<InvalidOperationException>(() => TransactionCategory.FromId(999));
    }
}