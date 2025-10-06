using Flow.Core.Entities;
using Flow.Core.Repositories;
using Flow.Infra.Data;
using MongoDB.Driver;

namespace Flow.Infra.Repositories
{
    public class DailyBalanceRepository : IDailyBalanceRepository
    {
        private readonly MongoDbContext _context;
        public DailyBalanceRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<DailyBalance> FindDailyBalanceByDay(DateTime date)
        {
            var filter = Builders<DailyBalance>.Filter.And(
                Builders<DailyBalance>.Filter.Gte(d => d.Date, date.Date),
                Builders<DailyBalance>.Filter.Lt(d => d.Date, date.Date.AddDays(1)));

            return await _context.DailyBalance.Find(filter).FirstOrDefaultAsync();
        }
    }
}
