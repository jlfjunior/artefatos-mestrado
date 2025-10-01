using CashFlow.Application.Common.Handlers;
using CashFlow.Application.Common.Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Application.Account.Handlers;

public record GetAccountResponse(Guid Id, string Name);

public interface IGetAccountByIdHandler : IHandler
{
    Task<ErrorOr<GetAccountResponse>> HandleAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);
}

public class GetAccountByIdHandler : IGetAccountByIdHandler
{
    private readonly ICashFlowDbContext _cashFlowDbContext;

    public GetAccountByIdHandler(ICashFlowDbContext cashFlowDbContext)
    {
        _cashFlowDbContext = cashFlowDbContext;
    }

    public async Task<ErrorOr<GetAccountResponse>> HandleAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var account =
            await _cashFlowDbContext.Accounts
                .FirstOrDefaultAsync(s => s.IdentityUserId == userId && s.Id == id, cancellationToken);

        if (account is null)
            return Error.NotFound(description: "Account not found.");

        var mapper = new AccountMapper();
        var response = mapper.ToGetAccountResponse(account);

        return response;
    }
}
