using Domain.Entities;

namespace DailySummary.Tests.Domain;

[TestFixture]
public class DailySummaryTests
{
    [Test]
    public void CreateDailySummary_ValidData_ShouldCreateSummary()
    {
        // Arrange
        DateTime date = new(2024, 2, 20);
        decimal totalCredits = 500.00m;
        decimal totalDebits = 200.00m;

        // Act
        var summary = DailySummaryEntity.Create(date, totalCredits, totalDebits);

        // Assert
        Assert.That(summary, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(summary.Date, Is.EqualTo(date));
            Assert.That(summary.TotalCredits, Is.EqualTo(totalCredits));
            Assert.That(summary.TotalDebits, Is.EqualTo(totalDebits));
            Assert.That(summary.Balance, Is.EqualTo(totalCredits - totalDebits));
            Assert.That(summary.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public void UpdateDailySummary_ValidData_ShouldUpdateSummary()
    {
        // Arrange
        var summary = DailySummaryEntity.Create(new DateTime(2024, 2, 20), 500.00m, 200.00m);
        decimal newTotalCredits = 700.00m;
        decimal newTotalDebits = 300.00m;

        // Act
        summary.Update(newTotalCredits, newTotalDebits);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(summary.TotalCredits, Is.EqualTo(newTotalCredits));
            Assert.That(summary.TotalDebits, Is.EqualTo(newTotalDebits));
            Assert.That(summary.Balance, Is.EqualTo(newTotalCredits - newTotalDebits));
        });
    }
}