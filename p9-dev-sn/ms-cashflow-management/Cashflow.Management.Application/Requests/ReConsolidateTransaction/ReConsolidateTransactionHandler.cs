using Cashflow.Management.Data.AppContext;
using Cashflow.Management.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Application.Requests.ReConsolidateTransaction
{
    public class ReConsolidateTransactionHandler(AppDbContext appDbContext) : IRequestHandler<ReConsolidateTransactionRequest, Unit>
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<Unit> Handle(ReConsolidateTransactionRequest request, CancellationToken cancellationToken)
        {
            var transaction = await _appDbContext.ConsolidatedTransactionHistories.Include(c => c.CashStatement).Where(c => c.TransactionId == request.TransactionId).FirstOrDefaultAsync(cancellationToken);

            if(request.OldAmount != request.NewAmount)
            {
                if (request.Type == "Income")
                {
                    transaction.CashStatement.Inflow -= request.OldAmount;
                }
                else if (request.Type == "Expense")
                {
                    transaction.CashStatement.Outflow -= request.OldAmount;
                }

                transaction.CashStatement.Balance = transaction.CashStatement.Inflow - transaction.CashStatement.Outflow;

                if (request.NewAmount > 0)
                {
                    if (request.Type == "Income")
                    {
                        transaction.CashStatement.Inflow += request.NewAmount;
                    }
                    else if (request.Type == "Expense")
                    {
                        transaction.CashStatement.Outflow += request.NewAmount;
                    }

                    transaction.CashStatement.Balance = transaction.CashStatement.Inflow - transaction.CashStatement.Outflow;
                }
            }

            if(request.Date != default && request.Date != transaction.CashStatement.OpeningDate)
            {
                if(request.Type == "Income")
                {
                    transaction.CashStatement.Inflow -= request.OldAmount;
                }
                else if(request.Type == "Expense")
                {
                    transaction.CashStatement.Outflow -= request.OldAmount;
                }

                transaction.CashStatement.Balance = transaction.CashStatement.Inflow - transaction.CashStatement.Outflow;

                var isOpen = await _appDbContext.CashStatements.AnyAsync(c => c.OpeningDate.Date == request.Date.Date, cancellationToken);
                var @newStatement = new CashStatement();

                if (!isOpen)
                {
                    @newStatement.OpenCash(request.Date);
                    @newStatement.AddTransaction(request.Type, request.NewAmount, request.Date);
                    _appDbContext.CashStatements.Add(@newStatement);
                }
                else
                {
                    @newStatement.AddTransaction(request.Type, request.NewAmount, request.Date);
                    _appDbContext.CashStatements.Update(@newStatement);
                }
            }

            _appDbContext.CashStatements.Update(transaction.CashStatement);
            _appDbContext.ConsolidatedTransactionHistories.Update(transaction);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
