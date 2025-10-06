using System.Collections;
using System.Net;
using CashFlow.API.Model;
using CashFlow.Application;
using CashFlow.Application.Commands;
using CashFlow.Application.Queries;

namespace CashFlow.UnitTest.API.Controllers;

internal class RetrieveCashFlowQueryTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<GetDailyBalanceQueryResponse>.CreateFail(ErrorCode.CommandInvalid, "Request inválida"),
            (int)HttpStatusCode.BadRequest
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<GetDailyBalanceQueryResponse>.CreateFail(ErrorCode.CashFlowNotFound, "Não encontrado!"),
            (int)HttpStatusCode.NotFound
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<GetDailyBalanceQueryResponse>.CreateFail(ErrorCode.InternalError, "Internal Error"),
            (int)HttpStatusCode.InternalServerError
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<GetDailyBalanceQueryResponse>.CreateSuccess(new GetDailyBalanceQueryResponse
            {
                Transactions = new List<Transaction>(),
                CurrentBalance = 100M,
                AccountId = Guid.NewGuid(),
                Date = DateOnly.FromDateTime(DateTime.UtcNow.Date)
            }),
            (int)HttpStatusCode.OK
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class CancelTransactionCommandTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<Guid>.CreateFail(ErrorCode.CommandInvalid, "Request inválida"),
            (int)HttpStatusCode.BadRequest
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<Guid>.CreateFail(ErrorCode.TransactionNotFound, "Não encontrado!"),
            (int)HttpStatusCode.NotFound
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<Guid>.CreateFail(ErrorCode.InternalError, "Internal Error"),
            (int)HttpStatusCode.InternalServerError
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            CommandResponse<Guid>.CreateSuccess(Guid.NewGuid()),
            (int)HttpStatusCode.Accepted
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class AddTransactionCommandTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            Guid.NewGuid(),
            new AddTransactionRequest
            {
                Amount = 50M,
                Type = TransactionType.Credit
            },
            CommandResponse<Guid>.CreateFail(ErrorCode.CommandInvalid, "Request inválida"),
            (int)HttpStatusCode.BadRequest
        };
        yield return new object[]
        {
            Guid.NewGuid(),
            new AddTransactionRequest
            {
                Amount = 50M,
                Type = TransactionType.Credit
            },
            CommandResponse<Guid>.CreateFail(ErrorCode.CashFlowNotFound, "Não encontrado"),
            (int)HttpStatusCode.NotFound
        };

        yield return new object[]
        {
            Guid.NewGuid(),
            new AddTransactionRequest
            {
                Amount = 50M,
                Type = TransactionType.Credit
            },
            CommandResponse<Guid>.CreateFail(ErrorCode.InternalError, "Internal Server Error"),
            (int)HttpStatusCode.InternalServerError
        };

        yield return new object[]
        {
            Guid.NewGuid(),
            new AddTransactionRequest
            {
                Amount = 50M,
                Type = TransactionType.Credit
            },
            CommandResponse<Guid>.CreateSuccess(Guid.NewGuid()),
            (int)HttpStatusCode.Created
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class RegisterNewCashFlowCommandTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new RegisterNewCashFlowRequest
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<Guid>.CreateFail(ErrorCode.CommandInvalid, "Request inválida"),
            (int)HttpStatusCode.BadRequest
        };
        yield return new object[]
        {
            new RegisterNewCashFlowRequest
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<Guid>.CreateFail(ErrorCode.InternalError, "Internal Server Error"),
            (int)HttpStatusCode.InternalServerError
        };

        yield return new object[]
        {
            new RegisterNewCashFlowRequest
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<Guid>.CreateSuccess(Guid.NewGuid()),
            (int)HttpStatusCode.Created
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class GetByAccountIdAndDateRangeQueryTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            Guid.NewGuid(),
            new GetByAccountIdAndDateRangeQuery
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<GetByAccountIdAndDateRangeQueryResponse>.CreateFail(ErrorCode.CommandInvalid,
                "Request inválida"),
            (int)HttpStatusCode.BadRequest
        };

        yield return new object[]
        {
            Guid.NewGuid(),
            new GetByAccountIdAndDateRangeQuery
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<GetByAccountIdAndDateRangeQueryResponse>.CreateFail(ErrorCode.CashFlowNotFound,
                "Request inválida"),
            (int)HttpStatusCode.NotFound
        };

        yield return new object[]
        {
            Guid.NewGuid(),
            new GetByAccountIdAndDateRangeQuery
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<GetByAccountIdAndDateRangeQueryResponse>.CreateFail(ErrorCode.CommandInvalid,
                "Request inválida"),
            (int)HttpStatusCode.BadRequest
        };

        yield return new object[]
        {
            Guid.NewGuid(),
            new GetByAccountIdAndDateRangeQuery
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<GetByAccountIdAndDateRangeQueryResponse>.CreateFail(ErrorCode.InternalError,
                "Request inválida"),
            (int)HttpStatusCode.InternalServerError
        };

        yield return new object[]
        {
            Guid.NewGuid(),
            new GetByAccountIdAndDateRangeQuery
            {
                AccountId = Guid.NewGuid()
            },
            CommandResponse<GetByAccountIdAndDateRangeQueryResponse>.CreateSuccess(
                new GetByAccountIdAndDateRangeQueryResponse()),
            (int)HttpStatusCode.OK
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}