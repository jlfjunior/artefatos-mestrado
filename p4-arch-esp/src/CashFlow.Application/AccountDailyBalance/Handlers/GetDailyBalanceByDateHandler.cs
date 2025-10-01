using CashFlow.Application.Account;
using CashFlow.Application.Common.Handlers;
using ErrorOr;

namespace CashFlow.Application.AccountDailyBalance.Handlers;

public record GetAccountDailyBalanceQuery(Guid UserId, Guid AccountId, DateOnly Date);

public record GetAccountDailyBalanceResponse(Guid Id, DateOnly Date,
    decimal TotalIncoming, decimal TotalOutgoing, decimal BalanceOfDay, DateTimeOffset? UpdatedAt);

public interface IGetDailyBalanceByDateHandler : IHandler
{
    Task<ErrorOr<GetAccountDailyBalanceResponse>> HandleAsync(
        GetAccountDailyBalanceQuery query,
        CancellationToken cancellationToken);
}

public class GetDailyBalanceByDateHandler : IGetDailyBalanceByDateHandler
{
    private readonly IAccountCachedRepository _accountCachedRepository;

    public GetDailyBalanceByDateHandler(IAccountCachedRepository accountCachedRepository)
    {
        _accountCachedRepository = accountCachedRepository;
    }

    public async Task<ErrorOr<GetAccountDailyBalanceResponse>> HandleAsync(
        GetAccountDailyBalanceQuery query,
        CancellationToken cancellationToken)
    {
        var accounts = await _accountCachedRepository.GetAllByUserIdAsync(query.UserId, cancellationToken);
        if (!accounts.Any(a => a.Id == query.AccountId))
            return Error.Unauthorized();

        var result = await _accountCachedRepository.GetDailyBalanceAsync(query.AccountId, query.Date, cancellationToken);
        if (result is null)
            return Error.Unexpected(description: "Try again later.");

        return result;
    }
}
