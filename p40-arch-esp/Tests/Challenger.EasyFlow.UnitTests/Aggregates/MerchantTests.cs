using Challenger.EasyFlow.Domain.Aggregates.MerchantAggregate;

namespace Challenger.EasyFlow.UnitTests.Aggregates;

public class MerchantTests
{
    [Fact]
    public void Should_Create_Merchant_With_Expected_Values()
    {
        var sut = new Merchant("Store A", "BRL", "America/Sao_Paulo");

        Assert.NotEqual(Guid.Empty, sut.Id);
        Assert.Equal("Store A", sut.Name);
        Assert.Equal("BRL", sut.Currency);
        Assert.Equal("America/Sao_Paulo", sut.Timezone);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Throw_When_Name_Is_Null_Or_Empty(string? name)
    {
        var sut = new Merchant("Store A", "BRL", "America/Sao_Paulo");

        Assert.ThrowsAny<ArgumentException>(() => sut.Name = name!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Throw_When_Currency_Is_Null_Or_Empty(string? currency)
    {
        var sut = new Merchant("Store A", "BRL", "America/Sao_Paulo");

        Assert.ThrowsAny<ArgumentException>(() => sut.Currency = currency!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Throw_When_Timezone_Is_Null_Or_Empty(string? timezone)
    {
        var sut = new Merchant("Store A", "BRL", "America/Sao_Paulo");

        Assert.ThrowsAny<ArgumentException>(() => sut.Timezone = timezone!);
    }
}