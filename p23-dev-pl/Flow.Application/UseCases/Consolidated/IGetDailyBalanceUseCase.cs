using Flow.Core.Entities;

namespace Flow.Application.UseCases.Consolidated
{
    public interface IGetDailyBalanceUseCase
    {
        Task<DailyBalance> Execute(DateTime date);
    }
}
