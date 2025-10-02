using Cashflow.Management.Application.Shared;
using Cashflow.Management.Data.AppContext;
using Cashflow.Management.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Application.Requests.ConsolidateTransaction
{
    public class ConsolidateTransactionHandler(AppDbContext appDbContext) : IRequestHandler<ConsolidateTransactionRequest, Unit>
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<Unit> Handle(ConsolidateTransactionRequest request, CancellationToken cancellationToken)
        {
            var transactionExists = await _appDbContext.ConsolidatedTransactionHistories.Where(c => c.TransactionId == request.TransactionId).FirstOrDefaultAsync(cancellationToken);

            if (transactionExists != null)
                throw new BusinessException("Transação já consolidada!");

            var cashStatement = await _appDbContext.CashStatements.Where(c => c.OpeningDate.Date == request.Date.Date).FirstOrDefaultAsync(cancellationToken);

            if (cashStatement == null)
            {
                cashStatement = new CashStatement();
                cashStatement.OpenCash(request.Date);

                cashStatement.AddTransaction(request.Type, request.Amount, request.Date);
                await _appDbContext.CashStatements.AddAsync(cashStatement, cancellationToken);
            }
            else
            {
                cashStatement.AddTransaction(request.Type, request.Amount, request.Date);
                _appDbContext.CashStatements.Update(cashStatement);
            }

            var consolidatedTransactionHistory = new ConsolidatedTransactionHistory()
                .AddConsolidatedTransactionHistory(request.TransactionId, request.Date, cashStatement.Id);

            await _appDbContext.ConsolidatedTransactionHistories.AddAsync(consolidatedTransactionHistory, cancellationToken);

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
