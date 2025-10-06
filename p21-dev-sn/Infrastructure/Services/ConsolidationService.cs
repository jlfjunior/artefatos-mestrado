using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Infrastructure.Services
{
    public class ConsolidationService : IConsolidationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ConsolidationService> _logger;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public ConsolidationService(ApplicationDbContext dbContext, ILogger<ConsolidationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;

            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: (ex, breakDelay) =>
                    {
                        _logger.LogWarning("Circuit breaker aberto: {0}. Duração: {1}", ex.Message, breakDelay);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker resetado.");
                    });
        }

        public async Task<ConsolidationReportResponse> GenerateDailyReportAsync(string date)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var transactions = await _dbContext.Transactions
                    .Where(t => t.Date == date)
                    .ToListAsync();

                decimal totalCredits = transactions.Where(t => t.Type.Equals("Credit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
                decimal totalDebits = transactions.Where(t => t.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);

                var report = new ConsolidationReportResponse
                {
                    Date = date,
                    TotalCredits = totalCredits,
                    TotalDebits = totalDebits,
                    DailyBalance = totalCredits - totalDebits
                };

                _logger.LogInformation("Relatório consolidado gerado para a data {date}", date);
                return report;
            });
        }
    }
}