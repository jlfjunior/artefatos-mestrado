using Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Application.Transaction.Handlers.GetTransactions;

namespace Application.Transaction.Handlers;

public class GetTransactions(IApplicationDbContext _context, IMapper _mapper) : IRequestHandler<GetTransactionsQuery, List<TransactionDTO>>
{
    public record GetTransactionsQuery() : IRequest<List<TransactionDTO>>;

    public async Task<List<TransactionDTO>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsQueryable();

        var transactions = await query
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<TransactionDTO>>(transactions);
    }
}