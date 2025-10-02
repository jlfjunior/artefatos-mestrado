using Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Application.Transaction.Handlers.GetTransactionById;

namespace Application.Transaction.Handlers;

public class GetTransactionById(IApplicationDbContext _context, IMapper _mapper) : IRequestHandler<GetTransactionByIdQuery, TransactionDTO?>
{
    public record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionDTO?>;

    public async Task<TransactionDTO?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return transaction is null ? null : _mapper.Map<TransactionDTO>(transaction);
    }
}