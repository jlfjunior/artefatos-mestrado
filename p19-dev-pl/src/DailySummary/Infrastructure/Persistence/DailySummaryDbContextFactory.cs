using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class DailySummaryDbContextFactory : IDesignTimeDbContextFactory<DailySummaryDbContext>
{
    public DailySummaryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DailySummaryDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5431;Database=verity_daily_summary;Username=admin;Password=admin");
        return new DailySummaryDbContext(optionsBuilder.Options);
    }
}
