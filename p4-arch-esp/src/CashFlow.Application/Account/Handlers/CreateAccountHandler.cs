using CashFlow.Application.Common.Handlers;
using ErrorOr;

namespace CashFlow.Application.Account.Handlers
{
    public record CreateAccountRequest(string Name);

    public record CreatedAccountResponse(Guid Id);

    public interface ICreateAccountHandler : IHandler
    {
        Task<ErrorOr<CreatedAccountResponse>> HandleAsync(
            Guid userId,
            CreateAccountRequest request,
            CancellationToken cancellationToken);
    }

    public class CreateAccountHandler : ICreateAccountHandler
    {
        private readonly IAccountCachedRepository _accountRepository;
        public CreateAccountHandler(IAccountCachedRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<ErrorOr<CreatedAccountResponse>> HandleAsync(Guid userId, CreateAccountRequest request, CancellationToken cancellationToken)
        {
            var mapper = new AccountMapper();
            var entity = mapper.ToAccountEntity(request);

            entity.IdentityUserId = userId;

            await _accountRepository.CreateAccountAsync(entity, cancellationToken);

            return new CreatedAccountResponse(entity.Id);
        }
    }
}
