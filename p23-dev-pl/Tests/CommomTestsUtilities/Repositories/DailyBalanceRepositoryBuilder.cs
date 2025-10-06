using Flow.Core.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommomTestsUtilities.Repositories
{
    public class DailyBalanceRepositoryBuilder
    {
        private readonly Mock<IDailyBalanceRepository> _dailyBalanceRepository;

        public DailyBalanceRepositoryBuilder() => _dailyBalanceRepository = new Mock<IDailyBalanceRepository>();

        public void FindDailyBalanceByDay(Flow.Core.Entities.DailyBalance dailyBalance) => _dailyBalanceRepository
            .Setup(repo => repo.FindDailyBalanceByDay(It.IsAny<DateTime>()))
            .ReturnsAsync(dailyBalance);

        public Mock<IDailyBalanceRepository> Builder() => _dailyBalanceRepository;

    }
}
