using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CashFlowService : ICashFlowService
    {
        private readonly ApplicationDbContext _dbContext;

        public CashFlowService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var transaction = new Transaction
            {
                Date = request.Date,
                Amount = request.Amount,
                Type = request.Type,
                Description = request.Description
            };

            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return new TransactionResponse
            {
                TransactionId = transaction.Id,
                Date = transaction.Date,
                Amount = transaction.Amount,
                Type = transaction.Type,
                Description = transaction.Description
            };
        }

        public async Task<IEnumerable<TransactionResponse>> GetTransactionsByDateAsync(string date)
        {
            var transactions = await _dbContext.Transactions
                .Where(t => t.Date == date)
                .ToListAsync();

            return transactions.Select(t => new TransactionResponse
            {
                TransactionId = t.Id,
                Date = t.Date,
                Amount = t.Amount,
                Type = t.Type,
                Description = t.Description
            });
        }
    }
}