
using Flow.Core.Entities;
using Flow.Core.Repositories;
using Flow.Infra.Data;
using MongoDB.Driver;

namespace Flow.Application.UseCases.Consolidated
{
    public class GetDailyBalanceUseCase : IGetDailyBalanceUseCase
    {
        private readonly IDailyBalanceRepository _dailyBalanceRepository;
        public GetDailyBalanceUseCase(IDailyBalanceRepository dailyBalanceRepository)
        {
            _dailyBalanceRepository = dailyBalanceRepository;
        }
        public async Task<DailyBalance> Execute(DateTime date)
        {
            var dailyBalance = await _dailyBalanceRepository.FindDailyBalanceByDay(date.Date);
            return dailyBalance;
        }
    }
}
