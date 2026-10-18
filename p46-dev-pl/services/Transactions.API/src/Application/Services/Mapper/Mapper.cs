using Shared.DTOs;
using Shared.Enums;
using Transactions.API.Domain.Models;

namespace Transactions.API.src.Application.Services.Mapper
{
    public static class Mapper
    {
        public static TransactionDto MapToDto(this Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                Lojista = transaction.Lojista,
                LojistaCodigo = transaction.Lojista.Codigo(),
                LojaNome = transaction.Lojista.NomeLoja(),
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                TransactionDate = transaction.TransactionDate,
                CreatedAt = transaction.CreatedAt,
                IsProcessed = transaction.IsProcessed
            };
        }
    }
}
