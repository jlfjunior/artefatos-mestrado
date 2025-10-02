using Cashflow.Management.Data.AppContext;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Management.Test.Helpers
{
    public static class DataHelper
    {
        public static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"DbTest{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }
    }
}
