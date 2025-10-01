using CashFlow.Application.Account;
using CashFlow.Application.Common.Handlers;
using ErrorOr;

namespace CashFlow.Application.AccountBalance.Handlers;

public record GetAccountBalanceResponse(Guid Id, decimal CurrentTotal);

public interface IGetBalanceByAccountIdHandler : IHandler
{
    Task<ErrorOr<GetAccountBalanceResponse>> HandleAsync(
        Guid userId,
        Guid accountId,
        CancellationToken cancellationToken);
}

public class GetBalanceByAccountIdHandler : IGetBalanceByAccountIdHandler
{
    private readonly IAccountCachedRepository _accountCachedRepository;

    public GetBalanceByAccountIdHandler(IAccountCachedRepository accountCachedRepository)
    {
        _accountCachedRepository = accountCachedRepository;
    }

    public async Task<ErrorOr<GetAccountBalanceResponse>> HandleAsync(Guid userId, Guid accountId,
        CancellationToken cancellationToken)
    {
        var accounts = await _accountCachedRepository.GetAllByUserIdAsync(userId, cancellationToken);
        if (!accounts.Any(a => a.Id == accountId))
            return Error.Unauthorized();

        var result = await _accountCachedRepository.GetBalanceAsync(accountId, cancellationToken);
        if (result is null)
            return Error.Unexpected(description: "Try again later.");

        return result;
    }
}
