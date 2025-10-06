using CommomTestsUtilities.Repositories;
using Flow.Application.UseCases.Consolidated;
using Flow.Core.Entities;
using Flow.Core.Repositories;
using Flow.Infra.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseCase.Tests.Consolidated
{
    public class GetDailyBalance
    {
        [Fact]
        public async Task Success()
        {
            var dailyBalance = new DailyBalance
            {
                Id = "123",
                Date = DateTime.Now.Date,
                TotalBalance = 1000
            };

            var getDailyBalanceUseCase = CreateUseCase(dailyBalance);

            var result = await getDailyBalanceUseCase.Execute(DateTime.Now.Date);

            Assert.Equal(dailyBalance.Date, result.Date);
            Assert.Equal(dailyBalance.TotalBalance, result.TotalBalance);
        }

        private static GetDailyBalanceUseCase CreateUseCase(DailyBalance dailyBalance)
        {
            var dailyBalanceRepository = new DailyBalanceRepositoryBuilder();
            dailyBalanceRepository.FindDailyBalanceByDay(dailyBalance);
            return new GetDailyBalanceUseCase(dailyBalanceRepository.Builder().Object);
        }
    }
}
