using MediatR;
using Transactions.Business.Model;

namespace Transactions.Business.UseCase
{
    public record TransactionInputDto(string Description, Double Value, TransactionType type) :IRequest { }
}

