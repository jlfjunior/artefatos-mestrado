using System.Net;
using CashFlow.API.Controllers;
using CashFlow.API.Model;
using CashFlow.Application.Commands;
using CashFlow.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.UnitTest.API.Controllers;

public class CashFlowsControllerTests
{
    private readonly CashFlowsController _controller;
    private readonly Mock<IMediator> _mediator = new();


    public CashFlowsControllerTests()
    {
        _controller = new CashFlowsController(_mediator.Object);
    }

    [Theory]
    [ClassData(typeof(RegisterNewCashFlowCommandTestData))]
    public async Task RegisterNewCashFlow_Responses(RegisterNewCashFlowRequest request,
        CommandResponse<Guid> commandResponse, int statusCode)
    {
        // Arrange
        _mediator
            .Setup(s => s.Send(It.IsAny<RegisterNewCashFlowCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResponse);

        // Act
        var response = (ObjectResult)await _controller.RegisterNewCashFlow(request);

       

        if (response.StatusCode == (int)HttpStatusCode.Created)
        {
            var content = (RegisterNewCashFlowResponse)response.Value!;
            Assert.NotEqual(default(Guid), content.CashFlowId);
        }
        
        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(AddTransactionCommandTestData))]
    public async Task NewTransaction_Responses(Guid cashFlowId, AddTransactionRequest request,
        CommandResponse<Guid> commandResponse, int statusCode)
    {
        // Arrange
        _mediator
            .Setup(s => s.Send(It.IsAny<AddTransactionDailyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResponse);

        // Act
        var response = (ObjectResult)await _controller.NewTransaction(cashFlowId, request);

        // Assert

        if (response.StatusCode == (int)HttpStatusCode.Created)
        {
            var content = (TransactionResponse)response.Value!;
            Assert.NotEqual(default(Guid), content.TransactionId);
        }
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(CancelTransactionCommandTestData))]
    public async Task CancelReverse_Responses(Guid cashFlowId, CommandResponse<Guid> commandResponse, int statusCode)
    {
        // Arrange
        _mediator
            .Setup(s => s.Send(It.IsAny<CancelTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResponse);

        // Act
        var response = (ObjectResult)await _controller.CancelReverse(cashFlowId);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(RetrieveCashFlowQueryTestData))]
    public async Task GetDailyBalance_Responses(Guid cashFlowId,
        CommandResponse<GetDailyBalanceQueryResponse> commandResponse, int statusCode)
    {
        // Arrange
        _mediator
            .Setup(s => s.Send(It.IsAny<GetDailyBalanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResponse);

        // Act
        var response = (ObjectResult)await _controller.GetDailyBalance(cashFlowId);

        // Assert

        if (response.StatusCode == (int)HttpStatusCode.OK)
        {
            var content = (GetDailyBalanceQueryResponse)response.Value!;
            Assert.NotEqual(default(Guid), content.AccountId);
            Assert.NotEqual(default(DateOnly), content.Date);
        }
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(GetByAccountIdAndDateRangeQueryTestData))]
    public async Task GetRangeDateBalance(Guid accountId, GetByAccountIdAndDateRangeQuery query,
        CommandResponse<GetByAccountIdAndDateRangeQueryResponse> commandResponse, int statusCode)
    {
        // Arrange
        _mediator
            .Setup(s => s.Send(It.IsAny<GetByAccountIdAndDateRangeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResponse);

        // Act
        var response = (ObjectResult)await _controller.GetRangeDateBalance(accountId, query);

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
    }
}