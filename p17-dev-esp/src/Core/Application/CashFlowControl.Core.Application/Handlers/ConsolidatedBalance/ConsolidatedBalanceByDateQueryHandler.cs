using CashFlowControl.Core.Application.Exceptions;
using CashFlowControl.Core.Application.Interfaces.Repositories;
using CashFlowControl.Core.Application.Queries;
using CashFlowControl.Core.Application.Security.Helpers;
using CashFlowControl.Core.Domain.Entities;
using MediatR;

namespace CashFlowControl.Core.Application.Handlers
{
    public class ConsolidatedBalanceByDateQueryHandler : IRequestHandler<ConsolidatedBalanceByDateQuery, Result<ConsolidatedBalance?>>
    {
        private readonly IConsolidatedBalanceRepository _balanceRepository;

        public ConsolidatedBalanceByDateQueryHandler(IConsolidatedBalanceRepository balanceRepository)
        {
            _balanceRepository = balanceRepository;
        }

        public async Task<Result<ConsolidatedBalance?>> Handle(ConsolidatedBalanceByDateQuery request, CancellationToken cancellationToken)
        {
            if (!request.IsValid)
            {
                throw new CommandValidationException(request.Notifications);
            }

            try
            {
                var retBalance = await _balanceRepository.GetBalanceByDateAsync(request.Date);
                return Result<ConsolidatedBalance?>.Success(retBalance);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(Result<ConsolidatedBalance?>.ValidationFailure(ex.Message));
            }
        }
    }
}